#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bandhana.EditorTools
{
    // Auto-runs the opening-scene builder after every script reload so that
    // edits to dialogue, layout, and behaviour show up the next time you press
    // Play, without having to invoke "Bandhana → Build Lele Opening" by hand.
    //
    //  • Skipped while compiling, in play mode, or if the active scene has
    //    unsaved changes (so manual edits aren't clobbered).
    //  • Skipped on the first reload after opening Unity (so just opening the
    //    project doesn't trigger a write).
    //  • Reloads the active scene after rebuild if it's one of the opening
    //    scenes, so the editor view picks up the new layout immediately.
    //  • Toggle on/off via Bandhana → Auto-Rebuild Opening.
    static class AutoBuildOpeningOnReload
    {
        const string PrefKey = "Bandhana.AutoBuildOpening";
        const string SessionArmedKey = "Bandhana.AutoBuildArmed";
        const string MenuPath = "Bandhana/Auto-Rebuild Opening";

        [DidReloadScripts]
        static void OnReload()
        {
            if (!EditorPrefs.GetBool(PrefKey, true)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            // Skip the first reload of the editor session — that one fires just
            // from opening the project and we don't want a write on launch.
            if (!SessionState.GetBool(SessionArmedKey, false))
            {
                SessionState.SetBool(SessionArmedKey, true);
                return;
            }

            var active = EditorSceneManager.GetActiveScene();
            if (active.IsValid() && active.isDirty)
            {
                Debug.Log("[Bandhana] Auto-rebuild skipped — active scene has unsaved changes.");
                return;
            }

            string activePath = active.IsValid() ? active.path : null;
            try
            {
                BuildOpeningStoryScenes.BuildAllSilent();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bandhana] Auto-rebuild failed: {e}");
                return;
            }

            if (!string.IsNullOrEmpty(activePath) && activePath.Contains("/Story/"))
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
        }

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            bool now = !EditorPrefs.GetBool(PrefKey, true);
            EditorPrefs.SetBool(PrefKey, now);
            Debug.Log($"[Bandhana] Auto-rebuild opening scenes: {(now ? "ON" : "OFF")}");
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, true));
            return true;
        }
    }
}
#endif
