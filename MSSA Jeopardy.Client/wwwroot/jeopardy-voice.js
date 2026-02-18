window.playWelcomeJeopardyVoice = function () {
    if (!window.speechSynthesis) return;
    var msg = new SpeechSynthesisUtterance("Welcome to MSSA Jeopardy! Test your knowledge and pick 6 categories to play");
    msg.rate = 0.85;
    msg.pitch = 0.7;
    msg.volume = 1;
    // Try to pick a deep/sultry male voice if available
    var voices = window.speechSynthesis.getVoices();
    var preferred = voices.find(v => v.name.toLowerCase().includes("barry") || v.name.toLowerCase().includes("matthew") || (v.gender && v.gender.toLowerCase() === "male"));
    if (preferred) msg.voice = preferred;
    else if (voices.length > 0) msg.voice = voices[0];
    window.speechSynthesis.speak(msg);
};
