using System.Collections;
using UnityEngine;
using Bandhana.Core;

namespace Bandhana.Story
{
    // Concrete cutscene routines for the opening (Acts 1–9 of the new
    // storyline: bedroom wakeup → TU garden boss). Each public method is
    // wired up by the editor builder via Interactable / AutoCutsceneTrigger
    // events, dispatched through OpeningInvoker.
    public static class OpeningCutscenes
    {
        static StoryAssets Assets => Object.FindFirstObjectByType<StoryAssets>();

        // ── Beat 0: bedroom wakeup ───────────────────────────────────────────
        public static void MomShouts() { CutsceneRunner.Instance.Run(MomShoutsRoutine()); }
        static IEnumerator MomShoutsRoutine()
        {
            yield return CutsceneRunner.Wait(0.4f);
            yield return CutsceneRunner.Say(Assets?.momShouts);
            yield return CutsceneRunner.SetFlag("woke");
        }

        // ── Beat 1: kitchen — mom + sister + lunch ───────────────────────────
        // Sister walks over to Lele only when her speaking turn arrives, not
        // on scene start. Path: east first, then straight north.
        public static void MomKitchen() { CutsceneRunner.Instance.Run(KitchenRoutine()); }
        static IEnumerator KitchenRoutine()
        {
            var a = Assets;
            yield return CutsceneRunner.Say(a?.momKitchenIntro);
            yield return WalkSisterToLele();
            yield return CutsceneRunner.Say(a?.momKitchenAfter);
            yield return CutsceneRunner.Say(a?.sisterMusicStar);
            yield return CutsceneRunner.Say(a?.momLunchInvite);
            yield return WalkAllToTable();
            yield return CutsceneRunner.Say(a?.momLunchEat);
            yield return CutsceneRunner.SetFlag("ateLunch");
        }

        static IEnumerator WalkSisterToLele()
        {
            var sister = GameObject.Find("NPC_Sister");
            if (sister == null) yield break;
            yield return WalkPath(sister.transform,
                new Vector2[] { new Vector2(1, -1), new Vector2(1, 3) }, 3.5f);
        }

        // Mom, Lele, and Sister all walk over to the dining table on the left
        // and take their chairs (Lele = north, Mom = west, Sister = east —
        // the south chair stays empty for Dad). Coroutines run in parallel so
        // they move together; routes go around the table to avoid passing
        // through it.
        static IEnumerator WalkAllToTable()
        {
            var lele   = GameObject.Find("Player");
            var mom    = GameObject.Find("NPC_Mom");
            var sister = GameObject.Find("NPC_Sister");

            const float speed = 3.5f;
            var coros = new System.Collections.Generic.List<Coroutine>();
            if (lele != null)
                coros.Add(CutsceneRunner.Instance.StartCoroutine(WalkPath(lele.transform,
                    new Vector2[] { new Vector2(0, 0), new Vector2(-3, 0) }, speed)));
            if (mom != null)
                coros.Add(CutsceneRunner.Instance.StartCoroutine(WalkPath(mom.transform,
                    new Vector2[] { new Vector2(-1, 0), new Vector2(-4, 0), new Vector2(-4, -1) }, speed)));
            if (sister != null)
                coros.Add(CutsceneRunner.Instance.StartCoroutine(WalkPath(sister.transform,
                    new Vector2[] { new Vector2(1, -1), new Vector2(-1, -1) }, speed)));

            foreach (var c in coros) yield return c;
        }

        static IEnumerator WalkPath(Transform t, Vector2[] path, float speed)
        {
            foreach (var wp in path)
            {
                while ((Vector2)t.position != wp)
                {
                    t.position = Vector2.MoveTowards(t.position, wp, speed * Time.deltaTime);
                    yield return null;
                }
            }
        }

