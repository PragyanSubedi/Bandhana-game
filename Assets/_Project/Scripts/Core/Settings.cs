using UnityEngine;

namespace Bandhana.Core
{
    // PlayerPrefs-backed settings. Apply() pushes them to Unity each time.
    public static class Settings
    {
        const string KeyVolume     = "bandhana.volume";
        const string KeyFullscreen = "bandhana.fullscreen";
        const string KeyVSync      = "bandhana.vsync";

        static bool _loaded;
        public static float Volume     { get; private set; } = 0.7f;
        public static bool  Fullscreen { get; private set; } = false;
        public static bool  VSync      { get; private set; } = true;

        public static void Load()
        {
            if (_loaded) return;
            Volume     = PlayerPrefs.GetFloat(KeyVolume, 0.7f);
            Fullscreen = PlayerPrefs.GetInt(KeyFullscreen, 0) == 1;
            VSync      = PlayerPrefs.GetInt(KeyVSync, 1) == 1;
            _loaded = true;
            Apply();
        }

        public static void SetVolume(float v)
        {
            Volume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(KeyVolume, Volume);
            AudioListener.volume = Volume;
        }

        public static void SetFullscreen(bool f)
        {
            Fullscreen = f;
            PlayerPrefs.SetInt(KeyFullscreen, f ? 1 : 0);
            Screen.fullScreen = f;
        }

        public static void SetVSync(bool v)
        {
            VSync = v;
            PlayerPrefs.SetInt(KeyVSync, v ? 1 : 0);
            QualitySettings.vSyncCount = v ? 1 : 0;
        }

        public static void Apply()
        {
            AudioListener.volume = Volume;
            Screen.fullScreen = Fullscreen;
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }
    }
}
