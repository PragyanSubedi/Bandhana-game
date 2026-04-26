#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bandhana.Core;
using Bandhana.Data;
using Bandhana.Overworld;
using Bandhana.UI;

namespace Bandhana.EditorTools
{
    // M6+: builds story scenes (Village, Foothills, Approach), Credits, and the
    // building interiors the player can enter. Idempotent — safe to re-run.
    // Visual style for a Building. Drives wall sprite choice, windows, roof
    // ornaments, and decorative additions (prayer flags / shop banner).
    public enum BuildingStyle { Residential, Temple, Shop, Hermit, Shrine }

    public static class BuildAllStoryScenes
    {
        // Outer scenes
        const string VillageScenePath    = "Assets/_Project/Scenes/Overworld/Village.unity";
        const string FoothillsScenePath  = "Assets/_Project/Scenes/Overworld/Foothills.unity";
        const string ApproachScenePath   = "Assets/_Project/Scenes/Overworld/Approach.unity";
        const string CreditsScenePath    = "Assets/_Project/Scenes/MainMenu/Credits.unity";

        // Building interiors
        const string VillageElderPath    = "Assets/_Project/Scenes/Overworld/Village_Elder.unity";
        const string VillageTemplePath   = "Assets/_Project/Scenes/Overworld/Village_Temple.unity";
        const string VillageShopPath     = "Assets/_Project/Scenes/Overworld/Village_Shop.unity";
        const string FoothillsHermitPath = "Assets/_Project/Scenes/Overworld/Foothills_Hermit.unity";
        const string ApproachShrinePath  = "Assets/_Project/Scenes/Overworld/Approach_Shrine.unity";

        const string DamaruPath  = "Assets/_Project/Data/Spirits/Spirit_Damaru.asset";
        const string KhyaakPath  = "Assets/_Project/Data/Spirits/Spirit_Khyaak.asset";
        const string YetiPath    = "Assets/_Project/Data/Spirits/Spirit_Yeti.asset";

        const string DialogueDir = "Assets/_Project/Data/Dialogue";

        // Village outer-scene dimensions (tiles). Player walks within ±halfW / ±halfH.
        const int VHalfW = 18, VHalfH = 12;
        const int FHalfW = 18, FHalfH = 12;
        const int AHalfW = 18, AHalfH = 12;

        [MenuItem("Bandhana/Build All Story Scenes (M6)")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureDialogues();

            BuildVillage();
            BuildVillageElderInterior();
            BuildVillageTempleInterior();
            BuildVillageShopInterior();

            BuildFoothills();
            BuildFoothillsHermitInterior();

            BuildApproach();
            BuildApproachShrineInterior();

            BuildCredits();

            EditorUtility.DisplayDialog("Bandhana — M6+",
                "Built: Village (+ Elder/Temple/Shop interiors),\n" +
                "Foothills (+ Hermit interior),\n" +
                "Approach (+ Shrine interior),\n" +
                "Credits.\n\n" +
                "All scenes registered in Build Settings. Open MainMenu and Play.",
                "OK");
        }

        // ── Dialogues ────────────────────────────────────────────────────────
        static DialogueSO Dlg(string fileName, params (string speaker, string text)[] lines)
        {
            Directory.CreateDirectory(DialogueDir);
            var path = $"{DialogueDir}/{fileName}.asset";
            var d = AssetDatabase.LoadAssetAtPath<DialogueSO>(path);
            if (d == null) { d = ScriptableObject.CreateInstance<DialogueSO>(); AssetDatabase.CreateAsset(d, path); }
            d.lines.Clear();
            foreach (var (s, t) in lines) d.lines.Add(new DialogueLine { speaker = s, text = t });
            EditorUtility.SetDirty(d);
            return d;
        }

        // Existing
        static DialogueSO dlgKaruna, dlgDrum, dlgDamaruPre, dlgDamaruSuccess, dlgDamaruFail;
        static DialogueSO dlgVillagerEast, dlgFoothillElder, dlgGateLocked;
        static DialogueSO dlgDevraj, dlgGateDisciple, dlgGatePostDefeat;

        // Village — additional villagers
        static DialogueSO dlgChild, dlgOldMan, dlgWoman, dlgMonkTrainee, dlgDrunkard, dlgWidow, dlgWaterCarrier;
        // Village interiors
        static DialogueSO dlgElder, dlgPriest, dlgShopkeeper;
        static DialogueSO dlgElderDoor, dlgTempleDoor, dlgShopDoor;

        // Foothills extras
        static DialogueSO dlgPilgrim, dlgWanderingMonk, dlgCairn, dlgHermit, dlgHermitDoor;

        // Approach extras
        static DialogueSO dlgFellowPilgrim, dlgStoneCarver, dlgSage, dlgShrineDoor;

