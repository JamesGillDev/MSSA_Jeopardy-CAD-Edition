window.playWelcomeVoice = function () {
    if (!('speechSynthesis' in window)) return;
    var msg = new SpeechSynthesisUtterance("Welcome to MSSA Jeopardy! Test your knowledge and learn well");
    msg.rate = 0.85; // slower for deep effect
    msg.pitch = 0.6; // lower pitch for deep voice
    msg.volume = 1;
    // Try to pick a male/en-US voice if available
    var voices = window.speechSynthesis.getVoices();
    var preferred = voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('male'))
        || voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('freeman'))
        || voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('barry'))
        || voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('david'))
        || voices.find(v => v.lang.startsWith('en') && v.name.toLowerCase().includes('english'))
        || voices.find(v => v.lang.startsWith('en'));
    if (preferred) msg.voice = preferred;
    window.speechSynthesis.speak(msg);
};
