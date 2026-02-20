window.playWelcomeVoice = function () {
    const storageKey = "mssa_jeopardy_welcome_played";

    if (!("speechSynthesis" in window)) {
        return;
    }

    if (sessionStorage.getItem(storageKey) === "1") {
        return;
    }

    let hasPlayed = false;

    const speak = () => {
        if (hasPlayed) {
            return;
        }

        hasPlayed = true;
        sessionStorage.setItem(storageKey, "1");

        const msg = new SpeechSynthesisUtterance(
            "Welcome to MSSA Jeopardy. Test your knowledge and learn well."
        );
        msg.rate = 0.85;
        msg.pitch = 0.5;
        msg.volume = 1;

        // Try to select a deep male voice
        const voices = window.speechSynthesis.getVoices();
        msg.voice = voices.find(v => v.name.toLowerCase().includes("david") || (v.gender === "male" && v.lang.startsWith("en"))) || voices.find(v => v.lang.startsWith("en") && v.gender === "male") || voices[0];

        window.speechSynthesis.cancel();
        window.speechSynthesis.speak(msg);
    };

    const onUserGesture = () => {
        speak();
        window.removeEventListener("click", onUserGesture);
        window.removeEventListener("keydown", onUserGesture);
        window.removeEventListener("touchstart", onUserGesture);
    };

    // Try immediately first for browsers that allow non-gesture speech.
    try {
        speak();
    } catch {}

    window.addEventListener("click", onUserGesture);
    window.addEventListener("keydown", onUserGesture);
    window.addEventListener("touchstart", onUserGesture);
};
