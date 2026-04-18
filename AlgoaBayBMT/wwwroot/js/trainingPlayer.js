// trainingPlayer.js - ES module for Blazor training course player
// Handles HTML5 video, YouTube IFrame API, and Vimeo Player SDK

const _players = {};
let _ytApiReady = null;
let _vimeoApiReady = null;

// ─── URL type detection ────────────────────────────────────────────────────

export function detectVideoType(url) {
    if (!url) return 'html5';
    if (/youtube\.com|youtu\.be/.test(url)) return 'youtube';
    if (/vimeo\.com/.test(url)) return 'vimeo';
    return 'html5';
}

function extractYouTubeId(url) {
    const match = url.match(/(?:v=|youtu\.be\/)([^&?/\s]+)/);
    return match ? match[1] : null;
}

// ─── Lazy API loading ──────────────────────────────────────────────────────

function ensureYouTubeApi() {
    if (_ytApiReady) return _ytApiReady;
    _ytApiReady = new Promise((resolve) => {
        if (window.YT && window.YT.Player) { resolve(); return; }
        const prev = window.onYouTubeIframeAPIReady;
        window.onYouTubeIframeAPIReady = () => { if (prev) prev(); resolve(); };
        if (!document.querySelector('script[src*="youtube.com/iframe_api"]')) {
            const tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            document.head.appendChild(tag);
        }
    });
    return _ytApiReady;
}

function ensureVimeoApi() {
    if (_vimeoApiReady) return _vimeoApiReady;
    _vimeoApiReady = new Promise((resolve, reject) => {
        if (window.Vimeo) { resolve(); return; }
        const tag = document.createElement('script');
        tag.src = 'https://player.vimeo.com/api/player.js';
        tag.onload = resolve;
        tag.onerror = reject;
        document.head.appendChild(tag);
    });
    return _vimeoApiReady;
}

// ─── HTML5 Video ───────────────────────────────────────────────────────────

function _attachHtml5(elementId, dotnetRef, onEnded, onTimeUpdate, onPlay, onPause, onMeta) {
    const el = document.getElementById(elementId);
    if (!el) {
        console.error('trainingPlayer: element not found:', elementId);
        return false;
    }

    destroyPlayer(elementId);

    let saveInterval = null;
    let lastReportedSecond = -1;
    let completionRaised = false;
    const notifyTimeUpdate = () => {
        const currentSecond = Math.floor(el.currentTime || 0);
        if (currentSecond === lastReportedSecond) {
            return;
        }

        lastReportedSecond = currentSecond;
        dotnetRef.invokeMethodAsync(onTimeUpdate, currentSecond).catch(() => {});
    };
    const raiseCompletedIfNeeded = () => {
        if (completionRaised) {
            return;
        }

        if (el.ended || (el.duration > 0 && el.currentTime >= el.duration - 1)) {
            completionRaised = true;
            dotnetRef.invokeMethodAsync(onEnded).catch(() => {});
        }
    };

    const handlers = {
        play: () => {
            if (onPlay) {
                dotnetRef.invokeMethodAsync(onPlay).catch(() => {});
            }

            if (saveInterval) {
                clearInterval(saveInterval);
            }

            saveInterval = setInterval(() => {
                if (!el.paused && !el.ended) {
                    notifyTimeUpdate();
                }
            }, 10000);

            const player = _players[elementId];
            if (player) {
                player.interval = saveInterval;
            }
        },
        pause: () => {
            if (saveInterval) {
                clearInterval(saveInterval);
                saveInterval = null;
            }

            const player = _players[elementId];
            if (player) {
                player.interval = null;
            }

            notifyTimeUpdate();

            if (onPause) {
                dotnetRef.invokeMethodAsync(onPause).catch(() => {});
            }

            raiseCompletedIfNeeded();
        },
        timeupdate: () => {
            notifyTimeUpdate();
            raiseCompletedIfNeeded();
        },
        ended: () => {
            if (saveInterval) {
                clearInterval(saveInterval);
                saveInterval = null;
            }

            const player = _players[elementId];
            if (player) {
                player.interval = null;
            }

            notifyTimeUpdate();
            raiseCompletedIfNeeded();
        },
        loadedmetadata: onMeta ? () => dotnetRef.invokeMethodAsync(onMeta, el.duration || 0).catch(() => {}) : null,
        error: (event) => console.error('trainingPlayer: html5 video error', event),
    };

    for (const [evt, fn] of Object.entries(handlers)) {
        if (fn) el.addEventListener(evt, fn);
    }

    _players[elementId] = { type: 'html5', el, handlers, interval: saveInterval };
    console.log('trainingPlayer: initialized html5 for', elementId, 'src:', el.src || el.currentSrc || '(none)');

    const playPromise = el.play();
    if (playPromise && typeof playPromise.catch === 'function') {
        playPromise.catch((error) => {
            console.warn('trainingPlayer: autoplay was blocked or failed', error);
        });
    }

    return true;
}

