#if UNITY_EDITOR
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
    // M6: builds the three story scenes (Village, Foothills, Approach) +
    // Credits and registers them in Build Settings, plus the dialogue assets
    // each scene needs. Idempotent.
    public static class BuildAllStoryScenes
    {
        const string VillageScenePath    = "Assets/_Project/Scenes/Overworld/Village.unity";
        const string FoothillsScenePath  = "Assets/_Project/Scenes/Overworld/Foothills.unity";
        const string ApproachScenePath   = "Assets/_Project/Scenes/Overworld/Approach.unity";
        const string CreditsScenePath    = "Assets/_Project/Scenes/MainMenu/Credits.unity";

        const string DamaruPath  = "Assets/_Project/Data/Spirits/Spirit_Damaru.asset";
        const string KhyaakPath  = "Assets/_Project/Data/Spirits/Spirit_Khyaak.asset";
        const string YetiPath    = "Assets/_Project/Data/Spirits/Spirit_Yeti.asset";

        const string DialogueDir = "Assets/_Project/Data/Dialogue";

        [MenuItem("Bandhana/Build All Story Scenes (M6)")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureDialogues();

            BuildVillage();
            BuildFoothills();
            BuildApproach();
            BuildCredits();

            EditorUtility.DisplayDialog("Bandhana — M6",
                "Built: Village, Foothills, Approach, Credits.\n" +
                "All scenes registered in Build Settings.\n\n" +
                "Open MainMenu, hit Play, click New Game.\n" +
                "Walk east through Village → Foothills → Approach → Credits.",
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

        static DialogueSO dlgKaruna, dlgDrum, dlgDamaruPre, dlgDamaruSuccess, dlgDamaruFail;
        static DialogueSO dlgVillagerEast, dlgFoothillElder, dlgGateLocked;
        static DialogueSO dlgDevraj, dlgGateDisciple, dlgGatePostDefeat;

        static void EnsureDialogues()
        {
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

            AssetDatabase.SaveAssets();
        }

        // ── Village (Act 0) ──────────────────────────────────────────────────
        static void BuildVillage()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VillageScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.10f, 0.09f, 0.16f);
            var player = MakePlayer(new Vector3(-3, 0, 0));

            var ui = new GameObject("UIRoot");
            ui.AddComponent<PartyMenu>();
            ui.AddComponent<DialogueRunner>();
            ui.AddComponent<PauseMenu>();
            ui.AddComponent<SettingsMenu>();

            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { Wall(new Vector2(-7, y), walls); }
            // Gap in east wall at y=0 for the transition
            for (int y = -4; y <= 4; y++) { if (y != 0) Wall(new Vector2(7, y), walls); }
            // Pyre block (decorative)
            for (int x = -2; x <= 0; x++) Wall(new Vector2(x, 2), walls);

            // Karuna-la's body NPC just south of the pyre (memory dialogue)
            MakeNPC("Karuna-la", new Vector3(-1, 1, 0), new Color(0.85f, 0.55f, 0.45f), dlgKaruna);

            // Villager hint
            MakeNPC("Villager", new Vector3(2, -2, 0), new Color(0.70f, 0.75f, 0.80f), dlgVillagerEast);

            // Drum pickup — interactive NPC that sets a flag (we use NPC for the dialogue, then wrap)
            // For simplicity: any NPC trigger here just plays dialogue. The drum is "picked up"
            // automatically when player walks past (8, 0)? Actually let's gate the east transition
            // on having talked to Karuna-la — auto-set the flag in the post-dialogue, simplest:
            // Instead, set the flag via a separate "DrumPickup" SceneTransition-like trigger.
            MakeFlagSetter("DrumPickup", new Vector3(2, 1, 0), new Color(0.90f, 0.80f, 0.55f),
                "drumCollected", dlgDrum);

            // East transition (gated by drumCollected)
            MakeTransition(new Vector3(7, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Foothills", new Vector2(-6, 0), "drumCollected",
                "the drum is yours. take it before you leave.");

            // Bootstrap Damaru NOT here — Damaru is bonded in Foothills via the Helping.
            // The starting party is empty in Act 0; player has no spirit yet.

            EditorSceneManager.SaveScene(scene, VillageScenePath);
            EnsureSceneInBuildSettings(VillageScenePath);
        }

        // ── Foothills (Act 1 — The Helping) ──────────────────────────────────
        static void BuildFoothills()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FoothillsScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.06f, 0.10f, 0.08f);
            var player = MakePlayer(new Vector3(-6, 0, 0));

            var ui = new GameObject("UIRoot");
            ui.AddComponent<PartyMenu>();
            ui.AddComponent<DialogueRunner>();
            ui.AddComponent<PauseMenu>();
            ui.AddComponent<SettingsMenu>();

            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { if (y != 0) Wall(new Vector2(-7, y), walls); }
            for (int y = -4; y <= 4; y++) { if (y != 0) Wall(new Vector2(7, y), walls); }
            // Some interior boulders
            Wall(new Vector2(-2,  2), walls);
            Wall(new Vector2(-1,  2), walls);
            Wall(new Vector2( 1, -1), walls);
            Wall(new Vector2( 2, -1), walls);

            // Foothill elder gates the way back if you skipped the Helping
            MakeNPC("Foothill Elder", new Vector3(-5, 3, 0), new Color(0.65f, 0.55f, 0.35f), dlgFoothillElder);

            // The Helping — dying Damaru
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

            // Optional Khyaak haunt for tutorial battle (post-Helping)
            var khyaak = AssetDatabase.LoadAssetAtPath<SpiritSO>(KhyaakPath);
            if (khyaak != null)
            {
                var go = new GameObject("SpiritHaunt_Khyaak");
                go.transform.position = new Vector3(4, 2, 0);
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

            // West transition — back to Village
            MakeTransition(new Vector3(-7, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Village", new Vector2(6, 0), null, "");

            // East transition (gated by damaruBonded) → Approach
            MakeTransition(new Vector3(7, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Approach", new Vector2(-6, 0), "damaruBonded",
                "the small one is still in the thornbrake. you cannot leave it.");

            EditorSceneManager.SaveScene(scene, FoothillsScenePath);
            EnsureSceneInBuildSettings(FoothillsScenePath);
        }

        // ── Approach (Act 2 opening) ─────────────────────────────────────────
        static void BuildApproach()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ApproachScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BasicCamera(0.05f, 0.07f, 0.14f);
            MakePlayer(new Vector3(-6, 0, 0));

            var ui = new GameObject("UIRoot");
            ui.AddComponent<PartyMenu>();
            ui.AddComponent<DialogueRunner>();
            ui.AddComponent<PauseMenu>();
            ui.AddComponent<SettingsMenu>();

            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { if (y != 0) Wall(new Vector2(-7, y), walls); Wall(new Vector2(7, y), walls); }
            // Stone columns — gate visual
            Wall(new Vector2(5,  1), walls);
            Wall(new Vector2(5, -1), walls);

            // Devraj cameo (NPC, no battle yet)
            MakeNPC("Devraj", new Vector3(-2, 2, 0), new Color(0.75f, 0.45f, 0.95f), dlgDevraj);

            // Gate disciple — boss BattleNPC
            var yeti = AssetDatabase.LoadAssetAtPath<SpiritSO>(YetiPath);
            if (yeti != null)
            {
                var go = new GameObject("BattleNPC_GateDisciple");
                go.transform.position = new Vector3(4, 0, 0);
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

            // West transition — back to Foothills
            MakeTransition(new Vector3(-7, 0, 0), new Color(0.45f, 0.85f, 0.65f),
                "Foothills", new Vector2(6, 0), null, "");

            EditorSceneManager.SaveScene(scene, ApproachScenePath);
            EnsureSceneInBuildSettings(ApproachScenePath);
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

        static void BasicCamera(float r, float g, float b)
        {
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(r, g, b);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);
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

        // A walk-into trigger that plays dialogue then sets a flag — used for the drum pickup.
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

        static void Wall(Vector2 pos, Transform parent)
        {
            var go = new GameObject($"Wall_{pos.x}_{pos.y}");
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Wall();
            sr.sortingOrder = 5;
            go.AddComponent<BoxCollider2D>();
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
