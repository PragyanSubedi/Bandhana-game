using UnityEngine;

namespace Bandhana.UI
{
    // M0 verify script — attach to any GameObject in the SampleScene and hit Play.
    // Replace with a real title screen in M7.
    public class HelloBandhana : MonoBehaviour
    {
        GUIStyle title;
        GUIStyle subtitle;

        void OnGUI()
        {
            title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            subtitle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.85f, 0.85f, 0.9f) }
            };

            var w = Screen.width;
            var h = Screen.height;
            GUI.Label(new Rect(0, h * 0.40f, w, 80), "Hello, Bandhana", title);
            GUI.Label(new Rect(0, h * 0.50f, w, 40), "Eight Petals of the Mandala", subtitle);
            GUI.Label(new Rect(0, h * 0.60f, w, 30), "M0 — setup verified.", subtitle);
        }
    }
}
