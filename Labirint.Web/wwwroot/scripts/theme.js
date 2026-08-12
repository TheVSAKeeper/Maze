window.labirintTheme = (() => {
    const storageKey = 'IsDarkMod';
    const barColors = { dark: '#14110d', light: '#e9e2d2' };

    const apply = theme => {
        document.documentElement.setAttribute('data-theme', theme);

        const bar = document.querySelector('meta[name="theme-color"]');

        if (bar !== null) {
            bar.setAttribute('content', barColors[theme]);
        }
    };

    return {
        apply: apply,
        applyStored: () => {
            let isDark = true;

            try {
                const stored = localStorage.getItem(storageKey);
                isDark = stored === null ? true : JSON.parse(stored) !== false;
            } catch {
                isDark = true;
            }

            apply(isDark ? 'dark' : 'light');
        },
    };
})();