        static void EnsureDialogues()
        {
            // ── Existing dialogue (preserved verbatim) ──
            dlgKaruna = Dlg("Dialogue_KarunaPyre",
                ("Karuna-la (memory)", "You stand by the pyre. The smoke rises to no one in particular."),
                ("Karuna-la (memory)", "When I am gone — and that will be soon — take the drum."),
                ("Karuna-la (memory)", "Walk east, until you cannot any longer. Whatever you find, do not force it."),
                ("Karuna-la (memory)", "Sit with it. Hear its rhythm. The bond comes after."));

            dlgDrum = Dlg("Dialogue_DrumPickup",
                ("", "You take Karuna-la's damaru drum. Its skin is warm."),
                ("", "The path east is clear. The stream-spirit is silent."));

            dlgDamaruPre = Dlg("Dialogue_DamaruPre",
                ("", "A small spirit lies in the thornbrake. Its drum-shell is cracked."),
                ("", "Its rhythm has stuttered into silence. Its form is fading."),
                ("", "You sit. You match the heartbeat as it slows. You tap with the drum."));

            dlgDamaruSuccess = Dlg("Dialogue_DamaruSuccess",
                ("Damaru", "...you are warm."),
                ("", "Damaru opens its eyes. Its drum syncs with yours."),
                ("", "Damaru joins your party."));

            dlgDamaruFail = Dlg("Dialogue_DamaruFail",
                ("", "The rhythm slips. Damaru's heartbeat falters."),
                ("", "It is not too late. Try again — listen, and do not rush."));

            dlgVillagerEast = Dlg("Dialogue_VillagerEast",
                ("Villager", "The stream is silent for the first time in my long life."),
                ("Villager", "Whatever Karuna-la was investigating — it is bigger than us."),
                ("Villager", "Take the path east when you are ready. The foothills are crying."));

            dlgFoothillElder = Dlg("Dialogue_FoothillElder",
                ("Foothill Elder", "Did you not stop for the small one in the thornbrake?"),
                ("Foothill Elder", "Go back. Sit with it. The road east stays closed for those who pass by suffering."));

            dlgGateLocked = Dlg("Dialogue_GateLocked",
                ("", "The path is not yet open. You must first do what you came to do."));

            dlgDevraj = Dlg("Dialogue_DevrajCameo",
                ("Devraj", "So. Another pilgrim, fresh from the foothills."),
                ("Devraj", "You learned the bond? The slow way? The kind one?"),
                ("Devraj", "I prefer mine faster. The spirits do not consent. They comply."),
                ("Devraj", "We will meet again at Vajrasana. Try not to die before then."));

            dlgGateDisciple = Dlg("Dialogue_GateDiscipleChallenge",
                ("Gate Disciple of Akshobhya", "You stand at the eastern gate of Vajrasana."),
                ("Gate Disciple of Akshobhya", "None pass to the mirror-pool unproven."),
                ("Gate Disciple of Akshobhya", "Show me your bond, pilgrim."));

            dlgGatePostDefeat = Dlg("Dialogue_GatePostDefeat",
                ("Gate Disciple of Akshobhya", "Your bond holds. Pass."),
                ("Gate Disciple of Akshobhya", "Akshobhya waits beyond the mirror-pool. Be still when you face them."));

            // ── New: Village ambient villagers ──
            dlgChild = Dlg("Dialogue_VillagerChild",
                ("Child", "Look at my stones! I made a cairn! Karuna-la said cairns mark where prayers wait."),
                ("Child", "Do you have a prayer to leave here? I'll guard it."));

            dlgOldMan = Dlg("Dialogue_VillagerOldMan",
                ("Old Pemba", "When I was small, the stream sang to my grandmother."),
                ("Old Pemba", "Now it does not sing to anyone. The world thins."),
                ("Old Pemba", "Walk slow, child. The fast pilgrims always die first."));

            dlgWoman = Dlg("Dialogue_VillagerWoman",
                ("Tashi-mo", "I'm gathering nettle for soup. The young monk eats like four men."),
                ("Tashi-mo", "If you see him in the temple, tell him the lentils are nearly ready."));

            dlgMonkTrainee = Dlg("Dialogue_MonkTrainee",
                ("Young Monk", "I was supposed to memorize the eight petals by sundown."),
                ("Young Monk", "I have memorized two and forgotten one. This is bad arithmetic."),
                ("Young Monk", "Don't tell the priest. He'll send me to scrub the lamps again."));

            dlgDrunkard = Dlg("Dialogue_VillagerDrunkard",
                ("Norbu", "I drink to the spirits. They drink with me. We are very honest, my friends and I."),
                ("Norbu", "Tell Karuna-la — oh. Right. The pyre. Forget it."));

            dlgWidow = Dlg("Dialogue_VillagerWidow",
                ("Dolma", "Karuna-la was kind to me when my husband walked out into the snow."),
                ("Dolma", "I light a butter lamp at the temple every dawn. Tell the priest if you see him."));

            dlgWaterCarrier = Dlg("Dialogue_VillagerWaterCarrier",
                ("Water Carrier", "The well still gives, but the water tastes thinner."),
                ("Water Carrier", "Even water remembers what it has lost."));

            // ── Village interior NPCs ──
            dlgElder = Dlg("Dialogue_VillageElder",
                ("Village Elder", "Sit, sit. Your boots are loud and the dust here is old."),
                ("Village Elder", "Karuna-la was my pupil, long ago. She taught the bond as listening."),
                ("Village Elder", "The disciples at Vajrasana have forgotten that. They mistake authority for relationship."),
                ("Village Elder", "When you reach the mirror-pool, do not show them your strength. Show them your stillness."));

            dlgPriest = Dlg("Dialogue_TemplePriest",
                ("Priest", "Welcome to the lamp-room. Mind the butter — it pools on cold nights."),
                ("Priest", "The eight petals of the mandala name the eight ways the world reaches for itself."),
                ("Priest", "Most pilgrims know one petal well and the other seven not at all. That is the work."));

            dlgShopkeeper = Dlg("Dialogue_VillageShopkeeper",
                ("Shopkeeper", "Half my shelves are empty. Caravans don't come past the foothills anymore."),
                ("Shopkeeper", "I'd sell you a charm but the carvings have stopped working. The spirit went out of them."),
                ("Shopkeeper", "Take a salt-lump for the road. Don't pay me — I'd only spend it on millet beer."));

            dlgElderDoor   = Dlg("Dialogue_VillageElderDoorLocked",
                ("", "The door is fastened. The Elder must be praying."));
            dlgTempleDoor  = Dlg("Dialogue_VillageTempleDoorLocked",
                ("", "The temple's door is heavy. Push it open."));
            dlgShopDoor    = Dlg("Dialogue_VillageShopDoorLocked",
                ("", "The shop is shuttered for the moment."));

            // ── Foothills extras ──
            dlgPilgrim = Dlg("Dialogue_FoothillsPilgrim",
                ("Frightened Pilgrim", "Don't go further north — the cave there hums with something old."),
                ("Frightened Pilgrim", "I prayed and ran. I am not proud of the running."));

            dlgWanderingMonk = Dlg("Dialogue_FoothillsWanderingMonk",
                ("Wandering Monk", "The mountains are tilting their faces toward Vajrasana."),
                ("Wandering Monk", "When stones lean toward an altar, something there has woken up."));

            dlgCairn = Dlg("Dialogue_FoothillsCairn",
                ("", "Three stacked stones. A pilgrim's cairn — left for the next traveller."),
                ("", "You add a small stone of your own. The wind feels lighter."));

            dlgHermit = Dlg("Dialogue_FoothillsHermit",
                ("Hermit", "I have not spoken in nine days. My voice is dusty."),
                ("Hermit", "You carry a drum. Good. Drums know things words have forgotten."),
                ("Hermit", "Beyond the gate-disciple is a mirror-pool. Do not look at yourself in it. Look through it."));

            dlgHermitDoor  = Dlg("Dialogue_FoothillsHermitDoorLocked",
                ("", "The hut is locked. The hermit emerges only on certain days."));

            // ── Approach extras ──
            dlgFellowPilgrim = Dlg("Dialogue_ApproachFellowPilgrim",
                ("Fellow Pilgrim", "I have been turned back twice. Devraj passed me without looking."),
                ("Fellow Pilgrim", "If you go through, please remember my name to the disciple. It is Pasang."));

            dlgStoneCarver = Dlg("Dialogue_ApproachStoneCarver",
                ("Stone Carver", "I carve the names of those who do not return. There are too many spaces left."),
                ("Stone Carver", "If you fall at the gate, I will carve yours kindly. But better not to fall."));

            dlgSage = Dlg("Dialogue_ApproachSage",
                ("Old Sage", "Vajrasana means 'the indestructible seat'. Some say it cannot be moved by storm or earthquake."),
                ("Old Sage", "I have seen it move. Not by storm. By grief."));

            dlgShrineDoor  = Dlg("Dialogue_ApproachShrineDoorLocked",
                ("", "The shrine door is heavy stone. You lean into it."));

            AssetDatabase.SaveAssets();
        }

