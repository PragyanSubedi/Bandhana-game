using UnityEngine;

namespace Bandhana.Core
{
    // Tiny SFX synthesis. Build AudioClips procedurally — sine blips, noise bursts,
    // and arpeggios — so M7 needs no imported audio.
    public static class ToneFactory
    {
        const int SampleRate = 44100;

        public static AudioClip MakeBlip(float freq, float duration, float amplitude = 0.3f)
        {
            int n = Mathf.RoundToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Pow(1f - t / duration, 2f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * env;
            }
            var clip = AudioClip.Create("blip", n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip MakeNoise(float duration, float amplitude = 0.3f)
        {
            int n = Mathf.RoundToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Pow(1f - t / duration, 2f);
                data[i] = (Random.value * 2f - 1f) * amplitude * env;
            }
            var clip = AudioClip.Create("noise", n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip MakeArpeggio(float[] frequencies, float perNoteDuration, float amplitude = 0.3f)
        {
            int notesN = frequencies.Length;
            int perN = Mathf.RoundToInt(SampleRate * perNoteDuration);
            int n = perN * notesN;
            var data = new float[n];
            for (int k = 0; k < notesN; k++)
            {
                float freq = frequencies[k];
                int offset = k * perN;
                for (int i = 0; i < perN; i++)
                {
                    float t = (float)i / SampleRate;
                    float env = Mathf.Pow(1f - t / perNoteDuration, 1.5f);
                    data[offset + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * env;
                }
            }
            var clip = AudioClip.Create("arp", n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
