using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !ODIN_INSPECTOR
namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableEventNoParam))]
    public class ScriptableEventNoParamDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                var eventNoParam = (ScriptableEventNoParam)target;
                eventNoParam.Raise();
            }
            GUI.enabled = true;

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            SoapInspectorUtils.DrawLine();

            var goContainer = (IDrawObjectsInInspector)target;
            var gameObjects = goContainer.GetAllObjects();
            if (gameObjects.Count > 0)
                DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> objects)
        {
            var title = $"Listeners : {objects.Count}";
            EditorGUILayout.LabelField(title);
            foreach (var obj in objects)
                EditorGUILayout.ObjectField(obj, typeof(Object), true);
        }
    }
}
#endif