using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !ODIN_INSPECTOR
namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableListBase), true)]
    public class ScriptableListDrawer : UnityEditor.Editor
    {
        private ScriptableBase _scriptableBase = null;
        private ScriptableListBase _scriptableListBase;
        private static bool _repaintFlag;

        public override void OnInspectorGUI()
        {
            if (_scriptableListBase == null)
                _scriptableListBase = target as ScriptableListBase;

            var isMonoBehaviourOrGameObject = _scriptableListBase.GetGenericType.IsSubclassOf(typeof(MonoBehaviour))
                                              || _scriptableListBase.GetGenericType == typeof(GameObject);
            if (isMonoBehaviourOrGameObject)
            {
                SoapInspectorUtils.DrawPropertiesExcluding(serializedObject, new[] { "_list" });
            }
            else
            {
                //we still want to display the native list for non MonoBehaviors (like SO for examples)
                DrawDefaultInspector();

                //Check for Serializable
                var genericType = _scriptableListBase.GetGenericType;
                var canBeSerialized =
                    SoapTypeUtils.IsUnityType(genericType) || SoapTypeUtils.IsSerializable(genericType);
                if (!canBeSerialized)
                {
                    SoapEditorUtils.DrawSerializationError(genericType);
                    return;
                }
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            var container = (IDrawObjectsInInspector)target;
            var gameObjects = container.GetAllObjects();

            SoapInspectorUtils.DrawLine();

            if (gameObjects.Count > 0)
                DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> objects)
        {
            var title = $"List Count : {objects.Count}";
            EditorGUILayout.LabelField(title);
            foreach (var obj in objects)
                EditorGUILayout.ObjectField(obj, typeof(Object), true);
        }

        #region Repaint

        private void OnEnable()
        {
            if (_repaintFlag)
                return;

            _scriptableBase = target as ScriptableBase;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _repaintFlag = true;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                if (_scriptableBase == null)
                    _scriptableBase = target as ScriptableBase;
                _scriptableBase.RepaintRequest += OnRepaintRequested;
            }
            else if (obj == PlayModeStateChange.EnteredEditMode)
                _scriptableBase.RepaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested() => Repaint();
    }

    #endregion
}
#endif