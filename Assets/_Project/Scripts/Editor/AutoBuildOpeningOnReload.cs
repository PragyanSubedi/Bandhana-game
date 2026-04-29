#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bandhana.EditorTools
{
    // Auto-runs the opening-scene builder + main-menu builder after every
    // script reload so edits show up the next time you press Play, without
    // having to invoke "Bandhana → Build Lele Opening" by hand.
    //
    // Behaviour:
    //  • Skipped while in play mode (would clobber state).
    //  • Skipped if the active scene has unsaved changes — the unsaved scene
    //    is auto-saved first if it's one of the regenerable scenes, otherwise
    //    we log and bail.
    //  • Reloads the active scene after rebuild if it's a regenerable scene
    //    so the editor view picks up the new layout.
    //  • Toggle on/off via Bandhana → Auto-Rebuild Opening (default ON).
    //  • Run-once-now via Bandhana → Force Rebuild Now.
    //
    // Every step is logged with the [Bandhana] prefix so you can verify in
    // the console that the hook is firing.
    static class AutoBuildOpeningOnReload
    {
        const string PrefKey = "Bandhana.AutoBuildOpening";
        const string MenuToggle = "Bandhana/Auto-Rebuild Opening";
        const string MenuForce  = "Bandhana/Force Rebuild Now";

        [DidReloadScripts]
        static void OnReload()
        {
            if (!EditorPrefs.GetBool(PrefKey, true))
            {
                Debug.Log("[Bandhana] Auto-rebuild disabled — toggle via 'Bandhana → Auto-Rebuild Opening'.");
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("[Bandhana] Auto-rebuild skipped — in play mode.");
                return;
            }
            // Defer the actual run by one editor tick so the reload fully
            // settles (scene serialization, asset import) before we touch
            // anything. Without this, EditorSceneManager calls can race.
            EditorApplication.delayCall += RunBuild;
        }

        static void RunBuild()
        {
            string activePath = null;
            var active = EditorSceneManager.GetActiveScene();
            if (active.IsValid()) activePath = active.path;

            // If the active scene has unsaved changes AND it's a scene we'd
            // rebuild, save it first so it isn't clobbered.
            if (active.IsValid() && active.isDirty)
            {
                if (IsRegenerableScene(activePath))
                {
                    Debug.Log($"[Bandhana] Saving dirty scene before auto-rebuild: {activePath}");
                    EditorSceneManager.SaveScene(active);
                }
                else
                {
                    Debug.Log($"[Bandhana] Auto-rebuild skipped — '{activePath}' has unsaved changes.");
                    return;
                }
            }

            try
            {
                Debug.Log("[Bandhana] Auto-rebuilding opening scenes + main menu…");
                BuildOpeningStoryScenes.BuildAllSilent();
                BuildMainMenuScene.BuildSilent();
                AssetDatabase.SaveAssets();
                Debug.Log("[Bandhana] Auto-rebuild complete.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bandhana] Auto-rebuild failed: {e}");
                return;
            }

            // Reload the active scene so the editor view picks up the new
            // layout. Without this, the user sees the previous build until
            // they manually re-open the scene.
            if (IsRegenerableScene(activePath))
            {
                Debug.Log($"[Bandhana] Reloading active scene: {activePath}");
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
            }
        }

        static bool IsRegenerableScene(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.Contains("/Story/") || path.EndsWith("/MainMenu.unity");
        }

        [MenuItem(MenuToggle)]
        static void Toggle()
        {
            bool now = !EditorPrefs.GetBool(PrefKey, true);
            EditorPrefs.SetBool(PrefKey, now);
            Debug.Log($"[Bandhana] Auto-rebuild opening scenes: {(now ? "ON" : "OFF")}");
        }

        [MenuItem(MenuToggle, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuToggle, EditorPrefs.GetBool(PrefKey, true));
            return true;
        }

        [MenuItem(MenuForce)]
        static void ForceRebuild()
        {
            Debug.Log("[Bandhana] Force-rebuild requested by menu.");
            RunBuild();
        }
    }
}
#endif
