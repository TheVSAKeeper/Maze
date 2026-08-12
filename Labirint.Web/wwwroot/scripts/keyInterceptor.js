const keyDownHandlers = new Map();

const isEditableTarget = target => {
    if (!target) {
        return false;
    }

    if (target.isContentEditable) {
        return true;
    }

    const tag = target.tagName;

    return tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
};

const isModalOpen = () => document.querySelector(".sheet--open, .ui-dialog-backdrop") !== null;

window.initializeKeyInterceptor = (dotNetHelper, id) => {
    window.finalizeKeyInterceptor(id);

    const handler = e => {
        if (isEditableTarget(e.target) || isModalOpen()) {
            return;
        }

        dotNetHelper.invokeMethodAsync('OnKeyDown', e.code);
    };

    keyDownHandlers.set(id, handler);
    document.addEventListener("keydown", handler);
};

window.finalizeKeyInterceptor = id => {
    const handler = keyDownHandlers.get(id);

    if (!handler) {
        return;
    }

    document.removeEventListener("keydown", handler);
    keyDownHandlers.delete(id);
};