        // ── Village (Act 0) ──────────────────────────────────────────────────
        static void BuildVillage()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VillageScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.10f, 0.09f, 0.16f);
            MakePlayer(new Vector3(-15, 0, 0));
            MakeUIRoot();

            // Outer perimeter walls
            BuildPerimeter(VHalfW, VHalfH, eastGapY: 0);

            // Pyre (decorative wall block) — preserved
            for (int x = -2; x <= 0; x++) Wall(new Vector2(x, 2));

            // Story-critical NPCs (positions preserved)
            MakeNPC("Karuna-la", new Vector3(-1, 1, 0), new Color(0.85f, 0.55f, 0.45f), dlgKaruna);
            MakeNPC("Villager",  new Vector3( 2, -2, 0), new Color(0.70f, 0.75f, 0.80f), dlgVillagerEast);

            // Drum pickup (preserved)
            MakeFlagSetter("DrumPickup", new Vector3(2, 1, 0), new Color(0.90f, 0.80f, 0.55f),
                "drumCollected", dlgDrum);

            // Ambient villagers spread across the larger map
            MakeNPC("Child",         new Vector3(-7, -3, 0), new Color(0.95f, 0.80f, 0.60f), dlgChild);
            MakeNPC("Old Pemba",     new Vector3(-12, 4,  0), new Color(0.55f, 0.50f, 0.45f), dlgOldMan);
            MakeNPC("Tashi-mo",      new Vector3( 7, -7,  0), new Color(0.80f, 0.50f, 0.45f), dlgWoman);
            MakeNPC("Monk Trainee",  new Vector3( 5,  3,  0), new Color(0.85f, 0.40f, 0.30f), dlgMonkTrainee);
            MakeNPC("Norbu",         new Vector3(-10,-7,  0), new Color(0.65f, 0.55f, 0.55f), dlgDrunkard);
            MakeNPC("Dolma",         new Vector3( 6,  6,  0), new Color(0.75f, 0.70f, 0.70f), dlgWidow);
            MakeNPC("Water Carrier", new Vector3(13, -3,  0), new Color(0.55f, 0.65f, 0.75f), dlgWaterCarrier);

