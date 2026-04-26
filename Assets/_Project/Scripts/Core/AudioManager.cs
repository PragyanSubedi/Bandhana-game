using UnityEngine;

namespace Bandhana.Core
{
    // Lazy-singleton audio source. Generates simple SFX clips at runtime so we
    // don't need any imported audio assets to ship M7. Replace with real .wav
    // assets later by swapping ToneFactory calls for AssetDatabase.LoadAssetAtPath.
    public class AudioManager : MonoBehaviour
    {
        static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance != null) return _instance;
                var go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        AudioSource src;
        AudioClip clipClick, clipStep, clipHit, clipBondHit, clipBondSuccess, clipBondFail;

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Settings.Load();

            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;

            clipClick       = ToneFactory.MakeBlip(880f, 0.05f, 0.25f);
            clipStep        = ToneFactory.MakeNoise(0.04f, 0.10f);
            clipHit         = ToneFactory.MakeNoise(0.10f, 0.30f);
            clipBondHit     = ToneFactory.MakeBlip(660f, 0.06f, 0.22f);
            clipBondSuccess = ToneFactory.MakeArpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.08f, 0.25f);
            clipBondFail    = ToneFactory.MakeArpeggio(new[] { 523f, 392f, 311f }, 0.10f, 0.22f);
        }

        public void Click()       => src.PlayOneShot(clipClick);
        public void Step()        => src.PlayOneShot(clipStep);
        public void Hit()         => src.PlayOneShot(clipHit);
        public void BondHit()     => src.PlayOneShot(clipBondHit);
        public void BondSuccess() => src.PlayOneShot(clipBondSuccess);
        public void BondFail()    => src.PlayOneShot(clipBondFail);
    }
}
