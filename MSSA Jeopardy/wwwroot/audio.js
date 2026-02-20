window.playWelcomeVoice = function () {
    let hasPlayed = false;
    let customVoiceAudio = null;
    const customVoicePath = "/audio/welcome-voice.mp3";

    const deepNameHints = [
        "guy",
        "davis",
        "david",
        "roger",
        "brian",
        "james",
        "george",
        "male"
    ];

    const feminineNameHints = [
        "zira",
        "aria",
        "jenny",
        "sara",
        "hazel",
        "libby",
        "emma",
        "female"
    ];

    const removeGestureListeners = () => {
        window.removeEventListener("click", onUserGesture);
        window.removeEventListener("keydown", onUserGesture);
        window.removeEventListener("touchstart", onUserGesture);
    };

    const pickPreferredVoice = (voices) => {
        if (!voices || voices.length === 0) {
            return null;
        }

        const englishVoices = voices.filter(v => (v.lang || "").toLowerCase().startsWith("en"));
        const candidates = englishVoices.length > 0 ? englishVoices : voices;

        const scoreVoice = (voice) => {
            const name = (voice.name || "").toLowerCase();
            const lang = (voice.lang || "").toLowerCase();
            let score = 0;

            if (lang.startsWith("en-us")) {
                score += 25;
            } else if (lang.startsWith("en-gb") || lang.startsWith("en-ca") || lang.startsWith("en-au")) {
                score += 15;
            }

            if (name.includes("natural") || name.includes("neural") || name.includes("online")) {
                score += 40;
            }

            if (name.includes("desktop")) {
                score -= 20;
            }

            if (deepNameHints.some(hint => name.includes(hint))) {
                score += 35;
            }

            if (feminineNameHints.some(hint => name.includes(hint))) {
                score -= 30;
            }

            if (voice.localService === false) {
                score += 8;
            }

            return score;
        };

        return candidates
            .map(v => ({ voice: v, score: scoreVoice(v) }))
            .sort((a, b) => b.score - a.score)[0]?.voice ?? null;
    };

    const tryPlayCustomVoice = async () => {
        if (hasPlayed) {
            return true;
        }

        try {
            if (!customVoiceAudio) {
                customVoiceAudio = new Audio(customVoicePath);
                customVoiceAudio.preload = "auto";
                customVoiceAudio.volume = 1;
            }

            customVoiceAudio.currentTime = 0;
            await customVoiceAudio.play();
            hasPlayed = true;
            removeGestureListeners();
            return true;
        } catch {
            return false;
        }
    };

    const speakFallback = () => {
        if (hasPlayed || !("speechSynthesis" in window)) {
            return false;
        }

        try {
            const msg = new SpeechSynthesisUtterance(
                "Welcome to MSSA Jeopardy... Test your knowledge, and play like a champion."
            );
            msg.rate = 0.82;
            msg.pitch = 0.72;
            msg.volume = 1;
            msg.lang = "en-US";

            const voices = window.speechSynthesis.getVoices();
            const preferredVoice = pickPreferredVoice(voices);
            if (preferredVoice) {
                msg.voice = preferredVoice;
                msg.lang = preferredVoice.lang || "en-US";
            }

            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(msg);
            hasPlayed = true;
            removeGestureListeners();
            return true;
        } catch {
            return false;
        }
    };

    const speakFallbackWhenVoicesReady = () => {
        if (hasPlayed || !("speechSynthesis" in window)) {
            return;
        }

        const voices = window.speechSynthesis.getVoices();
        if (voices.length > 0) {
            speakFallback();
            return;
        }

        let completed = false;
        const finish = () => {
            if (completed || hasPlayed) {
                return;
            }
            completed = true;
            speakFallback();
        };

        const onVoicesChanged = () => {
            if (window.speechSynthesis.getVoices().length > 0) {
                finish();
                if (typeof window.speechSynthesis.removeEventListener === "function") {
                    window.speechSynthesis.removeEventListener("voiceschanged", onVoicesChanged);
                } else {
                    window.speechSynthesis.onvoiceschanged = null;
                }
            }
        };

        if (typeof window.speechSynthesis.addEventListener === "function") {
            window.speechSynthesis.addEventListener("voiceschanged", onVoicesChanged);
        } else {
            window.speechSynthesis.onvoiceschanged = onVoicesChanged;
        }

        setTimeout(finish, 1000);
    };

    const attemptVoicePlayback = async () => {
        const customPlayed = await tryPlayCustomVoice();
        if (!customPlayed) {
            speakFallbackWhenVoicesReady();
        }
    };

    const onUserGesture = () => {
        void attemptVoicePlayback();
    };

    window.addEventListener("click", onUserGesture);
    window.addEventListener("keydown", onUserGesture);
    window.addEventListener("touchstart", onUserGesture);

    void attemptVoicePlayback();
};

window.jeopardyFocusElement = function (element) {
    if (!element || typeof element.focus !== "function") {
        return;
    }

    // Delay one frame to ensure the modal is painted before focus.
    requestAnimationFrame(() => element.focus({ preventScroll: true }));
};

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
                tone({ frequency: 880, duration: 0.05, type: "triangle", gain: 0.05 });
                break;
            case "scene-open":
                tone({ frequency: 294, toFrequency: 392, duration: 0.14, type: "triangle", gain: 0.06 });
                tone({ at: 0.12, frequency: 523, duration: 0.1, type: "triangle", gain: 0.06 });
                break;
            case "close":
                tone({ frequency: 420, toFrequency: 260, duration: 0.09, type: "triangle", gain: 0.06 });
                break;
            case "clue-open":
                tone({ frequency: 520, duration: 0.08, type: "triangle", gain: 0.06 });
                tone({ at: 0.06, frequency: 690, duration: 0.08, type: "triangle", gain: 0.06 });
                break;
            case "buzz":
                tone({ frequency: 240, duration: 0.08, type: "square", gain: 0.055 });
                tone({ at: 0.08, frequency: 280, duration: 0.08, type: "square", gain: 0.05 });
                break;
            case "answer-correct":
                tone({ frequency: 392, duration: 0.12, type: "triangle", gain: 0.07 });
                tone({ at: 0.1, frequency: 494, duration: 0.12, type: "triangle", gain: 0.07 });
                tone({ at: 0.2, frequency: 587, duration: 0.16, type: "triangle", gain: 0.08 });
                break;
            case "answer-wrong":
                tone({ frequency: 330, toFrequency: 250, duration: 0.14, type: "sawtooth", gain: 0.065 });
                tone({ at: 0.12, frequency: 230, toFrequency: 170, duration: 0.16, type: "sawtooth", gain: 0.06 });
                break;
            case "countdown-tick":
                tone({ frequency: 1220, duration: 0.04, type: "square", gain: 0.035 });
                break;
            case "time-up":
                tone({ frequency: 180, toFrequency: 120, duration: 0.22, type: "sawtooth", gain: 0.08 });
                tone({ at: 0.14, frequency: 150, toFrequency: 105, duration: 0.22, type: "sawtooth", gain: 0.08 });
                break;
            case "winner-fanfare":
                tone({ frequency: 392, duration: 0.14, type: "triangle", gain: 0.07 });
                tone({ at: 0.1, frequency: 523, duration: 0.14, type: "triangle", gain: 0.075 });
                tone({ at: 0.2, frequency: 659, duration: 0.18, type: "triangle", gain: 0.085 });
                tone({ at: 0.33, frequency: 784, duration: 0.24, type: "triangle", gain: 0.09 });
                break;
            default:
                tone({ frequency: 880, duration: 0.05, type: "triangle", gain: 0.05 });
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