            // Buildings — interiors are separate scenes loaded via SceneTransition
            // (doors live on the south edge of each building footprint)
            Building("Elder",  new Vector2Int(-15,  6), 7, 6, 3,
                     BuildingStyle.Residential,
                     wallColor: new Color(0.78f, 0.62f, 0.42f),  // tan plank
                     roofColor: new Color(0.58f, 0.30f, 0.22f),  // crimson clay
                     "Village_Elder", new Vector2(0, -3));
            Building("Temple", new Vector2Int( -3,  6), 7, 6, 3,
                     BuildingStyle.Temple,
                     wallColor: new Color(0.92f, 0.86f, 0.72f),  // whitewashed plaster
                     roofColor: new Color(0.85f, 0.40f, 0.18f),  // saffron-orange
                     "Village_Temple", new Vector2(0, -3));
            Building("Shop",   new Vector2Int( 10, -10), 6, 5, 3,
                     BuildingStyle.Shop,
                     wallColor: new Color(0.72f, 0.55f, 0.32f),
                     roofColor: new Color(0.45f, 0.32f, 0.20f),
                     "Village_Shop", new Vector2(0, -3));

            // Decor
            Decor(new Vector3(-3, -1, 0), new Color(0.40f, 0.55f, 0.30f), 0); // potted plant
            Decor(new Vector3(-7,  4, 0), new Color(0.50f, 0.50f, 0.50f), 1); // cairn
            Decor(new Vector3(-11, 5, 0), new Color(0.45f, 0.30f, 0.20f), 2); // bench (next to Old Pemba)
            Decor(new Vector3( 4,  4, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3(11, -3, 0), new Color(0.50f, 0.50f, 0.50f), 1);
            Decor(new Vector3(-5,  6, 0), new Color(0.40f, 0.55f, 0.30f), 0);

            // East transition (gated) — moved to new east boundary
            MakeTransition(new Vector3(VHalfW, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Foothills", new Vector2(-FHalfW + 1, 0), "drumCollected",
                "the drum is yours. take it before you leave.");

            EditorSceneManager.SaveScene(scene, VillageScenePath);
            EnsureSceneInBuildSettings(VillageScenePath);
        }

        static void BuildVillageElderInterior()
        {
            BuildInterior(VillageElderPath, "Village",
                          new Color(0.18f, 0.12f, 0.08f),
                          new Color(0.45f, 0.30f, 0.20f),
                          width: 13, height: 9,
                          returnSpawn: new Vector2(-12, 5),  // outside Village Elder's south door
                          populate: () =>
                          {
                              MakeNPC("Village Elder", new Vector3(0, 1, 0),
                                      new Color(0.85f, 0.65f, 0.55f), dlgElder);
                              Decor(new Vector3(-3, 2, 0), new Color(0.50f, 0.40f, 0.25f), 2); // bench
                              Decor(new Vector3( 3, 2, 0), new Color(0.50f, 0.40f, 0.25f), 2);
                              Decor(new Vector3(-4, -2, 0), new Color(0.40f, 0.55f, 0.30f), 0); // plant
                              Decor(new Vector3( 4, -2, 0), new Color(0.40f, 0.55f, 0.30f), 0);
                          });
        }

        static void BuildVillageTempleInterior()
        {
            BuildInterior(VillageTemplePath, "Village",
                          new Color(0.10f, 0.07f, 0.05f),
                          new Color(0.50f, 0.30f, 0.20f),
                          width: 13, height: 9,
                          returnSpawn: new Vector2(0, 5),
                          populate: () =>
                          {
                              MakeNPC("Priest", new Vector3(0, 2, 0),
                                      new Color(0.85f, 0.40f, 0.30f), dlgPriest);
                              Decor(new Vector3(-3, 0, 0), new Color(0.85f, 0.65f, 0.30f), 1); // cairn (lamp-stack feel)
                              Decor(new Vector3( 3, 0, 0), new Color(0.85f, 0.65f, 0.30f), 1);
                              Decor(new Vector3( 0,-2, 0), new Color(0.40f, 0.55f, 0.30f), 0);
                          });
        }

        static void BuildVillageShopInterior()
        {
            BuildInterior(VillageShopPath, "Village",
                          new Color(0.14f, 0.10f, 0.07f),
                          new Color(0.55f, 0.40f, 0.25f),
                          width: 11, height: 8,
                          returnSpawn: new Vector2(13, -11),
                          populate: () =>
                          {
                              MakeNPC("Shopkeeper", new Vector3(0, 1, 0),
                                      new Color(0.75f, 0.55f, 0.40f), dlgShopkeeper);
                              Decor(new Vector3(-3, 1, 0), new Color(0.50f, 0.40f, 0.25f), 2); // bench/counter
                              Decor(new Vector3( 3, 1, 0), new Color(0.50f, 0.40f, 0.25f), 2);
                              Decor(new Vector3(-4,-2, 0), new Color(0.50f, 0.50f, 0.50f), 1); // jars
                              Decor(new Vector3( 4,-2, 0), new Color(0.50f, 0.50f, 0.50f), 1);
                          });
        }

        // ── Foothills (Act 1 — The Helping) ──────────────────────────────────
        static void BuildFoothills()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FoothillsScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.06f, 0.10f, 0.08f);
            MakePlayer(new Vector3(-FHalfW + 1, 0, 0));
            MakeUIRoot();

            BuildPerimeter(FHalfW, FHalfH, westGapY: 0, eastGapY: 0);

            // Boulders (preserved scattering pattern, expanded)
            Wall(new Vector2(-2,  2)); Wall(new Vector2(-1,  2));
            Wall(new Vector2( 1, -1)); Wall(new Vector2( 2, -1));
            Wall(new Vector2(-8,  4)); Wall(new Vector2(-9,  4));
            Wall(new Vector2( 7, -5)); Wall(new Vector2( 8, -5));
            Wall(new Vector2(12,  3)); Wall(new Vector2(13,  3));
            Wall(new Vector2(-13,-7)); Wall(new Vector2(-14,-7));

