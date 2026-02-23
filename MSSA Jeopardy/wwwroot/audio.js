(() => {
    const audioVersion = "20260223a";
    const versioned = (path) => `${path}?v=${audioVersion}`;

    const clipCatalog = Object.freeze({
        "welcome-voice": [versioned("/audio/welcome-voice.mp3")],
        // The uploaded recordings are currently named opposite of the button intent.
        "board-setup": [versioned("/audio/add-player.mp3")],
        "add-player": [versioned("/audio/board-setup.mp3")],
        "daily-double": [versioned("/audio/jeopardy-daily-double.mp3")],
        "jeopardy-daily-double": [versioned("/audio/jeopardy-daily-double.mp3")],
        "start-game-1": [versioned("/audio/start-game-1.mp3")],
        "start-game-2": [versioned("/audio/start-game-2.mp3")],
        "start-game-3": [versioned("/audio/start-game-3.mp3")],
        "start-game-4": [versioned("/audio/start-game-4.mp3")],
        "stary-game-4": [versioned("/audio/start-game-4.mp3")],
        "start-game-random": [
            versioned("/audio/start-game-1.mp3"),
            versioned("/audio/start-game-2.mp3"),
            versioned("/audio/start-game-3.mp3"),
            versioned("/audio/start-game-4.mp3")
        ],
        "winner-1": [versioned("/audio/winner-1.mp3")],
        "winner-2": [versioned("/audio/winner-2.mp3")],
        "winner-3": [versioned("/audio/winner-3.mp3")],
        "winnder-3": [versioned("/audio/winner-3.mp3")],
        "winner-random": [
            versioned("/audio/winner-1.mp3"),
            versioned("/audio/winner-2.mp3"),
            versioned("/audio/winner-3.mp3")
        ]
    });

    const clipCache = new Map();
    const sessionKeyPrefix = "mssa-jeopardy:clip:";
    const welcomeCooldownMs = 1200;
    let lastWelcomePlayMs = 0;
    let welcomeGestureHandler = null;

    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

    const randomItem = (items) => {
        if (!Array.isArray(items) || items.length === 0) {
            return null;
        }

        if (items.length === 1) {
            return items[0];
        }

        return items[Math.floor(Math.random() * items.length)];
    };

    const resolveClipSource = (name) => {
        const key = String(name || "").trim().toLowerCase();
        if (!key) {
            return null;
        }

        const candidates = clipCatalog[key];
        return randomItem(candidates);
    };

    const getClip = (source) => {
        let clip = clipCache.get(source);
        if (clip) {
            return clip;
        }

        clip = new Audio(source);
        clip.preload = "auto";
        clipCache.set(source, clip);
        return clip;
    };

    const sessionStorageSafe = () => {
        try {
            return window.sessionStorage;
        } catch {
            return null;
        }
    };

    const playSource = async (source, options = {}) => {
        if (!source) {
            return false;
        }

        const clip = getClip(source);
        const restart = options.restart !== false;
        const volume = clamp(Number(options.volume ?? 1), 0, 1);

        clip.volume = volume;

        if (restart) {
            try {
                clip.pause();
                clip.currentTime = 0;
            } catch {
                // Ignore reset failures and still attempt playback.
            }
        }

        try {
            await clip.play();
            return true;
        } catch {
            return false;
        }
    };

    const unbindWelcomeGestureFallback = () => {
        if (!welcomeGestureHandler) {
            return;
        }

        window.removeEventListener("pointerdown", welcomeGestureHandler);
        window.removeEventListener("keydown", welcomeGestureHandler);
        window.removeEventListener("touchstart", welcomeGestureHandler);
        welcomeGestureHandler = null;
    };

    const bindWelcomeGestureFallback = () => {
        if (welcomeGestureHandler) {
            return;
        }

        welcomeGestureHandler = async () => {
            const now = Date.now();
            if (now - lastWelcomePlayMs < welcomeCooldownMs) {
                unbindWelcomeGestureFallback();
                return;
            }

            const played = await playSource(versioned("/audio/welcome-voice.mp3"), { restart: true, volume: 1 });
            if (played) {
                lastWelcomePlayMs = Date.now();
                unbindWelcomeGestureFallback();
            }
        };

        window.addEventListener("pointerdown", welcomeGestureHandler);
        window.addEventListener("keydown", welcomeGestureHandler);
        window.addEventListener("touchstart", welcomeGestureHandler);
    };

    const playClipByName = async (name, options = {}) => {
        const source = resolveClipSource(name);
        if (!source) {
            return false;
        }

        const oncePerSession = options.oncePerSession === true;
        const storage = oncePerSession ? sessionStorageSafe() : null;
        const rawSessionKey = String(options.sessionKey || name || "").trim().toLowerCase();
        const sessionKey = rawSessionKey ? `${sessionKeyPrefix}${rawSessionKey}` : null;

        if (oncePerSession && storage && sessionKey && storage.getItem(sessionKey) === "1") {
            return false;
        }

        const played = await playSource(source, options);
        if (played && oncePerSession && storage && sessionKey) {
            storage.setItem(sessionKey, "1");
        }

        return played;
    };

    window.playWelcomeVoice = async function () {
        const now = Date.now();
        if (now - lastWelcomePlayMs < welcomeCooldownMs) {
            return false;
        }

        const played = await playSource(versioned("/audio/welcome-voice.mp3"), { restart: true, volume: 1 });
        if (played) {
            lastWelcomePlayMs = Date.now();
            unbindWelcomeGestureFallback();
            return true;
        }

        bindWelcomeGestureFallback();
        return false;
    };

    window.jeopardyPlayClip = async function (name, options) {
        return playClipByName(name, options || {});
    };

    window.playBoardSetupVoice = async function () {
        return playSource(versioned("/audio/add-player.mp3"), { restart: true, volume: 1 });
    };

    window.jeopardyFocusElement = function (element) {
        if (!element || typeof element.focus !== "function") {
            return;
        }

        // Delay one frame to ensure the modal is painted before focus.
        requestAnimationFrame(() => element.focus({ preventScroll: true }));
    };
})();

