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
using Bandhana.Story;
using Bandhana.UI;

namespace Bandhana.EditorTools
{
    // Builds the new Lele/Baje/TU-garden opening — 9 scenes from bedroom
    // wakeup through the Karuna astral boss fight. Idempotent. Run from
    // Bandhana/Build Lele Opening.
    public static class BuildOpeningStoryScenes
    {
        const string Dir = "Assets/_Project/Scenes/Story";
        const string DialogueDir = "Assets/_Project/Data/Dialogue";

        // Dialogue refs (filled by EnsureDialogues, consumed by builders)
        static StoryDialogues D = new StoryDialogues();

        class StoryDialogues
        {
            public DialogueSO momShouts, momKitchen, sisterMusicStar, momLunch;
            public DialogueSO karunasDad, karunaTUGarden, damaruPickup;
            public DialogueSO leleAlone, emptyStreet;
            public DialogueSO bajeMeeting, bajeAstralLore, bajeOutside;
            public DialogueSO karunaTwistIntro, karunaPostBoss;

            // Locked-door / hint dialogues
            public DialogueSO doorBedroomLocked, doorKarunaLocked, doorTUGardenLocked;

            // Misc NPCs (sister, dog)
            public DialogueSO sisterIdle;
        }

        [MenuItem("Bandhana/Build Lele Opening")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureDialogues();

            Build_01_Bedroom();
            Build_02_Kitchen();
            Build_03_Town_Day();
            Build_04_KarunaHouse_Day();
            Build_05_TUGarden_Day();
            Build_06_TUGarden_Night();
            Build_07_Town_Night_Astral();
            Build_08_LeleHouse_Astral();
            Build_09_TUGarden_Boss();

            EditorUtility.DisplayDialog("Bandhana — Lele Opening",
                "Built 9 opening scenes under " + Dir + ".\n\n" +
                "To play: open 01_LeleBedroom_Wakeup and press Play.\n" +
                "Move with WASD/arrows. E to interact.",
                "OK");
        }

        // ── Dialogues ────────────────────────────────────────────────────────
        static DialogueSO Dlg(string fileName, params (string speaker, string text)[] lines)
        {
            Directory.CreateDirectory(DialogueDir);
            var path = $"{DialogueDir}/{fileName}.asset";
            var d = AssetDatabase.LoadAssetAtPath<DialogueSO>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<DialogueSO>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.lines.Clear();
            foreach (var (s, t) in lines) d.lines.Add(new DialogueLine { speaker = s, text = t });
            EditorUtility.SetDirty(d);
            return d;
        }

