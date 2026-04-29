using System.Collections;
using UnityEngine;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Core
{
    // Singleton coroutine host. Story scripts hand it an IEnumerator and it
    // runs it with UIState locked so the player can't move mid-cutscene.
    // Survives scene loads.
    public class CutsceneRunner : MonoBehaviour
    {
        static CutsceneRunner _instance;
        public static CutsceneRunner Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<CutsceneRunner>();
                if (_instance != null) return _instance;
                var go = new GameObject("CutsceneRunner");
                _instance = go.AddComponent<CutsceneRunner>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        public bool IsPlaying { get; private set; }

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Run(IEnumerator routine)
        {
            if (IsPlaying || routine == null) return;
            StartCoroutine(Wrap(routine));
        }

        IEnumerator Wrap(IEnumerator inner)
        {
            IsPlaying = true;
            UIState.Open();
            yield return inner;
            UIState.Close();
            IsPlaying = false;
        }

        // Convenience step builders ────────────────────────────────────────────
        public static IEnumerator Say(DialogueSO d)
        {
            if (d == null || DialogueRunner.Instance == null) yield break;
            DialogueRunner.Instance.Play(d);
            while (DialogueRunner.Instance.IsPlaying) yield return null;
        }

        public static IEnumerator Wait(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }

        public static IEnumerator SetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) GameManager.Instance.SetFlag(flag);
            yield break;
        }

        public static IEnumerator SetRealm(WorldRealm r)
        {
            WorldState.Set(r);
            yield break;
        }
    }
}
