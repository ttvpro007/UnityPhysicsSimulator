using System.Reflection;
using Obvious.Soap.Editor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Attributes
{
    // [CustomPropertyDrawer(typeof(SubAssetAttribute))]
    // public class SubAssetPropertyDrawer : PropertyDrawer
    // {
    //     private const BindingFlags FieldBindingFlags =
    //         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    //
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         var attribute = (SubAssetAttribute)this.attribute;
    //
    //
    //         if (IsValidSubAssetProperty(property))
    //         {
    //             
    //             //draw default
    //             EditorGUILayout.PropertyField(property, label);
    //             //GUI.enabled = false;
    //             //EditorGUILayout.PropertyField(property);
    //             //GUI.enabled = true;
    //         }
    //         else
    //         {
    //             GUILayout.BeginVertical();
    //             EditorGUILayout.HelpBox("[SubAsset] attribute can only be used on ScriptableObjects.",
    //                 MessageType.Error,false);
    //             GUI.enabled = false;
    //             EditorGUILayout.PropertyField(property);
    //             GUI.enabled = true;
    //             GUILayout.EndVertical();
    //         }
    //     }
    //
    //     private static FieldInfo GetFieldInfoForProperty(SerializedProperty property)
    //     {
    //         var objectType = property.serializedObject.targetObject.GetType();
    //         var fieldInfo = objectType.GetField(property.name, FieldBindingFlags);
    //         return fieldInfo;
    //     }
    //
    //     internal static bool IsValidSubAssetProperty(SerializedProperty property)
    //     {
    //         return property.propertyType == SerializedPropertyType.ObjectReference &&
    //                IsValidSubAssetProperty(GetFieldInfoForProperty(property));
    //     }
    //
    //     internal static bool IsValidSubAssetProperty(FieldInfo field)
    //     {
    //         return field != null && field.FieldType.IsSubclassOf(typeof(ScriptableObject));
    //     }
    // }
}