        static void EnsureDialogues()
        {
            D = new StoryDialogues();

            D.momShouts = Dlg("Dialogue_Lele_MomShouts",
                ("Mom (downstairs)", "Lele! It's almost three in the afternoon!"),
                ("Mom (downstairs)", "Were you on that game all night again? Get down here!"),
                ("", "You sit up. The bedroom is too bright. Your eyes hurt."));

            D.momKitchen = Dlg("Dialogue_Lele_MomKitchen",
                ("Mom", "There you are. The whole village heard me calling."),
                ("Mom", "Your father is doing his sadhana. Don't go knocking on that door."),
                ("Mom", "Karuna came by earlier — said she found something yesterday evening, wants to show you."));

            D.sisterMusicStar = Dlg("Dialogue_Lele_Sister",
                ("Sister", "Bhaiya! When I'm older I'm going to be a music star."),
                ("Sister", "Like really really famous. People will paint my face on motorbikes."),
                ("Sister", "You can be in my videos. Maybe. If you're nice."));

            D.momLunch = Dlg("Dialogue_Lele_MomLunch",
                ("Mom", "Sit. Eat. Daal-bhaat is still warm."),
                ("", "You eat. The daal is good. Your sister is still talking about motorbikes."),
                ("Mom", "When you're done — go find Karuna. And come back before dark."));

            D.karunasDad = Dlg("Dialogue_KarunasDad",
                ("Karuna's Dad", "Oh — Lele. She was looking for you this morning."),
                ("Karuna's Dad", "She went to the TU garden with that dog of hers."),
                ("Karuna's Dad", "Said she had something to show you. Drum-shaped, I think? Strange thing."));

            D.karunaTUGarden = Dlg("Dialogue_KarunaTU",
                ("Karuna", "There you are! Took you long enough."),
                ("Karuna", "I found this in front of my house yesterday evening."),
                ("Karuna", "It's a damaru — like the ones the sadhus carry."),
                ("Karuna", "Bhotu won't stop barking at it. Look — go on. Try it."));

            D.damaruPickup = Dlg("Dialogue_DamaruPlay",
                ("", "You pick up the damaru. The skin is warm — warmer than it should be."),
                ("", "You twist your wrist. The beads strike."),
                ("", "Dum… dum… dum…"));

            D.leleAlone = Dlg("Dialogue_TUNight",
                ("", "You wake. The garden is dim. The sky is the wrong color."),
                ("", "Karuna is gone. Bhotu is gone. The damaru is gone."),
                ("", "How long were you out? It feels like evening already."));

            D.emptyStreet = Dlg("Dialogue_EmptyStreet",
                ("", "The street is empty. No motorbikes. No tea-shop chatter."),
                ("", "A dog stands at the corner without moving. Its eyes don't follow you."),
                ("", "You should check on your family."));

            D.bajeMeeting = Dlg("Dialogue_BajeMeeting",
                ("Lele", "Hey! What are you doing in my house?!"),
                ("Old Man", "You're here. You're finally here."),
                ("Old Man", "You look just like him."),
                ("Lele", "What — this is my home. Where are my mom and sister?"),
                ("Lele", "I'll call the police. I'll —"));

            D.bajeAstralLore = Dlg("Dialogue_BajeAstralLore",
                ("Old Man", "Your phone won't work here, child. Sit down."),
                ("Old Man", "When you struck the damaru, you flipped the state of the world."),
                ("Old Man", "The physical and the astral — most never know there is a difference."),
                ("Old Man", "Your body is in the astral now. Mine has been here a long time."),
                ("Old Man", "I am called Baje. The damaru pulled you across. We must find it again."),
                ("Old Man", "But understand: each time the damaru is struck, the boundary thins."),
                ("Old Man", "Something on the other side has noticed you."));

            D.bajeOutside = Dlg("Dialogue_BajeOutside",
                ("Baje", "Stay close. Do not let them see you."),
                ("Baje", "In this state you are unfinished. They are drawn to that."),
                ("Lele", "…that figure at the end of the street. Who is it?"),
                ("Baje", "Don't ask. Move. Karuna may not still be Karuna."));

            D.karunaTwistIntro = Dlg("Dialogue_KarunaTwist",
                ("Karuna", "You took long enough."),
                ("Lele", "Karuna — what —"),
                ("Karuna", "I didn't find this. It was waiting."),
                ("Karuna", "Listen. Isn't it clearer here?"),
                ("Baje", "She isn't attacking you. She's anchoring it. Break the rhythm."),
                ("Baje", "Match her beat — then disrupt it. Press SPACE between her drums."));

            D.karunaPostBoss = Dlg("Dialogue_KarunaPostBoss",
                ("Karuna", "Lele…?"),
                ("Karuna", "I couldn't stop hearing it…"),
                ("", "The damaru cracks faintly — light leaks from inside."),
                ("Baje", "You didn't win. You interrupted it."),
                ("Baje", "Now it knows you can interfere."));

            D.doorBedroomLocked = Dlg("Dialogue_BedroomDoorLocked",
                ("", "Mom is still shouting. Better wake up first."));
            D.doorKarunaLocked = Dlg("Dialogue_KarunaDoorLocked",
                ("", "The door is shut. You should head outside first."));
            D.doorTUGardenLocked = Dlg("Dialogue_TUDoorLocked",
                ("", "The path north-west leads to the TU garden."));

            D.sisterIdle = Dlg("Dialogue_SisterIdle",
                ("Sister", "Bhaiya, do you know any songs? I know seven."),
                ("Sister", "I forgot four of them. But the other three are amazing."));

            AssetDatabase.SaveAssets();
        }

