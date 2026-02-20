window.playWelcomeVoice = function () {
    if (!("speechSynthesis" in window)) {
        return;
    }

    let hasPlayed = false;

    const pickPreferredVoice = (voices) => {
        if (!voices || voices.length === 0) {
            return null;
        }

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

    const speak = () => {
        if (hasPlayed) {
            return;
        }

        hasPlayed = true;

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
    };

    const speakWhenVoicesReady = () => {
        const voices = window.speechSynthesis.getVoices();
        if (voices.length > 0) {
            speak();
            return;
        }

        let completed = false;
        const finish = () => {
            if (completed) {
                return;
            }
            completed = true;
            speak();
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

    const onUserGesture = () => {
        speakWhenVoicesReady();
        window.removeEventListener("click", onUserGesture);
        window.removeEventListener("keydown", onUserGesture);
        window.removeEventListener("touchstart", onUserGesture);
    };

    // Try immediately first for browsers that allow non-gesture speech.
    try {
        speakWhenVoicesReady();
    } catch {}

    window.addEventListener("click", onUserGesture);
    window.addEventListener("keydown", onUserGesture);
    window.addEventListener("touchstart", onUserGesture);
};