        // ── Beat 2: KarunaHouse — Karuna's dad ───────────────────────────────
        public static void KarunaDad() { CutsceneRunner.Instance.Run(KarunaDadRoutine()); }
        static IEnumerator KarunaDadRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.karunasDad);
            yield return CutsceneRunner.SetFlag("knowsKarunaAtTU");
        }

        // ── Beat 3: TU garden day — Karuna greets ────────────────────────────
        public static void KarunaTUDay() { CutsceneRunner.Instance.Run(KarunaTUDayRoutine()); }
        static IEnumerator KarunaTUDayRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.karunaTUGarden);
            yield return CutsceneRunner.SetFlag("metKarunaAtTU");
        }

        // ── Beat 4: play damaru → world flips → blackout → load TU night ────
        public static void PlayDamaruFirstTime() { CutsceneRunner.Instance.Run(PlayDamaruFirstRoutine()); }
        static IEnumerator PlayDamaruFirstRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.damaruPickup);
            for (int i = 0; i < 3; i++)
            {
                yield return ScreenFader.Instance.Fade(0f, 0.7f, 0.25f, new Color(0.55f, 0.30f, 0.85f));
                yield return ScreenFader.Instance.Fade(0.7f, 0.0f, 0.20f, new Color(0.55f, 0.30f, 0.85f));
                yield return CutsceneRunner.Wait(0.15f);
            }
            yield return ScreenFader.Instance.Fade(0f, 1f, 0.9f, Color.black);
            yield return CutsceneRunner.SetRealm(WorldRealm.Astral);
            yield return CutsceneRunner.SetFlag("damaruPlayedOnce");
            yield return CutsceneRunner.Wait(0.6f);

            SaveContext.SetPending(new Vector2(0, -3));
            UnityEngine.SceneManagement.SceneManager.LoadScene("06_TUGarden_Night");
            yield return null;
            yield return ScreenFader.Instance.Fade(1f, 0f, 1.2f, Color.black);
        }

        // ── Beat 5: TU night wakeup ─────────────────────────────────────────
        public static void TUNightWake() { CutsceneRunner.Instance.Run(TUNightWakeRoutine()); }
        static IEnumerator TUNightWakeRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.leleAlone);
            yield return CutsceneRunner.SetFlag("wokeAlone");
        }

        // ── Beat 6: empty street first impression ────────────────────────────
        public static void EmptyStreet() { CutsceneRunner.Instance.Run(EmptyStreetRoutine()); }
        static IEnumerator EmptyStreetRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.emptyStreet);
        }

        // ── Beat 7: Lele's bedroom astral — find Baje ────────────────────────
        public static void BajeAppears() { CutsceneRunner.Instance.Run(BajeAppearsRoutine()); }
        static IEnumerator BajeAppearsRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.bajeMeeting);
            yield return CutsceneRunner.Say(Assets?.bajeAstralLore);
            yield return CutsceneRunner.SetFlag("metBaje");
        }

        // ── Beat 8: outside, distorted figure beat ───────────────────────────
        public static void BajeOutside() { CutsceneRunner.Instance.Run(BajeOutsideRoutine()); }
        static IEnumerator BajeOutsideRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.bajeOutside);
        }

        // ── Beat 9: TU garden boss intro + start rhythm fight ────────────────
        public static void KarunaTwistIntro() { CutsceneRunner.Instance.Run(KarunaTwistRoutine()); }
        static IEnumerator KarunaTwistRoutine()
        {
            yield return CutsceneRunner.Say(Assets?.karunaTwistIntro);
            var boss = Object.FindFirstObjectByType<RhythmBossKaruna>();
            if (boss == null) { Debug.LogError("[Bandhana] No RhythmBossKaruna in scene."); yield break; }
            boss.BeginFight();
            while (boss.IsActive) yield return null;
            yield return CutsceneRunner.Say(Assets?.karunaPostBoss);
            yield return CutsceneRunner.SetFlag("bossInterrupted");
        }
    }
}
