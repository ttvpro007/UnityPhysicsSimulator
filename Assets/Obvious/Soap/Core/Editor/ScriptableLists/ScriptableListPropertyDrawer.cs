using UnityEngine;
using UnityEditor;

namespace Obvious.Soap.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableListBase), true)]
    public class ScriptableListPropertyDrawer : ScriptableBasePropertyDrawer
    {
        private SerializedObject _serializedObject;
        private ScriptableListBase _scriptableListBase;

        protected override void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label,
            Object targetObject)
        {
            if (_serializedObject == null || _serializedObject.targetObject != targetObject)
                _serializedObject = new SerializedObject(targetObject);

            _serializedObject.UpdateIfRequiredOrScript();
            base.DrawUnExpanded(position, property, label, targetObject);
            if (_serializedObject.targetObject != null) //can be destroyed when using sub assets
                _serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawShortcut(Rect rect, SerializedProperty property, Object targetObject)
        {
            if (_scriptableListBase == null)
                _scriptableListBase = _serializedObject.targetObject as ScriptableListBase;
            
            //can be destroyed when using sub assets
            if (targetObject == null)
                return;
            
            DrawShortcut(rect);
        }
        
        public void DrawShortcut(Rect rect)
        {
            var genericType = _scriptableListBase.GetGenericType;
            var canBeSerialized = SoapTypeUtils.IsUnityType(genericType) || SoapTypeUtils.IsSerializable(genericType);
            if (!canBeSerialized)
            {
                SoapEditorUtils.DrawSerializationError(genericType, rect);
                return;
            }

            var value = _serializedObject.FindProperty("_list");
            var count = value.arraySize;
            EditorGUI.LabelField(rect, "Count: " + count);
        }
        
        public ScriptableListPropertyDrawer(SerializedObject serializedObject, ScriptableListBase scriptableListBase)
        {
            _serializedObject = serializedObject;
            _scriptableListBase = scriptableListBase;
        }
        
        public ScriptableListPropertyDrawer() { }
    }
}