            MakeNPC("Foothill Elder", new Vector3(-12, 6, 0), new Color(0.65f, 0.55f, 0.35f), dlgFoothillElder);
            MakeNPC("Frightened Pilgrim", new Vector3(-4,  7, 0), new Color(0.85f, 0.75f, 0.65f), dlgPilgrim);
            MakeNPC("Wandering Monk", new Vector3( 9,  6, 0), new Color(0.85f, 0.40f, 0.30f), dlgWanderingMonk);
            MakeNPC("Cairn", new Vector3(-3, -7, 0), new Color(0.55f, 0.55f, 0.55f), dlgCairn);

            // The Helping — dying Damaru (preserved)
            var damaru = AssetDatabase.LoadAssetAtPath<SpiritSO>(DamaruPath);
            if (damaru != null)
            {
                var go = new GameObject("HelpingTrigger_Damaru");
                go.transform.position = new Vector3(0, 0, 0);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Drum(new Color(0.95f, 0.85f, 0.55f));
                sr.sortingOrder = 6;
                var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
                var help = go.AddComponent<HelpingTrigger>();
                help.spirit = damaru;
                help.spiritLevel = 5;
                help.preDialogue = dlgDamaruPre;
                help.successDialogue = dlgDamaruSuccess;
                help.failureDialogue = dlgDamaruFail;
                help.completionFlag = "damaruBonded";
            }

            // Optional Khyaak haunt (preserved, moved to a more open spot)
            var khyaak = AssetDatabase.LoadAssetAtPath<SpiritSO>(KhyaakPath);
            if (khyaak != null)
            {
                var go = new GameObject("SpiritHaunt_Khyaak");
                go.transform.position = new Vector3(11, 4, 0);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Haunt(new Color(0.55f, 0.85f, 0.95f));
                sr.sortingOrder = 6;
                var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
                var h = go.AddComponent<SpiritHaunt>();
                h.spirit = khyaak;
                h.minLevel = 4; h.maxLevel = 6;
                h.battleSceneName = "M3Battle";
                h.returnSceneName = "Foothills";
            }

            // Hermit's Hut
            Building("Hermit", new Vector2Int(-14, -10), 6, 5, 3,
                     BuildingStyle.Hermit,
                     wallColor: new Color(0.52f, 0.40f, 0.28f),  // weathered wood
                     roofColor: new Color(0.32f, 0.24f, 0.16f),  // dark slate
                     "Foothills_Hermit", new Vector2(0, -3));

