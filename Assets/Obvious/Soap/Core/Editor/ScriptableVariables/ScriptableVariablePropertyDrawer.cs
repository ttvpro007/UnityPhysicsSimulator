namespace Obvious.Soap.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariablePropertyDrawer : ScriptableBasePropertyDrawer
    {
        private SerializedObject _serializedObject;
        private ScriptableVariableBase _scriptableVariable;
        private float? _propertyWidthRatio;

        protected override string GetFieldName()
        {
            //fieldInfo.Name does not work for VariableReferences so we have to make an edge case for that.
            var isAbstract = fieldInfo.DeclaringType?.IsAbstract == true;
            var fieldName = isAbstract ? fieldInfo.FieldType.Name : fieldInfo.Name;
            return fieldName;
        }

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
            if (_scriptableVariable == null)
                _scriptableVariable = _serializedObject.targetObject as ScriptableVariableBase;

            //can be destroyed when using sub assets
            if (targetObject == null)
                return;

            DrawShortcut(rect);
        }

        public void DrawShortcut(Rect rect)
        {
            var genericType = _scriptableVariable.GetGenericType;
            var canBeSerialized = SoapTypeUtils.IsUnityType(genericType) || SoapTypeUtils.IsSerializable(genericType);
            if (!canBeSerialized)
            {
                SoapEditorUtils.DrawSerializationError(genericType, rect);
                return;
            }

            var propertyName = Application.isPlaying ? "_runtimeValue" : "_value";
            var value = _serializedObject.FindProperty(propertyName);

            var isSceneObject = typeof(Component).IsAssignableFrom(genericType) ||
                                typeof(GameObject).IsAssignableFrom(genericType);

            if (isSceneObject)
            {
                var objectValue = EditorGUI.ObjectField(rect, value.objectReferenceValue, genericType, true);
                _serializedObject.targetObject.GetType().GetProperty("Value")?
                    .SetValue(_serializedObject.targetObject, objectValue);
            }
            else
            {
                EditorGUI.PropertyField(rect, value, GUIContent.none);
            }
        }

        public ScriptableVariablePropertyDrawer(SerializedObject serializedObject,
            ScriptableVariableBase scriptableVariableBase)
        {
            _serializedObject = serializedObject;
            _scriptableVariable = scriptableVariableBase;
            _propertyWidthRatio = null;
        }

        public ScriptableVariablePropertyDrawer()
        {
        }

        protected override float WidthRatio
        {
            get
            {
                if (_scriptableVariable == null)
                {
                    _propertyWidthRatio = null;
                    return 0.82f;
                }

                if (_propertyWidthRatio.HasValue)
                    return _propertyWidthRatio.Value;

                var genericType = _scriptableVariable.GetGenericType;
                if (genericType == typeof(Vector2))
                    _propertyWidthRatio = 0.72f;
                else if (genericType == typeof(Vector3))
                    _propertyWidthRatio = 0.62f;
                else
                    _propertyWidthRatio = 0.82f;
                return _propertyWidthRatio.Value;
            }
        }
    }
}