        // ── Scene 01: bedroom wakeup ─────────────────────────────────────────
        static void Build_01_Bedroom()
        {
            var scene = NewScene("01_LeleBedroom_Wakeup");
            BasicCamera(0.10f, 0.09f, 0.16f);
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, 0, 0));

            // Small bedroom: 9 wide x 7 tall
            BuildPerimeter(4, 3, southGapX: 0);

            // Bed (block of 3 walls)
            Wall(new Vector2(-3, 1)); Wall(new Vector2(-2, 1)); Wall(new Vector2(-3, 2));
            // Desk
            Wall(new Vector2( 3, 1)); Wall(new Vector2( 3, 2));

            // Auto-trigger near the bed: mom shouts (fires once when player steps onto tile)
            MakeAutoTrigger("Trigger_MomShouts", new Vector3(0, 1, 0),
                            OpeningBeat.MomShouts, completionFlag: "woke");

            // South-edge door — locked until "woke" flag set
            MakeTransition(new Vector3(0, -3, 0),
                "02_LeleHouse_Kitchen", new Vector2(0, 3),
                requiredFlag: "woke", lockedHint: "mom is still shouting. better wake up first.");

            SaveAndRegister(scene, "01_LeleBedroom_Wakeup");
        }

        // ── Scene 02: kitchen ────────────────────────────────────────────────
        static void Build_02_Kitchen()
        {
            var scene = NewScene("02_LeleHouse_Kitchen");
            BasicCamera(0.08f, 0.07f, 0.14f);
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, 3, 0));

            BuildPerimeter(6, 4, southGapX: 0);

            // Stove block (NW corner)
            Wall(new Vector2(-5, 3)); Wall(new Vector2(-4, 3)); Wall(new Vector2(-5, 2));

            // Mom NPC (interactable; pressing E triggers the kitchen cutscene)
            MakeBeatNPC("Mom", new Vector3(-3, 2, 0), new Color(0.85f, 0.55f, 0.45f),
                        OpeningBeat.MomKitchen, completionFlag: "ateLunch");

            // Sister NPC — idle dialogue (just an NPC with DialogueSO)
            MakeNPC("Sister", new Vector3(2, 0, 0), new Color(0.95f, 0.70f, 0.55f), D.sisterIdle);

            // Table decor
            Decor(new Vector3( 0, 0, 0), new Color(0.55f, 0.40f, 0.25f), 2);  // bench/table

            // South door to town — gated by "ateLunch"
            MakeTransition(new Vector3(0, -4, 0),
                "03_Town_Day", new Vector2(0, 5),
                requiredFlag: "ateLunch",
                lockedHint: "you should eat first. mom made daal-bhaat.");

            SaveAndRegister(scene, "02_LeleHouse_Kitchen");
        }

        // ── Scene 03: town (day) ─────────────────────────────────────────────
        static void Build_03_Town_Day()
        {
            var scene = NewScene("03_Town_Day");
            BasicCamera(0.30f, 0.42f, 0.35f);  // sunny day green
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, 5, 0));

            BuildPerimeter(12, 8);

            // Lele's house (north — door re-enters kitchen)
            Building("LeleHouse", new Vector2Int(-3, 5), 6, 3, 3,
                     wallColor: new Color(0.78f, 0.62f, 0.42f),
                     roofColor: new Color(0.58f, 0.30f, 0.22f),
                     interiorScene: "02_LeleHouse_Kitchen", interiorSpawn: new Vector2(0, 3));

            // Karuna's house (east) — door enters Karuna house scene
            Building("KarunaHouse", new Vector2Int(7, -2), 5, 3, 2,
                     wallColor: new Color(0.72f, 0.55f, 0.32f),
                     roofColor: new Color(0.45f, 0.32f, 0.20f),
                     interiorScene: "04_KarunaHouse_Day", interiorSpawn: new Vector2(0, 3));

            // TU garden gate (NW corner) — leads to scene 05
            MakeTransition(new Vector3(-12, 6, 0),
                "05_TUGarden_Day", new Vector2(0, -5),
                requiredFlag: null, lockedHint: "");

            // Decor
            Decor(new Vector3(-6, -2, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3( 4,  3, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3( 9, -6, 0), new Color(0.50f, 0.50f, 0.50f), 1);

            SaveAndRegister(scene, "03_Town_Day");
        }

        // ── Scene 04: Karuna's house (day) ───────────────────────────────────
        static void Build_04_KarunaHouse_Day()
        {
            var scene = NewScene("04_KarunaHouse_Day");
            BasicCamera(0.14f, 0.10f, 0.07f);
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, 3, 0));

            BuildPerimeter(5, 4, southGapX: 0);

            MakeBeatNPC("KarunaDad", new Vector3(-2, 1, 0),
                        new Color(0.55f, 0.45f, 0.35f),
                        OpeningBeat.KarunaDad, completionFlag: "knowsKarunaAtTU");

            // Decor
            Decor(new Vector3( 2, 1, 0), new Color(0.40f, 0.55f, 0.30f), 0);
            Decor(new Vector3( 0,-2, 0), new Color(0.50f, 0.40f, 0.25f), 2);

            // South door back to town
            MakeTransition(new Vector3(0, -4, 0),
                "03_Town_Day", new Vector2(7, -3),
                requiredFlag: null, lockedHint: "");

            SaveAndRegister(scene, "04_KarunaHouse_Day");
        }

        // ── Scene 05: TU garden (day) ────────────────────────────────────────
        static void Build_05_TUGarden_Day()
        {
            var scene = NewScene("05_TUGarden_Day");
            BasicCamera(0.18f, 0.30f, 0.18f);  // deeper green
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, -5, 0));

            BuildPerimeter(10, 7, southGapX: 0);

            // Trees
            for (int i = 0; i < 6; i++) Wall(new Vector2(-8 + i * 2, 5));
            Wall(new Vector2(-6, -2)); Wall(new Vector2(6, -2));
            Wall(new Vector2(-7, 2));  Wall(new Vector2(7, 2));

            // Karuna NPC — first time triggers KarunaTUDay; afterward she becomes
            // a damaru pickup once metKarunaAtTU set. We split into two GameObjects.
            MakeBeatNPC("Karuna_Day", new Vector3(-1, 0, 0),
                        new Color(0.85f, 0.55f, 0.45f),
                        OpeningBeat.KarunaTUDay, completionFlag: "metKarunaAtTU");

            // Bhotu (Karuna's dog) — purely flavor NPC
            MakeNPC("Bhotu", new Vector3(0, -1, 0), new Color(0.80f, 0.65f, 0.35f),
                Dlg("Dialogue_Bhotu", ("Bhotu", "*growls at the small drum*")));

            // Damaru — interactable. Only available after metKarunaAtTU. On
            // interact runs PlayDamaruFirstTime which fades to black and loads
            // 06_TUGarden_Night.
            MakeBeatInteractable("Damaru_Day", new Vector3(1, 0, 0),
                                 SpriteFactory.Drum(new Color(0.95f, 0.85f, 0.55f)),
                                 OpeningBeat.PlayDamaruFirstTime,
                                 requiredFlag: "metKarunaAtTU",
                                 forbiddenFlag: "damaruPlayedOnce");

            // South gate back to town
            MakeTransition(new Vector3(0, -7, 0),
                "03_Town_Day", new Vector2(-11, 5),
                requiredFlag: null, lockedHint: "");

            SaveAndRegister(scene, "05_TUGarden_Day");
        }

        // ── Scene 06: TU garden (night, alone) ───────────────────────────────
        static void Build_06_TUGarden_Night()
        {
            var scene = NewScene("06_TUGarden_Night");
            BasicCamera(0.06f, 0.05f, 0.14f);  // indigo night
            MakeUIRoot();
            MakeStoryRoot(astralOnLoad: true);
            MakePlayer(new Vector3(0, -3, 0));

            BuildPerimeter(10, 7, southGapX: 0);

            // Twisted trees — same wall positions but spaced as if grown
            for (int i = 0; i < 6; i++) Wall(new Vector2(-8 + i * 2, 5));
            Wall(new Vector2(-6, -2)); Wall(new Vector2(6, -2));
            Wall(new Vector2(-7, 2));  Wall(new Vector2(7, 2));

            // Auto-trigger right where the player spawns: leleAlone monologue
            MakeAutoTrigger("Trigger_LeleAlone", new Vector3(0, -3, 0),
                            OpeningBeat.TUNightWake, completionFlag: "wokeAlone");

            // South gate to astral town
            MakeTransition(new Vector3(0, -7, 0),
                "07_Town_Night_Astral", new Vector2(0, 7),
                requiredFlag: "wokeAlone",
                lockedHint: "your head is still ringing.");

            SaveAndRegister(scene, "06_TUGarden_Night");
        }

        // ── Scene 07: town (night, astral) ───────────────────────────────────
        static void Build_07_Town_Night_Astral()
        {
            var scene = NewScene("07_Town_Night_Astral");
            BasicCamera(0.05f, 0.04f, 0.12f);
            MakeUIRoot();
            MakeStoryRoot(astralOnLoad: true);
            MakePlayer(new Vector3(0, 7, 0));

            BuildPerimeter(12, 8);

            // Lele's house (north) — entering loads the astral interior
            Building("LeleHouse_Astral", new Vector2Int(-3, 5), 6, 3, 3,
                     wallColor: new Color(0.42f, 0.32f, 0.45f),
                     roofColor: new Color(0.28f, 0.18f, 0.30f),
                     interiorScene: "08_LeleHouse_Astral", interiorSpawn: new Vector2(0, -3));

            // Auto-trigger near spawn — first emptyStreet impression
            MakeAutoTrigger("Trigger_EmptyStreet", new Vector3(0, 5, 0),
                            OpeningBeat.EmptyStreet, completionFlag: "sawAstralStreet");

            // Distorted figure walking at the south end of the street
            MakeAmbientFigure(new Vector3(0, -6, 0));

            // A still dog — purely visual flavor (NPC with one line)
            MakeNPC("StillDog", new Vector3(-6, 0, 0), new Color(0.55f, 0.40f, 0.25f),
                Dlg("Dialogue_StillDog", ("", "The dog stands without breathing. Its eyes don't move.")));

            // After visiting Baje, the player is sent to TU garden for the boss.
            // We expose the TU gate (NW) gated on metBaje.
            MakeTransition(new Vector3(-12, 6, 0),
                "09_TUGarden_Boss", new Vector2(0, -6),
                requiredFlag: "metBaje",
                lockedHint: "the path is here, but baje wanted to talk first. check the house.");

            SaveAndRegister(scene, "07_Town_Night_Astral");
        }

        // ── Scene 08: Lele's house — astral, find Baje ───────────────────────
        static void Build_08_LeleHouse_Astral()
        {
            var scene = NewScene("08_LeleHouse_Astral");
            BasicCamera(0.07f, 0.05f, 0.10f);
            MakeUIRoot();
            MakeStoryRoot(astralOnLoad: true);
            MakePlayer(new Vector3(0, -3, 0));

            BuildPerimeter(6, 4, southGapX: 0);

            // The "bed" again — Baje is searching it
            Wall(new Vector2(-3, 1)); Wall(new Vector2(-2, 1)); Wall(new Vector2(-3, 2));

            // Baje NPC — first interaction triggers BajeAppears, then he becomes
            // a regular interact that fires BajeOutside (the going-out hint).
            MakeBeatNPC("Baje", new Vector3(-1, 1, 0),
                        new Color(0.65f, 0.55f, 0.35f),
                        OpeningBeat.BajeAppears, completionFlag: "metBaje");

            // After metBaje, a separate auto-trigger near the door fires the
            // outside-warning monologue once.
            MakeAutoTrigger("Trigger_BajeOutside", new Vector3(0, -3, 0),
                            OpeningBeat.BajeOutside, completionFlag: "bajeWarned",
                            requiredFlag: "metBaje");

            // South door back to astral street
            MakeTransition(new Vector3(0, -4, 0),
                "07_Town_Night_Astral", new Vector2(0, 5),
                requiredFlag: null, lockedHint: "");

            SaveAndRegister(scene, "08_LeleHouse_Astral");
        }

        // ── Scene 09: TU garden — boss fight ─────────────────────────────────
        static void Build_09_TUGarden_Boss()
        {
            var scene = NewScene("09_TUGarden_Boss");
            BasicCamera(0.06f, 0.04f, 0.10f);
            MakeUIRoot();
            MakeStoryRoot(astralOnLoad: true);
            MakePlayer(new Vector3(0, -3, 0));

            BuildPerimeter(10, 7, southGapX: 0);

            // Twisted trees, denser
            for (int i = 0; i < 6; i++) Wall(new Vector2(-8 + i * 2, 5));
            Wall(new Vector2(-7,  3)); Wall(new Vector2(7,  3));
            Wall(new Vector2(-6,  0)); Wall(new Vector2(6,  0));
            Wall(new Vector2(-7, -2)); Wall(new Vector2(7, -2));

            // The RhythmBoss singleton lives on its own GameObject; the boss
            // intro NPC (Karuna again, but altered) fires KarunaTwistIntro on
            // interact, which finds the boss and runs it.
            var bossGO = new GameObject("RhythmBoss");
            bossGO.AddComponent<RhythmBossKaruna>();

            MakeBeatNPC("Karuna_Boss", new Vector3(0, 1, 0),
                        new Color(0.70f, 0.30f, 0.55f),
                        OpeningBeat.KarunaTwistIntro, completionFlag: "bossInterrupted");

            // South gate back to astral town (only re-opens after boss interrupted)
            MakeTransition(new Vector3(0, -7, 0),
                "07_Town_Night_Astral", new Vector2(-11, 6),
                requiredFlag: "bossInterrupted",
                lockedHint: "the trees lean inward. you can't leave yet.");

            SaveAndRegister(scene, "09_TUGarden_Boss");
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        static UnityEngine.SceneManagement.Scene NewScene(string name)
        {
            Directory.CreateDirectory(Dir);
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        static void SaveAndRegister(UnityEngine.SceneManagement.Scene scene, string name)
        {
            var path = $"{Dir}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            EnsureSceneInBuildSettings(path);
        }

        static void EnsureSceneInBuildSettings(string path)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (current.Exists(s => s.path == path)) return;
            current.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = current.ToArray();
        }

        static void BasicCamera(float r, float g, float b)
        {
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.backgroundColor = new Color(r, g, b);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);
        }

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
            ui.AddComponent<DialogueRunner>();
            ui.AddComponent<PauseMenu>();
            ui.AddComponent<SettingsMenu>();
        }

        // The persistent singletons (CutsceneRunner, ScreenFader) survive scene
        // loads via DontDestroyOnLoad. Their second-instance copies destroy
        // their own GameObjects on Awake — so anything on those GameObjects
        // dies too. To keep per-scene wiring (StoryAssets, RealmInitializer)
        // alive on each new scene load, put it on its own GameObject.
        static void MakeStoryRoot(bool astralOnLoad = false)
        {
            // Persistent singletons — exactly one survives across scenes.
            var singletons = new GameObject("StorySingletons");
            singletons.AddComponent<CutsceneRunner>();
            singletons.AddComponent<ScreenFader>();

            // Per-scene assets/initializer — duplicates of CutsceneRunner's
            // GameObject would have been Destroy()d on Awake; this GameObject
            // is independent so it persists for this scene only.
            var perScene = new GameObject("SceneStory");
            var assets = perScene.AddComponent<StoryAssets>();
            assets.momShouts        = D.momShouts;
            assets.momKitchen       = D.momKitchen;
            assets.sisterMusicStar  = D.sisterMusicStar;
            assets.momLunch         = D.momLunch;
            assets.karunasDad       = D.karunasDad;
            assets.karunaTUGarden   = D.karunaTUGarden;
            assets.damaruPickup     = D.damaruPickup;
            assets.leleAlone        = D.leleAlone;
            assets.emptyStreet      = D.emptyStreet;
            assets.bajeMeeting      = D.bajeMeeting;
            assets.bajeAstralLore   = D.bajeAstralLore;
            assets.bajeOutside      = D.bajeOutside;
            assets.karunaTwistIntro = D.karunaTwistIntro;
            assets.karunaPostBoss   = D.karunaPostBoss;
            perScene.AddComponent<RealmInitializer>().setTo =
                astralOnLoad ? WorldRealm.Astral : WorldRealm.Physical;
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

        // Build a rectangular outer perimeter centered at (0,0) with optional
        // openings at x = southGapX/northGapX (use int.MinValue for "no gap")
        // and y = westGapY/eastGapY.
        static void BuildPerimeter(int halfW, int halfH,
                                   int northGapX = int.MinValue, int southGapX = int.MinValue,
                                   int westGapY = int.MinValue, int eastGapY = int.MinValue)
        {
            var walls = new GameObject("Walls").transform;
            for (int x = -halfW; x <= halfW; x++)
            {
                if (x != northGapX) Wall(new Vector2(x,  halfH), walls);
                if (x != southGapX) Wall(new Vector2(x, -halfH), walls);
            }
            for (int y = -halfH + 1; y <= halfH - 1; y++)
            {
                if (y != westGapY) Wall(new Vector2(-halfW, y), walls);
                if (y != eastGapY) Wall(new Vector2( halfW, y), walls);
            }
        }

        static void MakeNPC(string name, Vector3 pos, Color color, DialogueSO dialogue)
        {
            var go = new GameObject($"NPC_{name}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(color);
            sr.sortingOrder = 6;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one;
            var npc = go.AddComponent<NPC>();
            npc.npcName = name;
            npc.dialogue = dialogue;
        }

        // NPC that runs an OpeningBeat instead of a plain dialogue. The cutscene
        // itself plays the dialogue. Optional completionFlag prevents re-firing.
        static void MakeBeatNPC(string name, Vector3 pos, Color color,
                                OpeningBeat beat, string completionFlag)
        {
            var go = new GameObject($"NPC_{name}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(color);
            sr.sortingOrder = 6;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one;
            var inter = go.AddComponent<Interactable>();
            inter.completionFlag = completionFlag;
            inter.forbiddenFlag = completionFlag;
            var inv = go.AddComponent<OpeningInvoker>();
            inv.beat = beat;
        }

        // Generic interactable (e.g. damaru pickup). Sprite chosen by caller.
        static void MakeBeatInteractable(string name, Vector3 pos, Sprite sprite,
                                         OpeningBeat beat,
                                         string requiredFlag = null,
                                         string forbiddenFlag = null)
        {
            var go = new GameObject($"Interact_{name}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 6;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one;
            var inter = go.AddComponent<Interactable>();
            inter.requiredFlag = requiredFlag;
            inter.forbiddenFlag = forbiddenFlag;
            var inv = go.AddComponent<OpeningInvoker>();
            inv.beat = beat;
        }

        // Walk-on trigger that runs an OpeningBeat once.
        static void MakeAutoTrigger(string name, Vector3 pos,
                                    OpeningBeat beat, string completionFlag,
                                    string requiredFlag = null)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            // Invisible — small trigger collider only
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one * 0.6f;
            var auto = go.AddComponent<AutoCutsceneTrigger>();
            auto.requiredFlag = requiredFlag;
            auto.forbiddenFlag = completionFlag;
            auto.completionFlag = completionFlag;
            var inv = go.AddComponent<OpeningInvoker>();
            inv.beat = beat;
        }

        static void MakeAmbientFigure(Vector3 pos)
        {
            var go = new GameObject("AstralFigure");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(new Color(0.20f, 0.18f, 0.30f));
            sr.sortingOrder = 6;
            // No collider — purely visual, can't be approached
            var w = go.AddComponent<AmbientWalker>();
            w.minX = -8f; w.maxX = 8f; w.speed = 0.4f;
            // Stretched proportions — set scale to suggest "too tall"
            go.transform.localScale = new Vector3(0.9f, 1.6f, 1f);
        }

        static void MakeTransition(Vector3 pos,
                                   string targetScene, Vector2 spawn,
                                   string requiredFlag, string lockedHint)
        {
            var go = new GameObject($"Transition_to_{targetScene}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Transition(new Color(0.45f, 0.85f, 0.65f));
            sr.sortingOrder = 4;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one;
            var st = go.AddComponent<SceneTransition>();
            st.targetSceneName = targetScene;
            st.spawnPosition = spawn;
            st.requiredFlag = requiredFlag;
            if (!string.IsNullOrEmpty(lockedHint)) st.lockedHint = lockedHint;
        }

        // Building exterior + door that loads an interior scene at a given spawn.
        static void Building(string name, Vector2Int origin, int w, int h, int doorOffsetX,
                             Color wallColor, Color roofColor,
                             string interiorScene, Vector2 interiorSpawn)
        {
            var root = new GameObject($"Building_{name}").transform;
            Color frame    = Darker(wallColor, 0.50f);
            Color glow     = new Color(0.96f, 0.78f, 0.40f);
            Color underEave = Darker(roofColor, 0.55f);
            Color finial    = new Color(0.85f, 0.70f, 0.30f);

            // Stoop
            SpawnTile(root, $"Stoop_{name}", origin.x + doorOffsetX, origin.y - 1,
                      SpriteFactory.Stoop(new Color(0.55f, 0.50f, 0.45f)), 1, false);

            // Facade row
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
                    continue;
                }
                if (x == doorOffsetX - 1 || x == doorOffsetX + 1)
                {
                    SpawnTile(root, $"DoorFrame_{x}", origin.x + x, origin.y,
                              SpriteFactory.DoorFrame(new Color(0.60f, 0.40f, 0.22f),
                                                      isLeft: x == doorOffsetX - 1),
                              5, true);
                    continue;
                }
                bool isCorner = x == 0 || x == w - 1;
                SpawnTile(root, $"Facade_{x}", origin.x + x, origin.y,
                          isCorner ? SpriteFactory.WallPlank(wallColor)
                                   : SpriteFactory.WallWithWindow(wallColor, frame, glow),
                          5, true);
            }

            // Side walls + eaves + interior shingles
            for (int y = 1; y < h - 1; y++)
            {
                SpawnTile(root, $"WallW_{y}", origin.x,         origin.y + y, SpriteFactory.WallPlank(wallColor), 5, true);
                SpawnTile(root, $"WallE_{y}", origin.x + w - 1, origin.y + y, SpriteFactory.WallPlank(wallColor), 5, true);
                for (int x = 1; x < w - 1; x++)
                {
                    Sprite tile = (y == 1)
                        ? SpriteFactory.Eave(roofColor, underEave)
                        : SpriteFactory.RoofShingle(roofColor);
                    SpawnTile(root, $"Roof_{x}_{y}", origin.x + x, origin.y + y, tile, 6, false);
                }
            }
            // Ridge
            for (int x = 0; x < w; x++)
                SpawnTile(root, $"Ridge_{x}", origin.x + x, origin.y + h - 1,
                          SpriteFactory.RoofRidge(roofColor, finial), 6, true);
        }

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

        static void Decor(Vector3 pos, Color color, int kind)
        {
            var go = new GameObject($"Decor_{kind}_{pos.x}_{pos.y}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Decor(color, kind);
            sr.sortingOrder = 5;
            go.AddComponent<BoxCollider2D>();
        }

        static Color Darker(Color c, float f)
            => new Color(c.r * (1f - f), c.g * (1f - f), c.b * (1f - f), c.a);
    }

}
#endif
