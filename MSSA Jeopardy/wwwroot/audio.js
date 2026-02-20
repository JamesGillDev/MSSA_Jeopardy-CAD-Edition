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
        msg.rate = 1;
        msg.pitch = 1;
        msg.volume = 1;
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
    } catch {
        hasPlayed = false;
    }

    if (!hasPlayed) {
        window.addEventListener("click", onUserGesture, { once: true, passive: true });
        window.addEventListener("keydown", onUserGesture, { once: true, passive: true });
        window.addEventListener("touchstart", onUserGesture, { once: true, passive: true });
    }
};
