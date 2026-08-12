window.labirintDialog = (() => {
    const focusable = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

    let previous = null;
    let trapped = null;

    const items = () => [...trapped.querySelectorAll(focusable)].filter(item => item.offsetParent !== null);

    const onKeyDown = event => {
        if (event.key !== 'Tab' || trapped === null) {
            return;
        }

        const reachable = items();

        if (reachable.length === 0) {
            event.preventDefault();
            trapped.focus();
            return;
        }

        const first = reachable[0];
        const last = reachable[reachable.length - 1];

        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    };

    return {
        trap: element => {
            if (trapped === null) {
                previous = document.activeElement;
                document.addEventListener('keydown', onKeyDown, true);
            }

            trapped = element;
            document.body.style.overflow = 'hidden';

            const reachable = items();
            (reachable.length > 0 ? reachable[0] : element).focus();
        },
        release: () => {
            trapped = null;
            document.body.style.overflow = '';
            document.removeEventListener('keydown', onKeyDown, true);

            if (previous !== null && previous.isConnected) {
                previous.focus();
            }

            previous = null;
        },
    };
})();
