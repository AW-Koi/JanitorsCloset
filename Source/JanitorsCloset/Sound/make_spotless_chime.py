"""Synthesises Spotless_Chime.ogg: a soft glass bell. Needs numpy and ffmpeg on PATH."""
import subprocess
import sys
import tempfile
import wave

import numpy as np

out = sys.argv[1]
rate = 44100
length = 1.1
t = np.arange(int(rate * length)) / rate

# Fundamental, an inharmonic overtone and a faint shimmer partial, each with its own decay.
partials = [(1568.0, 1.00, 2.6), (3920.0, 0.35, 4.5), (6272.0, 0.12, 7.0)]
signal = sum(amp * np.sin(2 * np.pi * freq * t) * np.exp(-decay * t) for freq, amp, decay in partials)
signal *= np.minimum(t / 0.006, 1.0) * np.clip((length - t) / 0.08, 0.0, 1.0)
signal *= 0.5 / np.max(np.abs(signal))

with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
    with wave.open(tmp.name, "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(rate)
        wav.writeframes((signal * 32767).astype("<i2").tobytes())
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp.name, "-c:a", "libvorbis", "-q:a", "5", out], check=True)
print("wrote", out)
