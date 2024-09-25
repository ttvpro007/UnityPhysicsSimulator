﻿using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if !ODIN_INSPECTOR
namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableEventBase), true)]
    public class ScriptableEventGenericDrawer : UnityEditor.Editor
    {
        private MethodInfo _methodInfo;
        private ScriptableEventBase _scriptableEventBase;

        private void OnEnable()
        {
            _methodInfo = target.GetType().BaseType.GetMethod("Raise",
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (_scriptableEventBase == null)
                _scriptableEventBase = target as ScriptableEventBase;
            var genericType = _scriptableEventBase.GetGenericType;

            var canBeSerialized = SoapTypeUtils.IsUnityType(genericType) || SoapTypeUtils.IsSerializable(genericType);
            if (!canBeSerialized)
            {
                SoapEditorUtils.DrawSerializationError(genericType);
                return;
            }

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                var property = serializedObject.FindProperty("_debugValue");
                _methodInfo.Invoke(target, new[] { GetDebugValue(property) });
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

        private object GetDebugValue(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField("_debugValue", BindingFlags.Instance | BindingFlags.NonPublic);
            return targetField.GetValue(property.serializedObject.targetObject);
        }
    }
}
#endif