(() => {
    const sfx = {
        context: null,
        gain: null,
        muted: false,
        volume: 0.8,
        unlocked: false,
        unlockBound: false
    };

    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

    const ensureAudio = () => {
        if (sfx.context) {
            return;
        }

        const AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) {
            return;
        }

        sfx.context = new AudioCtx();
        sfx.gain = sfx.context.createGain();
        sfx.gain.gain.value = 0;
        sfx.gain.connect(sfx.context.destination);
        applyGain();
    };

    const applyGain = () => {
        if (!sfx.context || !sfx.gain) {
            return;
        }

        const target = sfx.muted ? 0 : clamp(sfx.volume, 0, 1);
        sfx.gain.gain.setTargetAtTime(target, sfx.context.currentTime, 0.02);
    };

    const unlockAudio = () => {
        ensureAudio();
        if (!sfx.context) {
            return;
        }

        sfx.context.resume().then(() => {
            sfx.unlocked = true;
            applyGain();
        }).catch(() => {
            // Ignore resume failures; UI should keep working.
        });
    };

    const bindUnlockHandlers = () => {
        if (sfx.unlockBound) {
            return;
        }

        sfx.unlockBound = true;
        const unlock = () => {
            unlockAudio();
            window.removeEventListener("pointerdown", unlock);
            window.removeEventListener("keydown", unlock);
            window.removeEventListener("touchstart", unlock);
        };

        window.addEventListener("pointerdown", unlock, { once: true });
        window.addEventListener("keydown", unlock, { once: true });
        window.addEventListener("touchstart", unlock, { once: true });
    };

    const tone = ({ at = 0, duration = 0.08, frequency = 440, toFrequency = null, type = "sine", gain = 0.12 }) => {
        if (!sfx.context || !sfx.gain) {
            return;
        }

        const startTime = sfx.context.currentTime + at;
        const endTime = startTime + duration;
        const osc = sfx.context.createOscillator();
        const env = sfx.context.createGain();

        osc.type = type;
        osc.frequency.setValueAtTime(frequency, startTime);
        if (toFrequency !== null) {
            osc.frequency.exponentialRampToValueAtTime(Math.max(1, toFrequency), endTime);
        }

        env.gain.setValueAtTime(0.0001, startTime);
        env.gain.exponentialRampToValueAtTime(Math.max(0.0002, gain), startTime + Math.min(0.02, duration * 0.35));
        env.gain.exponentialRampToValueAtTime(0.0001, endTime);

        osc.connect(env);
        env.connect(sfx.gain);
        osc.start(startTime);
        osc.stop(endTime + 0.01);
    };

    const playPattern = (name) => {
        if (!sfx.context || !sfx.unlocked) {
            return;
        }

        switch ((name || "").toLowerCase()) {
            case "ui":
                tone({ frequency: 1180, duration: 0.045, type: "triangle", gain: 0.044 });
                tone({ at: 0.03, frequency: 1520, toFrequency: 1420, duration: 0.09, type: "sine", gain: 0.036 });
                break;
            case "scene-open":
                tone({ frequency: 523, duration: 0.09, type: "triangle", gain: 0.05 });
                tone({ at: 0.08, frequency: 659, duration: 0.1, type: "triangle", gain: 0.055 });
                tone({ at: 0.16, frequency: 784, duration: 0.13, type: "triangle", gain: 0.06 });
                break;
            case "close":
                tone({ frequency: 740, toFrequency: 520, duration: 0.1, type: "triangle", gain: 0.045 });
                break;
            case "clue-open":
                tone({ frequency: 1046, duration: 0.06, type: "sine", gain: 0.05 });
                tone({ at: 0.05, frequency: 1318, duration: 0.1, type: "triangle", gain: 0.054 });
                break;
            case "buzz":
                tone({ frequency: 290, duration: 0.08, type: "square", gain: 0.055 });
                tone({ at: 0.075, frequency: 330, duration: 0.08, type: "square", gain: 0.05 });
                break;
            case "answer-correct":
                tone({ frequency: 523, duration: 0.1, type: "triangle", gain: 0.06 });
                tone({ at: 0.09, frequency: 659, duration: 0.1, type: "triangle", gain: 0.065 });
                tone({ at: 0.18, frequency: 784, duration: 0.13, type: "triangle", gain: 0.07 });
                break;
            case "answer-wrong":
                tone({ frequency: 370, toFrequency: 290, duration: 0.11, type: "sawtooth", gain: 0.06 });
                tone({ at: 0.1, frequency: 280, toFrequency: 210, duration: 0.13, type: "sawtooth", gain: 0.058 });
                break;
            case "countdown-tick":
                tone({ frequency: 1280, duration: 0.04, type: "square", gain: 0.035 });
                break;
            case "time-up":
                tone({ frequency: 210, toFrequency: 135, duration: 0.2, type: "sawtooth", gain: 0.08 });
                tone({ at: 0.13, frequency: 180, toFrequency: 120, duration: 0.2, type: "sawtooth", gain: 0.078 });
                break;
            case "winner-fanfare":
                tone({ frequency: 523, duration: 0.12, type: "triangle", gain: 0.065 });
                tone({ at: 0.1, frequency: 659, duration: 0.13, type: "triangle", gain: 0.07 });
                tone({ at: 0.2, frequency: 784, duration: 0.15, type: "triangle", gain: 0.075 });
                tone({ at: 0.32, frequency: 988, duration: 0.2, type: "triangle", gain: 0.078 });
                break;
            default:
                tone({ frequency: 1180, duration: 0.045, type: "triangle", gain: 0.044 });
                tone({ at: 0.03, frequency: 1520, toFrequency: 1420, duration: 0.09, type: "sine", gain: 0.036 });
                break;
        }
    };

    window.jeopardyAudioInit = function () {
        ensureAudio();
        bindUnlockHandlers();
        unlockAudio();
    };

    window.jeopardySetMute = function (isMuted) {
        ensureAudio();
        sfx.muted = !!isMuted;
        applyGain();
    };

    window.jeopardySetVolume = function (volume) {
        ensureAudio();
        sfx.volume = clamp(Number(volume) || 0, 0, 1);
        applyGain();
    };

    window.jeopardyPlaySfx = function (name) {
        ensureAudio();
        if (!sfx.unlocked) {
            unlockAudio();
        }
        playPattern(name);
    };
})();
