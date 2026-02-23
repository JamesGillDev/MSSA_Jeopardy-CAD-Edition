window.playWelcomeVoice = async function () {
    try {
        const clip = new Audio("/audio/welcome-voice.mp3?v=20260223a");
        clip.preload = "auto";
        clip.volume = 1;
        clip.currentTime = 0;
        await clip.play();
        return true;
    } catch {
        return false;
    }
};
