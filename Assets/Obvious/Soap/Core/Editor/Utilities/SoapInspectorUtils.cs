using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Obvious.Soap.Editor
{
    public static class SoapInspectorUtils
    {
        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes the specified fields by name.
        /// </summary>
        /// <param name="fieldsToSkip">An array of names that should be excluded.</param>
        /// <example>Example: new string[] { "m_Script" , "myInt" } will skip the default Script field and the Integer field myInt.
        /// </example>
        internal static void DrawInspectorExcept(this SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(prop.name.Contains))
                        continue;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                } while (prop.NextVisible(false));
            }
        }

        internal static void DrawCustomInspector(this SerializedObject serializedObject, HashSet<string> fieldsToSkip,
            System.Type genericType)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            var propertyCount = 0;
            var runtimeValueProperty = serializedObject.FindProperty("_runtimeValue");
            var savedProperty = serializedObject.FindProperty("_saved");
            var guidProperty = serializedObject.FindProperty("_guid");
            var saveGuidProperty = serializedObject.FindProperty("_saveGuid");

            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Contains(prop.name))
                        continue;

                    if (prop.name == "_value")
                    {
                        DrawValueField(prop.name, serializedObject.targetObject);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    }

                    propertyCount++;
                    //Draw save properties
                    if (propertyCount == 4 && savedProperty.boolValue)
                    {
                        DrawSaveProperties();
                    }
         
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
            
            void DrawValueField(string propertyName,  Object target)
            {
                if (Application.isPlaying)
                {
                    //Draw Object field
                    if (genericType != null)
                    {
                        var objectValue = EditorGUILayout.ObjectField("Runtime Value",
                            runtimeValueProperty.objectReferenceValue, genericType,
                            true);
                        target.GetType().GetProperty("Value").SetValue(target, objectValue);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(runtimeValueProperty);
                    }
                }
                else
                {
                    //Draw Object field
                    if (genericType != null)
                    {
                        var tooltip = "The value should only be set at runtime.";
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(new GUIContent("Value", tooltip), null, genericType, false);
                        GUI.enabled = true;
                            
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), true);
                    }
                }
            }
            
            void DrawSaveProperties()
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(saveGuidProperty, true);
                var saveGuidType = (SaveGuidType)saveGuidProperty.enumValueIndex;
                if (saveGuidType == SaveGuidType.Auto)
                {
                    GUI.enabled = false;
                    EditorGUILayout.TextField(guidProperty.stringValue);
                    GUI.enabled = true;
                }
                else
                {
                    guidProperty.stringValue = EditorGUILayout.TextField(guidProperty.stringValue);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        internal static void DrawOnlyField(this SerializedObject serializedObject, string fieldName,
            bool isReadOnly)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name != fieldName)
                        continue;

                    GUI.enabled = !isReadOnly;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    GUI.enabled = true;
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw all properties except the ones specified.
        /// Also disables the m_Script property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyToExclude"></param>
        internal static void DrawPropertiesExcluding(SerializedObject obj, params string[] propertyToExclude)
        {
            obj.Update();
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    GUI.enabled = iterator.name != "m_Script";
                    EditorGUILayout.PropertyField(iterator, true);
                    GUI.enabled = true;
                }
            }

            obj.ApplyModifiedProperties();
        }

        internal static void DrawLine(int height = 1) => DrawColoredLine(height, new Color(0.5f, 0.5f, 0.5f, 1));

        internal static void DrawColoredLine(int height, Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }

        internal static void DrawVerticalColoredLine(int width, Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(width), GUILayout.ExpandHeight(true));
            rect.width = width;
            EditorGUI.DrawRect(rect, color);
        }

        internal static void DrawSelectableObject(Object obj, string[] labels)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(labels[0], GUILayout.MaxWidth(300)))
                EditorGUIUtility.PingObject(obj);

            if (GUILayout.Button(labels[1], GUILayout.MaxWidth(75)))
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        internal static Texture2D CreateTexture(Color color)
        {
            var result = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }

        /// <summary> Centers a rect inside another window. </summary>
        /// <param name="window"></param>
        /// <param name="origin"></param>
        internal static Rect CenterInWindow(Rect window, Rect origin)
        {
            var pos = window;
            float w = (origin.width - pos.width) * 0.5f;
            float h = (origin.height - pos.height) * 0.5f;
            pos.x = origin.x + w;
            pos.y = origin.y + h;
            return pos;
        }

        internal static void DrawPopUpHeader(EditorWindow editorWindow, string titleName)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(Icons.Cancel, Styles.CancelButton))
                editorWindow.Close();

            EditorGUILayout.LabelField(titleName, Styles.Header);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        internal static bool DrawCallToActionButton(string text, ButtonSize size)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var style = new GUIStyle(Styles.CallToActionButton);
            switch (size)
            {
                case ButtonSize.Small:
                    style.fixedHeight = 20;
                    style.fixedWidth = 60;
                    break;
                case ButtonSize.Medium:
                    style.fixedHeight = 25;
                    style.fixedWidth = 75;
                    break;
                case ButtonSize.Large:
                    style.fixedHeight = 25;
                    style.fixedWidth = 150;
                    break;
            }

            Color originalColor = GUI.backgroundColor;
            var color = new Color(0.2f, 1.1f, 1.7f, 1);
            GUI.backgroundColor = color.Lighten(0.3f);
            var hasClicked = GUILayout.Button(text, style);
            GUI.backgroundColor = originalColor;
            GUILayout.EndHorizontal();
            return hasClicked;
        }

        internal enum ButtonSize
        {
            Small,
            Medium,
            Large
        }

        internal static class Icons
        {
            private static Texture _cancel;

            internal static Texture Cancel =>
                _cancel ? _cancel : _cancel = Resources.Load<Texture>("Icons/icon_cancel");

            private static Texture _subAsset;

            internal static Texture SubAsset =>
                _subAsset ? _subAsset : _subAsset = Resources.Load<Texture>("Icons/icon_subAsset");
        }

        internal static class Styles
        {
            private static GUIStyle _header;

            internal static GUIStyle Header
            {
                get
                {
                    if (_header == null)
                    {
                        _header = new GUIStyle(EditorStyles.boldLabel)
                        {
                            fontSize = 14,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleCenter,
                            fixedHeight = 25,
                            contentOffset = new Vector2(-5, 0)
                        };
                    }

                    return _header;
                }
            }

            private static GUIStyle _cancelButton;

            internal static GUIStyle CancelButton
            {
                get
                {
                    if (_cancelButton == null)
                    {
                        _cancelButton = new GUIStyle(GUIStyle.none)
                        {
                            padding = new RectOffset(4, 4, 4, 4),
                            margin = new RectOffset(4, 0, 4, 0),
                            fixedWidth = 20,
                            fixedHeight = 20
                        };
                    }

                    return _cancelButton;
                }
            }

            private static GUIStyle _popupContentStyle;

            internal static GUIStyle PopupContent
            {
                get
                {
                    if (_popupContentStyle == null)
                    {
                        _popupContentStyle = new GUIStyle(GUIStyle.none)
                        {
                            padding = new RectOffset(10, 10, 10, 10),
                        };
                    }

                    return _popupContentStyle;
                }
            }

            private static GUIStyle _callToActionButton;

            internal static GUIStyle CallToActionButton
            {
                get
                {
                    if (_callToActionButton == null)
                    {
                        _callToActionButton = new GUIStyle(GUI.skin.button);
                    }

                    return _callToActionButton;
                }
            }

            private static GUIStyle _toolbarButton;

            internal static GUIStyle ToolbarButton
            {
                get
                {
                    if (_toolbarButton == null)
                    {
                        _toolbarButton = new GUIStyle(EditorStyles.toolbar)
                        {
                            fontSize = 10,
                            contentOffset = new Vector2(0, 2),
                        };
                        _toolbarButton.normal.textColor = Color.gray.Lighten(0.1f);
                    }

                    return _toolbarButton;
                }
            }
        }
    }
}