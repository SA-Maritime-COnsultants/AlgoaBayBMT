let listenerRegistry = new Map();

function resolveElement(elementId) {
    return elementId ? document.getElementById(elementId) : null;
}

function getFullscreenElement() {
    return document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement || null;
}

async function requestFullscreen(element) {
    if (!element) {
        return false;
    }

    const request = element.requestFullscreen
        || element.webkitRequestFullscreen
        || element.msRequestFullscreen;

    if (!request) {
        return false;
    }

    await request.call(element);
    return true;
}

async function exitFullscreen() {
    const exit = document.exitFullscreen
        || document.webkitExitFullscreen
        || document.msExitFullscreen;

    if (!exit) {
        return false;
    }

    await exit.call(document);
    return true;
}

export async function toggleCoursePlayerFullscreen(elementId) {
    const element = resolveElement(elementId);
    if (!element) {
        return false;
    }

    const fullscreenElement = getFullscreenElement();
    if (fullscreenElement === element) {
        return await exitFullscreen();
    }

    return await requestFullscreen(element);
}

export async function exitCoursePlayerFullscreen(elementId) {
    const element = resolveElement(elementId);
    if (!element) {
        return false;
    }

    const fullscreenElement = getFullscreenElement();
    if (fullscreenElement !== element) {
        return false;
    }

    return await exitFullscreen();
}

export function isFullscreenSupported() {
    return !!(document.fullscreenEnabled || document.webkitFullscreenEnabled || document.msFullscreenEnabled);
}

export async function isElementFullscreen(elementId) {
    const element = resolveElement(elementId);
    return !!element && getFullscreenElement() === element;
}

export function registerFullscreenChange(dotNetRef, elementId) {
    unregisterFullscreenChange(elementId);

    const handler = async () => {
        const element = resolveElement(elementId);
        const isFullscreen = !!element && getFullscreenElement() === element;
        try {
            await dotNetRef.invokeMethodAsync('OnFullscreenChanged', isFullscreen);
        }
        catch {
        }
    };

    document.addEventListener('fullscreenchange', handler);
    document.addEventListener('webkitfullscreenchange', handler);
    document.addEventListener('msfullscreenchange', handler);

    listenerRegistry.set(elementId, { handler, dotNetRef });
}

export function unregisterFullscreenChange(elementId) {
    const registration = listenerRegistry.get(elementId);
    if (!registration) {
        return;
    }

    document.removeEventListener('fullscreenchange', registration.handler);
    document.removeEventListener('webkitfullscreenchange', registration.handler);
    document.removeEventListener('msfullscreenchange', registration.handler);
    listenerRegistry.delete(elementId);
}
