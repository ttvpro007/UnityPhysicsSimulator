using UnityEngine;

namespace InfinityPBR
{
    public abstract class InfinitySoEditor<T> : InfinityEditorScriptableObject<T> where T : ScriptableObject
    {
        public static Color inactive = new Color(0.75f, .75f, 0.75f, 1f);
        public static Color active = new Color(0.6f, 1f, 0.6f, 1f);
        public static Color active2 = new Color(0.0f, 1f, 0.0f, 1f);
        public static Color dark = new Color(0.25f, 0.25f, 0.25f, 1f);
        public static Color mixed = Color.yellow;
        public static Color red = new Color(1f, 0.25f, 0.25f, 1f);
        public static Color blue = new Color(0.25f, 0.25f, 1f, 1f);
        
        
        
        public static bool ShowFullInspector(string scriptName)
        {
            Space();
            SetBool("Show full inspector " + scriptName,
                LeftCheck("Show full inspector", GetBool("Show full inspector " + scriptName)));
            return GetBool("Show full inspector " + scriptName);
        }
    }
}
