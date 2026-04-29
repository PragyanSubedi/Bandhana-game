using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Story
{
    // Container the editor builder fills with handles to dialogue assets the
    // runtime cutscenes need. A single instance lives on the CutsceneRunner
    // GameObject in any opening scene; cutscenes look it up via
    // FindFirstObjectByType.
    public class StoryAssets : MonoBehaviour
    {
        [Header("Opening dialogue")]
        public DialogueSO momShouts;
        public DialogueSO momKitchenIntro;     // Mom's first lines, before Sister chimes in
        public DialogueSO momKitchenAfter;     // Sister's first quip onward
        public DialogueSO sisterMusicStar;
        public DialogueSO momLunch;
        public DialogueSO karunasDad;
        public DialogueSO karunaTUGarden;
        public DialogueSO damaruPickup;
        public DialogueSO leleAlone;
        public DialogueSO emptyStreet;
        public DialogueSO bajeMeeting;
        public DialogueSO bajeAstralLore;
        public DialogueSO bajeOutside;
        public DialogueSO karunaTwistIntro;
        public DialogueSO karunaPostBoss;
    }
}