export function prepareVideoPlayer(elementId) {
    const el = document.getElementById(elementId);
    if (!el) {
        console.error('trainingPlayer: element not found:', elementId);
        return false;
    }

    const playPromise = el.play();
    if (playPromise && typeof playPromise.catch === 'function') {
        playPromise.catch((error) => {
            console.warn('trainingPlayer: autoplay was blocked or failed', error);
        });
    }

    return true;
}

export function getCurrentTime(elementId) {
    const el = document.getElementById(elementId);
    return el ? el.currentTime || 0 : 0;
}

export function getDuration(elementId) {
    const el = document.getElementById(elementId);
    return el ? el.duration || 0 : 0;
}

export function initAvatarPlayer(elementId, dotnetRef) {
    return _attachHtml5(elementId, dotnetRef,
        'OnAvatarEnded', 'OnAvatarTimeUpdate', null, null, 'OnAvatarMetadata');
}

// ─── YouTube ───────────────────────────────────────────────────────────────

export async function initYouTubePlayer(containerId, videoId, dotnetRef, isAvatar) {
    await ensureYouTubeApi();
    destroyPlayer(containerId);

    const onEndedMethod  = isAvatar ? 'OnAvatarEnded'      : 'OnVideoEnded';
    const onUpdateMethod = isAvatar ? 'OnAvatarTimeUpdate'  : 'OnVideoTimeUpdate';

    const player = new YT.Player(containerId, {
        videoId,
        playerVars: { autoplay: 1, controls: isAvatar ? 0 : 1, rel: 0, modestbranding: 1, playsinline: 1 },
        events: {
            onReady: (e) => e.target.playVideo(),
            onStateChange: (e) => {
                if (e.data === YT.PlayerState.ENDED) {
                    dotnetRef.invokeMethodAsync(onEndedMethod).catch(() => {});
                }
            }
        }
    });

    const intervalId = setInterval(() => {
        if (player.getCurrentTime) {
            dotnetRef.invokeMethodAsync(onUpdateMethod, player.getCurrentTime()).catch(() => {});
        }
    }, 5000);

    _players[containerId] = { type: 'youtube', player, intervalId };
}

// ─── Vimeo ─────────────────────────────────────────────────────────────────

export async function initVimeoPlayer(containerId, videoUrl, dotnetRef, isAvatar) {
    await ensureVimeoApi();
    destroyPlayer(containerId);

    const iframe = document.getElementById(containerId);
    if (!iframe) return;

    const player = new Vimeo.Player(iframe, { url: videoUrl, autoplay: true });

    const onEndedMethod  = isAvatar ? 'OnAvatarEnded'     : 'OnVideoEnded';
    const onUpdateMethod = isAvatar ? 'OnAvatarTimeUpdate' : 'OnVideoTimeUpdate';

    let lastReported = -6;
    player.on('timeupdate', (d) => {
        const now = Math.floor(d.seconds);
        if (now - lastReported >= 5) {
            lastReported = now;
            dotnetRef.invokeMethodAsync(onUpdateMethod, d.seconds).catch(() => {});
        }
    });
    player.on('ended', () => dotnetRef.invokeMethodAsync(onEndedMethod).catch(() => {}));

    _players[containerId] = { type: 'vimeo', player };
}

// ─── Seek / Mute ──────────────────────────────────────────────────────────

export function seekVideo(elementId, seconds) {
    const e = _players[elementId];
    if (!e) return;
    if (e.type === 'html5') {
        if (!seconds || seconds <= 0) {
            return;
        }

        const doSeek = () => {
            try {
                e.el.currentTime = seconds;
            } catch (error) {
                console.error('trainingPlayer: seek error', error);
            }
        };

        if (e.el.readyState >= 1) {
            doSeek();
        } else {
            e.el.addEventListener('loadedmetadata', doSeek, { once: true });
        }
    }
    else if (e.type === 'youtube' && e.player?.seekTo) e.player.seekTo(seconds, true);
    else if (e.type === 'vimeo')  e.player?.setCurrentTime(seconds);
}

export function muteVideo(elementId)   { const e = _players[elementId]; if (e?.type === 'html5') e.el.muted = true; }
export function unmuteVideo(elementId) { const e = _players[elementId]; if (e?.type === 'html5') e.el.muted = false; }

// ─── Destroy ───────────────────────────────────────────────────────────────

export function destroyPlayer(elementId) {
    const e = _players[elementId];
    if (!e) return;
    if (e.type === 'html5' && e.el) {
        if (e.interval) {
            clearInterval(e.interval);
        }
        for (const [evt, fn] of Object.entries(e.handlers)) {
            if (fn) e.el.removeEventListener(evt, fn);
        }
    } else if (e.type === 'youtube') {
        if (e.intervalId) clearInterval(e.intervalId);
        try { e.player?.destroy?.(); } catch {}
    } else if (e.type === 'vimeo') {
        try { e.player?.destroy?.(); } catch {}
    }
    delete _players[elementId];
}
