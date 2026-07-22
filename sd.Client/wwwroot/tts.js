window.tts = {
    speak: (text, lang = "de-DE", rate = 1, pitch = 1, volume = 1) => {
        if (!("speechSynthesis" in window)) {
            console.warn("SpeechSynthesis not supported in this browser.");
            return;
        }

        window.speechSynthesis.cancel(); // stop previous speech

        const u = new SpeechSynthesisUtterance(text);
        u.lang = lang;
        u.rate = rate;
        u.pitch = pitch;
        u.volume = volume;

        // Try to pick a voice that matches lang (optional)
        const voices = window.speechSynthesis.getVoices?.() ?? [];
        const match = voices.find(v => (v.lang || "").toLowerCase().startsWith(lang.toLowerCase().slice(0, 2)));
        if (match) u.voice = match;

        window.speechSynthesis.speak(u);
    },

    stop: () => {
        if ("speechSynthesis" in window) window.speechSynthesis.cancel();
    }
};
