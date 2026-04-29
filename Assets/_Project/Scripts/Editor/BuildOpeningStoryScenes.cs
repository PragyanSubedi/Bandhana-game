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
            public DialogueSO momShouts, momKitchenIntro, momKitchenAfter, sisterMusicStar;
            public DialogueSO momLunchInvite, momLunchEat;
            public DialogueSO karunasDad, karunaTUGarden, damaruPickup;
            public DialogueSO leleAlone, emptyStreet;
            public DialogueSO bajeMeeting, bajeAstralLore, bajeOutside;
            public DialogueSO karunaTwistIntro, karunaPostBoss;

            // Locked-door / hint dialogues
            public DialogueSO doorBedroomLocked, doorKarunaLocked, doorTUGardenLocked;
            public DialogueSO parentsDoorLocked;

            // Misc NPCs (sister, dog)
            public DialogueSO sisterIdle;
            public DialogueSO momIdleAfterLunch;
        }

        [MenuItem("Bandhana/Build Lele Opening")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAllSilent();
            EditorUtility.DisplayDialog("Bandhana — Lele Opening",
                "Built 9 opening scenes under " + Dir + ".\n\n" +
                "To play: open 01_LeleBedroom_Wakeup and press Play.\n" +
                "Move with WASD/arrows. E to interact.",
                "OK");
        }

        // Same as BuildAll but no save-prompt and no popup — used by the
        // auto-rebuild-on-reload hook so iteration is silent.
        public static void BuildAllSilent()
        {
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
                ("Mom (downstairs)", "Lele!… Lele!!"),
                ("Mom (downstairs)", "It's almost three in the afternoon!"),
                ("Mom (downstairs)", "Were you on that game all night again? Get down here!"),
                ("", "You sit up. The bedroom is too bright. Your eyes hurt."));

            // Split into two parts so Sister can walk over to Lele in between,
            // arriving just as her speaking turn begins.
            D.momKitchenIntro = Dlg("Dialogue_Lele_MomKitchenIntro",
                ("Mom", "Look at this face. Look at it. Two o'clock and his hair is its own country."),
                ("Mom", "Three times I called. Three! By the fourth, I was going to send your sister up with the broom."));

            D.momKitchenAfter = Dlg("Dialogue_Lele_MomKitchenAfter",
                ("Sister", "I volunteered, <i>Dai</i> (big brother). I had a strategy."),
                ("Mom", "Hush. Your father is in his room doing <i>sadhana</i> (spiritual practice). Don't even breathe near that door."),
                ("Mom", "Last week you knocked and he lost focus. He growled at the rice for an hour."),
                ("Sister", "<i>He growled at the rice.</i>"),
                ("Mom", "Stop repeating things. Karuna came by this morning, just after the milkman."),
                ("Mom", "She said she found something yesterday evening. Wouldn't even put it down to drink tea."),
                ("Mom", "I asked what. She said 'Lele needs to see it first.' Mysterious girl."));

            D.sisterMusicStar = Dlg("Dialogue_Lele_Sister",
                ("Sister", "<i>Dai. Dai. DAI.</i> I have an announcement."),
                ("Sister", "When I am older, like eleven, I am going to be a music star."),
                ("Sister", "Like really really famous. Bigger than that one with the hair. The hair one."),
                ("Sister", "People will paint my face on motorbikes. And buses. Maybe one airplane."),
                ("Mom", "Eat your dal."),
                ("Sister", "I'm describing my future, Mama!"),
                ("Mom", "Describe it with food in your mouth."),
                ("Sister", "(whispers) You can be in my videos, <i>Dai</i>. As a backup person. Standing behind me. Mostly."),
                ("Sister", "But you have to fix your hair first. It looks like a goat slept on it."));

            // Split: Mom invites them to the table → all three walk over →
            // narrator notes them sitting + the rest of the meal banter.
            D.momLunchInvite = Dlg("Dialogue_Lele_MomLunchInvite",
                ("Mom", "Sit. The lentil soup is still warm. Rice from this morning, but I added butter."),
                ("Mom", "There's spinach too. Don't make a face. I saw the face."));

            D.momLunchEat = Dlg("Dialogue_Lele_MomLunchEat",
                ("", "You sit. The lentil soup is, annoyingly, very good."),
                ("Sister", "Mama, when I'm famous I'll buy you a fridge that talks."),
                ("Mom", "I don't want a fridge that talks. I have you."),
                ("Sister", "That's mean and also funny. I'm putting it in a song."),
                ("", "You finish. Mom slides a glass of water across the table without looking."),
                ("Mom", "Now go find Karuna before she comes back here and starts knocking again."),
                ("Mom", "And be home before dark. Your father will surface around then. He'll want to see you."));

            D.karunasDad = Dlg("Dialogue_KarunasDad",
                ("Karuna's Dad", "Oh, Lele. She was looking for you this morning."),
                ("Karuna's Dad", "She went to the TU garden with that dog of hers."),
                ("Karuna's Dad", "Said she had something to show you. Shaped like a drum, I think? Strange thing."));

            D.karunaTUGarden = Dlg("Dialogue_KarunaTU",
                ("Karuna", "There you are! Took you long enough."),
                ("Karuna", "I found this in front of my house yesterday evening."),
                ("Karuna", "It's a damaru, like the small drums the wandering monks carry."),
                ("Karuna", "Bhotu won't stop barking at it. Look. Go on. Try it."));

            D.damaruPickup = Dlg("Dialogue_DamaruPlay",
                ("", "You pick up the damaru. The skin is warm. Warmer than it should be."),
                ("", "You twist your wrist. The beads strike."),
                ("", "Dum… dum… dum…"));

            D.leleAlone = Dlg("Dialogue_TUNight",
                ("", "You wake. The garden is dim. The sky is the wrong color."),
                ("", "Karuna is gone. Bhotu is gone. The damaru is gone."),
                ("", "How long were you out? It feels like evening already."));

            D.emptyStreet = Dlg("Dialogue_EmptyStreet",
                ("", "The street is empty. No motorbikes. No tea shop chatter."),
                ("", "A dog stands at the corner without moving. Its eyes don't follow you."),
                ("", "You should check on your family."));

            D.bajeMeeting = Dlg("Dialogue_BajeMeeting",
                ("Lele", "Hey! What are you doing in my house?!"),
                ("Old Man", "You're here. You're finally here."),
                ("Old Man", "You look just like him."),
                ("Lele", "What… this is my home. Where are my mom and sister?"),
                ("Lele", "I'll call the police. I'll…"));

            D.bajeAstralLore = Dlg("Dialogue_BajeAstralLore",
                ("Old Man", "Your phone won't work here, child. Sit down."),
                ("Old Man", "When you struck the damaru, you flipped the state of the world."),
                ("Old Man", "The physical and the astral. Most never know there is a difference."),
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
                ("Lele", "Karuna… what…"),
                ("Karuna", "I didn't find this. It was waiting."),
                ("Karuna", "Listen. Isn't it clearer here?"),
                ("Baje", "She isn't attacking you. She's anchoring it. Break the rhythm."),
                ("Baje", "Match her beat. Then disrupt it. Press SPACE between her drums."));

            D.karunaPostBoss = Dlg("Dialogue_KarunaPostBoss",
                ("Karuna", "Lele…?"),
                ("Karuna", "I couldn't stop hearing it…"),
                ("", "The damaru cracks faintly. Light leaks from inside."),
                ("Baje", "You didn't win. You interrupted it."),
                ("Baje", "Now it knows you can interfere."));

            D.doorBedroomLocked = Dlg("Dialogue_BedroomDoorLocked",
                ("", "Mom is still shouting. Better wake up first."));
            D.doorKarunaLocked = Dlg("Dialogue_KarunaDoorLocked",
                ("", "The door is shut. You should head outside first."));
            D.doorTUGardenLocked = Dlg("Dialogue_TUDoorLocked",
                ("", "The path northwest leads to the TU garden."));

            D.parentsDoorLocked = Dlg("Dialogue_ParentsDoorLocked",
                ("Lele", "I shouldn't open this right now. Dad is meditating."));

            D.momIdleAfterLunch = Dlg("Dialogue_Lele_MomIdleAfterLunch",
                ("Mom", "Why are you still here? Karuna's been waiting all morning."),
                ("Mom", "Go on. The girl will come back and knock if you don't."),
                ("Mom", "And remember. Home before dark."));

            D.sisterIdle = Dlg("Dialogue_SisterIdle",
                ("Sister", "<i>Dai</i>, do you know any songs? I know seven."),
                ("Sister", "I forgot four of them. The other three are extremely good though."),
                ("Sister", "One of them is just the word 'momo' said different ways. It's an art piece."),
                ("Sister", "Don't tell Mama. She doesn't understand my creative process."));

            AssetDatabase.SaveAssets();
        }

        // ── Scene 01: bedroom wakeup ─────────────────────────────────────────
        // Layout: 9-wide x 7-tall room. Lele asleep on the bed at the right
        // wall, downstairs door centered on the east wall. Mom's shouting fires
        // automatically on scene Start, before Lele moves.
        static void Build_01_Bedroom()
        {
            var scene = NewScene("01_LeleBedroom_Wakeup");
            BasicCamera(0.10f, 0.09f, 0.16f);
            MakeUIRoot();
            MakeStoryRoot();

            // Lele spawns on the bed (left side, on the head tile).
            MakePlayer(new Vector3(-3, 1, 0));

            // Perimeter is solid — the downstairs doorway sits inside the room,
            // not in the wall line.
            BuildPerimeter(4, 3);

            // Bed (left side of the room) — 1×2 vertical: head at (-3,1),
            // base at (-3,0). Lele stands on the head tile; both are non-blocking
            // so movement off the bed is free.
            SpawnBedTile(new Vector3(-3, 0, 0),
                         SpriteFactory.BedBase(new Color(0.85f, 0.78f, 0.70f)));
            SpawnBedTile(new Vector3(-3, 1, 0),
                         SpriteFactory.BedHead(new Color(0.85f, 0.78f, 0.70f),
                                               new Color(0.95f, 0.92f, 0.85f)));

            // Desk on the right side (single tile, leaves room above for the doorway)
            Wall(new Vector2(3, 1));
            // Small chest of drawers below the desk
            Decor(new Vector3(3, -1, 0), new Color(0.50f, 0.40f, 0.25f), 2);

            // Mom shouts on Start, before player moves. Suppressed if the
            // player has already woken (e.g., walked back upstairs from the
            // kitchen).
            MakeStartTrigger("Trigger_MomShouts_OnStart", OpeningBeat.MomShouts,
                             forbiddenFlag: "woke");

            // Downstairs doorway — top-right corner of the room, one tile inside
            // both walls. Locked until "woke" flag set by the shout cutscene.
            // Player returning from kitchen spawns at (2, 2), one tile west of
            // the doorway, so they don't immediately re-trigger it.
            MakeTransition(new Vector3(3, 2, 0),
                "02_LeleHouse_Kitchen", new Vector2(0, 3),
                requiredFlag: "woke",
                lockedHint: "mom is still shouting. better answer her first.");

            SaveAndRegister(scene, "01_LeleBedroom_Wakeup");
        }

        // Decor-style sprite tile with no collider — used for the bed.
        static void SpawnBedTile(Vector3 pos, Sprite sprite)
        {
            var go = new GameObject($"Bed_{pos.x}_{pos.y}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;  // below player (player is sortingOrder 10)
        }

        // Spawns an invisible GameObject that fires an OpeningBeat on Start().
        // No trigger collider — runs once per scene load. Pass a forbiddenFlag
        // so the cutscene doesn't replay if the player returns to this scene.
        static void MakeStartTrigger(string name, OpeningBeat beat, string forbiddenFlag = null)
        {
            var go = new GameObject(name);
            var inv = go.AddComponent<OpeningInvoker>();
            inv.beat = beat;
            inv.fireOnStart = true;
            inv.fireOnStartForbiddenFlag = forbiddenFlag;
        }

        // ── Scene 02: kitchen ────────────────────────────────────────────────
        static void Build_02_Kitchen()
        {
            var scene = NewScene("02_LeleHouse_Kitchen");
            BasicCamera(0.08f, 0.07f, 0.14f);
            MakeUIRoot();
            MakeStoryRoot();
            MakePlayer(new Vector3(0, 3, 0));

            // East wall has a gap at y=0 where the parents' bedroom door sits.
            BuildPerimeter(6, 4, southGapX: 0, eastGapY: 0);

            // Stove block (NW corner)
            Wall(new Vector2(-5, 3)); Wall(new Vector2(-4, 3)); Wall(new Vector2(-5, 2));

            // Mom NPC. On first entry she walks up to Lele and the kitchen
            // cutscene fires automatically. After "ateLunch" she stays put and
            // the same NPC works as a normal E-press interactable (idle line).
            var mom = MakeBeatNPC("Mom", new Vector3(-3, 2, 0),
                                  new Color(0.85f, 0.55f, 0.45f),
                                  OpeningBeat.MomKitchen, completionFlag: "ateLunch");
            // After the kitchen cutscene, E-pressing Mom replays a "go find
            // Karuna" reminder rather than going inert.
            mom.GetComponent<Interactable>().afterDialogue = D.momIdleAfterLunch;
            var approach = mom.AddComponent<NPCApproachOnSpawn>();
            // Right-angle path: walk east first to (-1, 2), then north to
            // (-1, 3) — one tile west of Lele's spawn at (0, 3). No diagonal.
            approach.waypoints = new Vector2[] {
                new Vector2(-1, 2),
                new Vector2(-1, 3),
            };
            approach.speed = 2f;
            approach.forbiddenFlag = "ateLunch";

            // Sister NPC — starts east of the dining table, idle. The kitchen
            // cutscene walks her over to Lele when her first speaking turn
            // arrives, then later all three walk back over to the table.
            MakeNPC("Sister", new Vector3(1, -1, 0),
                    new Color(0.95f, 0.70f, 0.55f), D.sisterIdle);

            // Dining table on the LEFT — a 2-tile rectangle with one chair on
            // each of the four sides. Lele takes the north chair, Mom the
            // west, Sister the east; the south chair stays empty (Dad's seat
            // — he's in his sadhana). Plates sit on the table tiles.
            var tableWood = new Color(0.55f, 0.40f, 0.25f);
            var chairWood = new Color(0.50f, 0.36f, 0.22f);
            // Table — 2 tiles, blocking
            Decor(new Vector3(-3, -1, 0), tableWood, 2);
            Decor(new Vector3(-2, -1, 0), tableWood, 2);
            // Chairs — non-blocking so a person can stand "on" the chair
            SpawnTile(null, "Chair_N", -3,  0, SpriteFactory.Chair(chairWood), sortingOrder: 4, hasCollider: false);
            SpawnTile(null, "Chair_S", -3, -2, SpriteFactory.Chair(chairWood), sortingOrder: 4, hasCollider: false);
            SpawnTile(null, "Chair_E", -1, -1, SpriteFactory.Chair(chairWood), sortingOrder: 4, hasCollider: false);
            SpawnTile(null, "Chair_W", -4, -1, SpriteFactory.Chair(chairWood), sortingOrder: 4, hasCollider: false);
            // Plates — on the table tiles, two food colors
            var plateRim = new Color(0.95f, 0.92f, 0.85f);
            SpawnTile(null, "Plate_L", -3, -1,
                      SpriteFactory.Plate(plateRim, new Color(0.95f, 0.78f, 0.30f)),
                      sortingOrder: 6, hasCollider: false);
            SpawnTile(null, "Plate_R", -2, -1,
                      SpriteFactory.Plate(plateRim, new Color(0.45f, 0.65f, 0.30f)),
                      sortingOrder: 6, hasCollider: false);

            // Parents' bedroom door — set into the east wall at (6, 0) where
            // the perimeter has its gap. Solid collider still blocks Lele;
            // pressing E plays the "I shouldn't open this right now" dialogue.
            MakeBlockingDoorWithDialogue("Door_ParentsBedroom",
                                         new Vector3(6, 0, 0), D.parentsDoorLocked);

            // Stairs back upstairs — same tile the player spawns on. Walking
            // away and back onto it loads the bedroom. (CheckTriggers only
            // fires on completed steps, so the spawn itself doesn't activate it.)
            MakeTransition(new Vector3(0, 3, 0),
                "01_LeleBedroom_Wakeup", new Vector2(2, 2),
                requiredFlag: null, lockedHint: "");

            // South door to town — gated by "ateLunch"
            MakeTransition(new Vector3(0, -4, 0),
                "03_Town_Day", new Vector2(0, 5),
                requiredFlag: "ateLunch",
                lockedHint: "you should eat first. mom made lunch.");

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
            assets.momKitchenIntro  = D.momKitchenIntro;
            assets.momKitchenAfter  = D.momKitchenAfter;
            assets.sisterMusicStar  = D.sisterMusicStar;
            assets.momLunchInvite   = D.momLunchInvite;
            assets.momLunchEat      = D.momLunchEat;
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

        static GameObject MakeNPC(string name, Vector3 pos, Color color, DialogueSO dialogue)
        {
            var go = new GameObject($"NPC_{name}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(color);
            sr.sortingOrder = 6;
            // Solid collider (non-trigger) so the player can't walk through them.
            // OverlapBoxAll in PlayerController.TryInteract still finds it for
            // the E-press dialogue.
            var col = go.AddComponent<BoxCollider2D>(); col.size = Vector2.one;
            var npc = go.AddComponent<NPC>();
            npc.npcName = name;
            npc.dialogue = dialogue;
            return go;
        }

        // NPC that runs an OpeningBeat instead of a plain dialogue. The cutscene
        // itself plays the dialogue. Optional completionFlag prevents re-firing.
        static GameObject MakeBeatNPC(string name, Vector3 pos, Color color,
                                      OpeningBeat beat, string completionFlag)
        {
            var go = new GameObject($"NPC_{name}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.NPC(color);
            sr.sortingOrder = 6;
            // Solid collider (non-trigger) so the player can't walk through.
            var col = go.AddComponent<BoxCollider2D>(); col.size = Vector2.one;
            var inter = go.AddComponent<Interactable>();
            inter.completionFlag = completionFlag;
            inter.forbiddenFlag = completionFlag;
            var inv = go.AddComponent<OpeningInvoker>();
            inv.beat = beat;
            return go;
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

        // A door tile that blocks movement (solid collider) and plays a
        // dialogue when interacted with via E. Used for the parents' bedroom
        // door in the kitchen — Lele can walk up to it but not through, and
        // pressing E reminds him not to disturb Dad's sadhana.
        static void MakeBlockingDoorWithDialogue(string name, Vector3 pos, DialogueSO dialogue)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Door(new Color(0.50f, 0.30f, 0.18f));
            sr.sortingOrder = 5;
            // Non-trigger collider blocks PlayerController.TryStep.
            go.AddComponent<BoxCollider2D>();
            var npc = go.AddComponent<NPC>();
            npc.npcName = "Door";
            npc.dialogue = dialogue;
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
