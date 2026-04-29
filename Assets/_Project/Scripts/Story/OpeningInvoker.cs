using UnityEngine;

namespace Bandhana.Story
{
    public enum OpeningBeat
    {
        None,
        MomShouts,
        MomKitchen,
        KarunaDad,
        KarunaTUDay,
        PlayDamaruFirstTime,
        TUNightWake,
        EmptyStreet,
        BajeAppears,
        BajeOutside,
        KarunaTwistIntro,
    }

    // Sibling of an Interactable / AutoCutsceneTrigger. Subscribes to its
    // event in Awake and dispatches by enum, so the editor builder doesn't
    // need to wire static methods through UnityEvent reflection.
    public class OpeningInvoker : MonoBehaviour
    {
        public OpeningBeat beat;

        void Awake()
        {
            var inter = GetComponent<Bandhana.Overworld.Interactable>();
            if (inter != null) inter.onInteract.AddListener(Fire);
            var auto  = GetComponent<Bandhana.Overworld.AutoCutsceneTrigger>();
            if (auto  != null) auto.onTriggered.AddListener(Fire);
        }

        public void Fire()
        {
            switch (beat)
            {
                case OpeningBeat.MomShouts:           OpeningCutscenes.MomShouts();           break;
                case OpeningBeat.MomKitchen:          OpeningCutscenes.MomKitchen();          break;
                case OpeningBeat.KarunaDad:           OpeningCutscenes.KarunaDad();           break;
                case OpeningBeat.KarunaTUDay:         OpeningCutscenes.KarunaTUDay();         break;
                case OpeningBeat.PlayDamaruFirstTime: OpeningCutscenes.PlayDamaruFirstTime(); break;
                case OpeningBeat.TUNightWake:         OpeningCutscenes.TUNightWake();         break;
                case OpeningBeat.EmptyStreet:         OpeningCutscenes.EmptyStreet();         break;
                case OpeningBeat.BajeAppears:         OpeningCutscenes.BajeAppears();         break;
                case OpeningBeat.BajeOutside:         OpeningCutscenes.BajeOutside();         break;
                case OpeningBeat.KarunaTwistIntro:    OpeningCutscenes.KarunaTwistIntro();    break;
            }
        }
    }
}