            // Decor
            Decor(new Vector3(-6,  3, 0), new Color(0.50f, 0.50f, 0.50f), 1);
            Decor(new Vector3( 6,  3, 0), new Color(0.50f, 0.50f, 0.50f), 1);
            Decor(new Vector3( 0,  9, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3(-9, -3, 0), new Color(0.40f, 0.55f, 0.30f), 0);

            // Transitions
            MakeTransition(new Vector3(-FHalfW, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Village", new Vector2(VHalfW - 1, 0), null, "");
            MakeTransition(new Vector3( FHalfW, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Approach", new Vector2(-AHalfW + 1, 0), "damaruBonded",
                "the small one is still in the thornbrake. you cannot leave it.");

            EditorSceneManager.SaveScene(scene, FoothillsScenePath);
            EnsureSceneInBuildSettings(FoothillsScenePath);
        }

        static void BuildFoothillsHermitInterior()
        {
            BuildInterior(FoothillsHermitPath, "Foothills",
                          new Color(0.10f, 0.08f, 0.05f),
                          new Color(0.40f, 0.30f, 0.20f),
                          width: 11, height: 8,
                          returnSpawn: new Vector2(-11, -11),
                          populate: () =>
                          {
                              MakeNPC("Hermit", new Vector3(0, 1, 0),
                                      new Color(0.65f, 0.55f, 0.35f), dlgHermit);
                              Decor(new Vector3(-3, 0, 0), new Color(0.40f, 0.55f, 0.30f), 0);
                              Decor(new Vector3( 3, 0, 0), new Color(0.50f, 0.50f, 0.50f), 1);
                              Decor(new Vector3( 0,-2, 0), new Color(0.50f, 0.40f, 0.25f), 2); // bench
                          });
        }

        // ── Approach (Act 2 opening) ─────────────────────────────────────────
        static void BuildApproach()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ApproachScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.05f, 0.07f, 0.14f);
            MakePlayer(new Vector3(-AHalfW + 1, 0, 0));
            MakeUIRoot();

            BuildPerimeter(AHalfW, AHalfH, westGapY: 0);

            // Stone columns — gate visual (preserved + extended)
            Wall(new Vector2(13,  1)); Wall(new Vector2(13, -1));
            Wall(new Vector2(15,  3)); Wall(new Vector2(15, -3));
            Wall(new Vector2(-3,  5)); Wall(new Vector2(-2,  5));
            Wall(new Vector2( 4, -6)); Wall(new Vector2( 5, -6));

            MakeNPC("Devraj", new Vector3(-2, 2, 0), new Color(0.75f, 0.45f, 0.95f), dlgDevraj);
            MakeNPC("Pasang", new Vector3(-9,  4, 0), new Color(0.85f, 0.75f, 0.65f), dlgFellowPilgrim);
            MakeNPC("Stone Carver", new Vector3( 3, -4, 0), new Color(0.55f, 0.55f, 0.55f), dlgStoneCarver);
            MakeNPC("Old Sage", new Vector3(-12, -5, 0), new Color(0.85f, 0.85f, 0.75f), dlgSage);

            // Gate disciple — boss BattleNPC (preserved, moved deeper east)
            var yeti = AssetDatabase.LoadAssetAtPath<SpiritSO>(YetiPath);
            if (yeti != null)
            {
                var go = new GameObject("BattleNPC_GateDisciple");
                go.transform.position = new Vector3(14, 0, 0);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.NPC(new Color(0.85f, 0.55f, 0.45f));
                sr.sortingOrder = 6;
                var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
                var bn = go.AddComponent<BattleNPC>();
                bn.npcName = "Gate Disciple of Akshobhya";
                bn.preBattleDialogue = dlgGateDisciple;
                bn.postDefeatDialogue = dlgGatePostDefeat;
                bn.enemySpirit = yeti;
                bn.enemyLevel = 10;
                bn.disableBond = true;
                bn.disableFlee = true;
                bn.battleSceneName = "M3Battle";
                bn.returnSceneName = "Approach";
                bn.winSceneName = "Credits";
                bn.defeatedFlag = "gateDiscipleDefeated";
            }

            // Stone Shrine (small enterable building)
            Building("Shrine", new Vector2Int(-16, 6), 6, 5, 3,
                     BuildingStyle.Shrine,
                     wallColor: new Color(0.55f, 0.55f, 0.58f),  // weathered stone
                     roofColor: new Color(0.30f, 0.32f, 0.38f),  // slate
                     "Approach_Shrine", new Vector2(0, -3));

            // Decor
            Decor(new Vector3(-6,  6, 0), new Color(0.50f, 0.50f, 0.50f), 1);
            Decor(new Vector3( 7,  6, 0), new Color(0.50f, 0.50f, 0.50f), 1);
            Decor(new Vector3( 0,  9, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3(-7, -8, 0), new Color(0.50f, 0.50f, 0.50f), 1);

            // West transition — back to Foothills
            MakeTransition(new Vector3(-AHalfW, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Foothills", new Vector2(FHalfW - 1, 0), null, "");

            EditorSceneManager.SaveScene(scene, ApproachScenePath);
            EnsureSceneInBuildSettings(ApproachScenePath);
        }

        static void BuildApproachShrineInterior()
        {
            BuildInterior(ApproachShrinePath, "Approach",
                          new Color(0.08f, 0.06f, 0.10f),
                          new Color(0.35f, 0.35f, 0.40f),
                          width: 11, height: 8,
                          returnSpawn: new Vector2(-13, 5),
                          populate: () =>
                          {
                              // No NPC — silent shrine. Cairn at center.
                              Decor(new Vector3(0, 1, 0), new Color(0.55f, 0.55f, 0.60f), 1);
                              Decor(new Vector3(-3, 1, 0), new Color(0.85f, 0.65f, 0.30f), 1); // butter lamp
                              Decor(new Vector3( 3, 1, 0), new Color(0.85f, 0.65f, 0.30f), 1);
                              MakeFlagSetter("ShrineBlessing", new Vector3(0, -1, 0),
                                             new Color(0.95f, 0.85f, 0.55f),
                                             "shrineBlessed",
                                             Dlg("Dialogue_ApproachShrineBlessing",
                                                 ("", "You kneel. The lamp-flames lean toward you, briefly."),
                                                 ("", "You leave a small stone on the cairn. The room feels warmer.")));
                          });
        }

        // ── Credits ──────────────────────────────────────────────────────────
        static void BuildCredits()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CreditsScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.05f, 0.04f, 0.10f);
            var ui = new GameObject("Credits");
            ui.AddComponent<CreditsScreen>();
            ui.AddComponent<SettingsMenu>();

            EditorSceneManager.SaveScene(scene, CreditsScenePath);
            EnsureSceneInBuildSettings(CreditsScenePath);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        static GameObject MakePlayer(Vector3 pos)
        {
            var go = new GameObject("Player");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Player(new Color(0.95f, 0.85f, 0.55f));
            sr.sortingOrder = 10;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<PlayerController>();

            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>() ?? cam.gameObject.AddComponent<CameraFollow>();
                follow.target = go.transform;
            }
            return go;
        }

        static void MakeUIRoot()
        {
            var ui = new GameObject("UIRoot");
            ui.AddComponent<PartyMenu>();
            ui.AddComponent<DialogueRunner>();
            ui.AddComponent<PauseMenu>();
            ui.AddComponent<SettingsMenu>();
        }

        static void BasicCamera(float r, float g, float b)
        {
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;   // slightly wider view for the bigger maps
            cam.backgroundColor = new Color(r, g, b);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);
        }

        // Build a rectangular outer perimeter centered at (0,0) with optional
        // openings at y = westGapY/eastGapY (use int.MinValue to mean "no gap").
        static void BuildPerimeter(int halfW, int halfH, int westGapY = int.MinValue, int eastGapY = int.MinValue)
        {
            var walls = new GameObject("Walls").transform;
            for (int x = -halfW; x <= halfW; x++)
            {
                Wall(new Vector2(x,  halfH), walls);
                Wall(new Vector2(x, -halfH), walls);
            }
            for (int y = -halfH + 1; y <= halfH - 1; y++)
            {
                if (y != westGapY) Wall(new Vector2(-halfW, y), walls);
                if (y != eastGapY) Wall(new Vector2( halfW, y), walls);
            }
        }

        static void MakeNPC(string name, Vector3 pos, Color color, DialogueSO dialogue)
        {
            var go = new GameObject($"NPC_{name.Replace(' ', '_')}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(color);
            sr.sortingOrder = 6;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
            var npc = go.AddComponent<NPC>();
            npc.npcName = name;
            npc.dialogue = dialogue;
        }

        static void MakeFlagSetter(string label, Vector3 pos, Color color, string flag, DialogueSO dialogue)
        {
            var go = new GameObject($"FlagSetter_{label}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Drum(color);
            sr.sortingOrder = 6;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
            var fs = go.AddComponent<FlagSetterTrigger>();
            fs.flag = flag;
            fs.dialogue = dialogue;
        }

        static void MakeTransition(Vector3 pos, Color color, string targetScene, Vector2 spawn,
                                   string requiredFlag, string lockedHint)
        {
            var go = new GameObject($"Transition_to_{targetScene}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Transition(color);
            sr.sortingOrder = 4;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
            var st = go.AddComponent<SceneTransition>();
            st.targetSceneName = targetScene;
            st.spawnPosition = spawn;
            st.requiredFlag = requiredFlag;
            if (!string.IsNullOrEmpty(lockedHint)) st.lockedHint = lockedHint;
        }

        static void Wall(Vector2 pos, Transform parent = null)
        {
            var go = new GameObject($"Wall_{pos.x}_{pos.y}");
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Wall();
            sr.sortingOrder = 5;
            go.AddComponent<BoxCollider2D>();
        }

        // Builds an architectural multi-row building exterior:
        //   y = origin.y     → facade row (door, frames, windows, corner walls)
        //   y = origin.y - 1 → step-stone stoop in front of the door
        //   y = origin.y + 1 → eave row (under-eave shadow + roof start)
        //   y = origin.y + 2..h-2 → interior roof shingles + side walls on edges
        //   y = origin.y + h - 1 → ridge row with finials (overlaid w/ prayer flags optional)
        //   y = origin.y + h - 1 (overlay) → prayer flags / banner overlays
        // The door is a trigger that loads `interiorScene` at `interiorSpawn`.
        static void Building(string name, Vector2Int origin, int w, int h, int doorOffsetX,
                             BuildingStyle style,
                             Color wallColor, Color roofColor,
                             string interiorScene, Vector2 interiorSpawn)
        {
            var root = new GameObject($"Building_{name}").transform;

            bool isPlaster = style == BuildingStyle.Temple;
            bool isStone   = style == BuildingStyle.Shrine;
            bool hasWindows = style == BuildingStyle.Residential
                           || style == BuildingStyle.Temple
                           || style == BuildingStyle.Shop;
            bool hasPrayerFlags = style != BuildingStyle.Hermit;
            bool hasBanner = style == BuildingStyle.Shop;

            // Derived colors
            Color frame    = Darker(wallColor, 0.50f);
            Color glow     = isPlaster ? new Color(0.92f, 0.55f, 0.30f)
                                       : new Color(0.96f, 0.78f, 0.40f);
            Color underEave = Darker(roofColor, 0.55f);
            Color finial    = new Color(0.85f, 0.70f, 0.30f);  // brass finials

            Sprite WallSprite() => isPlaster ? SpriteFactory.WallPlaster(wallColor)
                                  : isStone  ? SpriteFactory.Foundation(wallColor)
                                             : SpriteFactory.WallPlank(wallColor);

            // ── 1) Stoop (in front of door; one tile south of footprint) ──
            SpawnTile(root, $"Stoop_{name}",
                      origin.x + doorOffsetX, origin.y - 1,
                      SpriteFactory.Stoop(new Color(0.55f, 0.50f, 0.45f)),
                      sortingOrder: 1, hasCollider: false);

            // ── 2) Facade row (y = 0) ──
            for (int x = 0; x < w; x++)
            {
                var pos = new Vector3(origin.x + x, origin.y, 0);

                if (x == doorOffsetX)
                {
                    var door = new GameObject($"Door_{name}");
                    door.transform.SetParent(root);
                    door.transform.position = pos;
                    var dsr = door.AddComponent<SpriteRenderer>();
                    dsr.sprite = SpriteFactory.Door(new Color(0.50f, 0.30f, 0.18f));
                    dsr.sortingOrder = 5;
                    var dcol = door.AddComponent<BoxCollider2D>(); dcol.isTrigger = true;
                    var st = door.AddComponent<SceneTransition>();
                    st.targetSceneName = interiorScene;
                    st.spawnPosition = interiorSpawn;
                    st.lockedHint = "the door does not open.";
                    continue;
                }
                if (x == doorOffsetX - 1 || x == doorOffsetX + 1)
                {
                    SpawnTile(root, $"DoorFrame_{x}", origin.x + x, origin.y,
                              SpriteFactory.DoorFrame(new Color(0.60f, 0.40f, 0.22f),
                                                      isLeft: x == doorOffsetX - 1),
                              sortingOrder: 5, hasCollider: true);
                    continue;
                }

                bool isCorner = x == 0 || x == w - 1;
                bool wantWindow = hasWindows && !isCorner;
                Sprite faceSprite = wantWindow
                    ? SpriteFactory.WallWithWindow(wallColor, frame, glow)
                    : WallSprite();
                SpawnTile(root, $"Facade_{x}", origin.x + x, origin.y,
                          faceSprite, sortingOrder: 5, hasCollider: true);
            }

            // ── 3) Side walls + eave row + interior shingles ──
            for (int y = 1; y < h - 1; y++)
            {
                SpawnTile(root, $"WallW_{y}", origin.x,         origin.y + y,
                          WallSprite(), sortingOrder: 5, hasCollider: true);
                SpawnTile(root, $"WallE_{y}", origin.x + w - 1, origin.y + y,
                          WallSprite(), sortingOrder: 5, hasCollider: true);

                for (int x = 1; x < w - 1; x++)
                {
                    Sprite tile = (y == 1)
                        ? SpriteFactory.Eave(roofColor, underEave)
                        : SpriteFactory.RoofShingle(roofColor);
                    SpawnTile(root, $"Roof_{x}_{y}", origin.x + x, origin.y + y,
                              tile, sortingOrder: 6, hasCollider: false);
                }
            }

            // ── 4) Ridge row (y = h - 1) with finials ──
            for (int x = 0; x < w; x++)
            {
                SpawnTile(root, $"Ridge_{x}", origin.x + x, origin.y + h - 1,
                          SpriteFactory.RoofRidge(roofColor, finial),
                          sortingOrder: 6, hasCollider: true);
            }

            // ── 5) Banner above shop door ──
            if (hasBanner)
            {
                SpawnTile(root, $"Banner_{name}",
                          origin.x + doorOffsetX, origin.y + 1,
                          SpriteFactory.Banner(new Color(0.85f, 0.30f, 0.30f),
                                               new Color(0.95f, 0.85f, 0.35f)),
                          sortingOrder: 7, hasCollider: false);
            }

            // ── 6) Prayer flags overlaid on ridge ──
            if (hasPrayerFlags)
            {
                for (int x = 0; x < w; x++)
                {
                    SpawnTile(root, $"Flags_{x}",
                              origin.x + x, origin.y + h - 1,
                              SpriteFactory.PrayerFlags(seed: x + (origin.x & 7)),
                              sortingOrder: 8, hasCollider: false);
                }
            }
        }

        // Spawn a single tile GameObject with sprite + optional collider.
        static void SpawnTile(Transform parent, string name, float x, float y,
                              Sprite sprite, int sortingOrder, bool hasCollider)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            if (hasCollider) go.AddComponent<BoxCollider2D>();
        }

        static Color Darker(Color c, float f)
            => new Color(c.r * (1f - f), c.g * (1f - f), c.b * (1f - f), c.a);

        // Build an interior scene: floor rectangle + walls + south-edge exit
        // door + populate(callback for NPCs/decor).
        static void BuildInterior(string scenePath, string returnScene,
                                  Color cameraBg, Color floorColor,
                                  int width, int height,
                                  Vector2 returnSpawn,
                                  Action populate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(cameraBg.r, cameraBg.g, cameraBg.b);
            MakePlayer(new Vector3(0, -height / 2 + 1, 0));
            MakeUIRoot();

            int halfW = width / 2;
            int halfH = height / 2;

            // Floor tiles
            var floor = new GameObject("Floor").transform;
            for (int y = -halfH + 1; y <= halfH - 1; y++)
            {
                for (int x = -halfW + 1; x <= halfW - 1; x++)
                {
                    var tile = new GameObject($"Floor_{x}_{y}");
                    tile.transform.SetParent(floor);
                    tile.transform.position = new Vector3(x, y, 0);
                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.Floor(floorColor);
                    sr.sortingOrder = 1;
                }
            }

            // Perimeter walls — gap at south (y = -halfH, x = 0) for the exit door
            var walls = new GameObject("Walls").transform;
            for (int x = -halfW; x <= halfW; x++)
            {
                Wall(new Vector2(x,  halfH), walls);
                if (x != 0) Wall(new Vector2(x, -halfH), walls);
            }
            for (int y = -halfH + 1; y <= halfH - 1; y++)
            {
                Wall(new Vector2(-halfW, y), walls);
                Wall(new Vector2( halfW, y), walls);
            }

            // Exit door (south edge, x = 0)
            var door = new GameObject("Door_Exit");
            door.transform.SetParent(walls);
            door.transform.position = new Vector3(0, -halfH, 0);
            var dsr = door.AddComponent<SpriteRenderer>();
            dsr.sprite = SpriteFactory.Door(new Color(0.50f, 0.30f, 0.18f));
            dsr.sortingOrder = 5;
            var dcol = door.AddComponent<BoxCollider2D>(); dcol.isTrigger = true;
            var st = door.AddComponent<SceneTransition>();
            st.targetSceneName = returnScene;
            st.spawnPosition = returnSpawn;

            // Populate with NPCs / decor
            populate?.Invoke();

            EditorSceneManager.SaveScene(scene, scenePath);
            EnsureSceneInBuildSettings(scenePath);
        }

        static void Decor(Vector3 pos, Color color, int kind)
        {
            var go = new GameObject($"Decor_{kind}_{pos.x}_{pos.y}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Decor(color, kind);
            sr.sortingOrder = 5;
            go.AddComponent<BoxCollider2D>();   // blocks the player so decor feels solid
        }

        static void EnsureSceneInBuildSettings(string path)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (current.Exists(s => s.path == path)) return;
            current.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = current.ToArray();
        }

        // Square sprite kept available for future use; SpriteFactory now does the heavy lifting.
        static Sprite MakeSquareSprite(Color color) => SpriteFactory.Solid(color);
    }
}
#endif
