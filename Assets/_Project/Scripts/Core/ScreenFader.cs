using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bandhana.Core
{
    // Persistent IMGUI black overlay for fades and timed blackouts.
    // Auto-creates on first access, survives scene loads. UIState-aware so
    // the player can't move during a fade.
    public class ScreenFader : MonoBehaviour
    {
        static ScreenFader _instance;
        public static ScreenFader Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<ScreenFader>();
                if (_instance != null) return _instance;
                var go = new GameObject("ScreenFader");
                _instance = go.AddComponent<ScreenFader>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        public float alpha;
        public Color tint = Color.black;
        Texture2D pixel;

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsurePixel();
        }

        void EnsurePixel()
        {
            if (pixel != null) return;
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
        }

        void OnGUI()
        {
            if (alpha <= 0f) return;
            EnsurePixel();
            var prev = GUI.color;
            GUI.color = new Color(tint.r, tint.g, tint.b, alpha);
            GUI.depth = -1000;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixel);
            GUI.color = prev;
        }

        public IEnumerator Fade(float from, float to, float seconds, Color color)
        {
            tint = color;
            float t = 0f;
            seconds = Mathf.Max(0.01f, seconds);
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                alpha = Mathf.Lerp(from, to, t / seconds);
                yield return null;
            }
            alpha = to;
        }

        public IEnumerator FadeOut(float seconds = 0.6f) => Fade(alpha, 1f, seconds, tint);
        public IEnumerator FadeIn(float seconds = 0.6f)  => Fade(alpha, 0f, seconds, tint);

        // Black out, hold, load scene at named spawn, fade back in.
        public IEnumerator FadeAndLoad(string sceneName, Vector2 spawn,
                                       float fadeOut = 0.6f, float hold = 0.3f, float fadeIn = 0.6f)
        {
            UIState.Open();
            yield return Fade(alpha, 1f, fadeOut, Color.black);
            if (hold > 0f) yield return new WaitForSecondsRealtime(hold);

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SaveContext.SetPending(spawn);
                SceneManager.LoadScene(sceneName);
            }
            else Debug.LogError($"[Bandhana] ScreenFader.FadeAndLoad: '{sceneName}' not in Build Settings.");

            // wait one frame so the new scene's Awake runs
            yield return null;
            yield return Fade(1f, 0f, fadeIn, Color.black);
            UIState.Close();
        }
    }
}
