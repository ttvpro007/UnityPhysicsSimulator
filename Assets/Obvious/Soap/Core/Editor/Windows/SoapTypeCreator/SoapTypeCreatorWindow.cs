using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class SoapTypeCreatorWindow : EditorWindow
    {
        private string _typeText = "NewClass";
        private string _namespaceText = "";
        private bool _baseClass;
        private bool _monoBehaviour;
        private bool _variable;
        private bool _event;
        private bool _eventListener;
        private bool _list;
        private bool _enum;
        private bool _save;
        private bool _isTypeNameInvalid;
        private bool _isNamespaceInvalid;
        private string _path;
        private Texture[] _icons;
        private int _destinationFolderIndex;
        private readonly string[] _destinationFolderOptions = { "Selected in Project", "Custom" };
        private readonly Color _validTypeColor = new Color(0.32f, 0.96f, 0.8f);

        internal const string DestinationFolderIndexKey = "SoapWizard_DestinationFolderIndex";
        internal const string DestinationFolderPathKey = "SoapWizard_DestinationFolderPath";
        
        [MenuItem("Window/Obvious Game/Soap/Soap Type Creator" )]
        public new static void Show()
        {
            var window = GetWindow<SoapTypeCreatorWindow>(typeof(SceneView));
            window.titleContent = new GUIContent("Soap Type Creator", Resources.Load<Texture>("Icons/icon_soapLogo"));
        }
        
        [MenuItem("Tools/Obvious Game/Soap/Soap Type Creator %#t")]
        private static void OpenSoapTypeCreator() => Show();

        private void OnEnable()
        {
            _icons = new Texture[8];
            _icons[0] = EditorGUIUtility.IconContent("cs Script Icon").image;
            _icons[1] = Resources.Load<Texture>("Icons/icon_scriptableVariable");
            _icons[2] = Resources.Load<Texture>("Icons/icon_scriptableEvent");
            _icons[3] = Resources.Load<Texture>("Icons/icon_eventListener");
            _icons[4] = Resources.Load<Texture>("Icons/icon_scriptableList");
            _icons[5] = Resources.Load<Texture>("Icons/icon_scriptableEnum");
            _icons[6] = EditorGUIUtility.IconContent("Error").image;
            _icons[7] = Resources.Load<Texture>("Icons/icon_scriptableSave");
            _destinationFolderIndex = EditorPrefs.GetInt(DestinationFolderIndexKey, 0);
            _path = _destinationFolderIndex == 0
                ? SoapFileUtils.GetSelectedFolderPathInProjectWindow()
                : EditorPrefs.GetString(DestinationFolderPathKey, "Assets");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create new Type(s)", SoapInspectorUtils.Styles.Header);
            GUILayout.BeginVertical(SoapInspectorUtils.Styles.PopupContent);
            DrawTextFields();
            GUILayout.Space(15);
            DrawTypeToggles();
            GUILayout.FlexibleSpace();
            DrawPath();
            GUILayout.Space(5);
            DrawButtons();
            GUILayout.EndVertical();
        }
        
          private void DrawTextFields()
        {
            //Draw Namespace Text Field
            {
                EditorGUILayout.BeginHorizontal();
                Texture2D texture = new Texture2D(0, 0);
                var icon = _isNamespaceInvalid ? _icons[6] : texture;
                var style = new GUIStyle(GUIStyle.none);
                style.margin = new RectOffset(10, 0, 5, 0);
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
                var labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                EditorGUILayout.LabelField("Namespace:", labelStyle, GUILayout.Width(75));
                EditorGUI.BeginChangeCheck();
                var textStyle = new GUIStyle(GUI.skin.textField);
                textStyle.focused.textColor = _isNamespaceInvalid ? Color.red : Color.white;
                _namespaceText = EditorGUILayout.TextField(_namespaceText, textStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    _isNamespaceInvalid = !SoapTypeUtils.IsNamespaceValid(_namespaceText);
                }

                EditorGUILayout.EndHorizontal();
            }

            //Draw TypeName Text Field
            {
                EditorGUILayout.BeginHorizontal();
                Texture2D texture = new Texture2D(0, 0);
                var icon = _isTypeNameInvalid ? _icons[6] : texture;
                var style = new GUIStyle(GUIStyle.none);
                style.margin = new RectOffset(10, 0, 5, 0);
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
                EditorGUILayout.LabelField("Type Name:", GUILayout.Width(75));
                EditorGUI.BeginChangeCheck();
                var textStyle = new GUIStyle(GUI.skin.textField);
                textStyle.focused.textColor = _isTypeNameInvalid ? Color.red : Color.white;
                _typeText = EditorGUILayout.TextField(_typeText, textStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    _isTypeNameInvalid = !SoapTypeUtils.IsTypeNameValid(_typeText);
                }

                EditorGUILayout.EndHorizontal();
            }
        }


        private void DrawTypeToggles()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            var capitalizedType = $"{_typeText.CapitalizeFirstLetter()}";
            if (_enum)
            {
                GUI.enabled = false;
                _baseClass = false;
                _variable = false;
                _event = false;
                _eventListener = false;
                _list = false;
                DrawCoreToggles(false);
                _save = false;
                GUI.enabled = true;
            }
            else
            {
                DrawCoreToggles(true);
            }

            GUILayout.Space(5);
            GUI.enabled = true;
            DrawToggle(ref _enum, "ScriptableEnum", $"{capitalizedType}", 5);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCoreToggles(bool allowEnable)
        {
            var capitalizedType = $"{_typeText.CapitalizeFirstLetter()}";
            if (!SoapTypeUtils.IsIntrinsicType(_typeText))
            {
                DrawToggle(ref _baseClass, $"{capitalizedType}", "", 0, true, 140);
                if (_save)
                    _monoBehaviour = false;
                if (_baseClass)
                {
                    GUI.enabled = !_save;
                    _monoBehaviour = GUILayout.Toggle(_monoBehaviour, "MonoBehaviour?");
                    GUI.enabled = true;
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Soap Classes", EditorStyles.boldLabel);
            GUILayout.Space(5);
            DrawToggle(ref _variable, $"{capitalizedType}", "Variable", 1, true, 140);
            GUILayout.Space(5);
            DrawToggle(ref _event, "ScriptableEvent", $"{capitalizedType}", 2);
            GUILayout.Space(5);
            if (!_event)
                _eventListener = false;
            GUI.enabled = _event;
            DrawToggle(ref _eventListener, "EventListener", $"{capitalizedType}", 3);
            GUI.enabled = allowEnable; // has to be there to reenable the list toggle
            GUILayout.Space(5);
            DrawToggle(ref _list, "ScriptableList", $"{capitalizedType}", 4);
            GUILayout.Space(5);
            DrawToggle(ref _save, "ScriptableSave", $"{capitalizedType}", 7);
        }

        private void DrawToggle(ref bool toggleValue, string typeName, string second, int iconIndex,
            bool isFirstRed = false, int maxWidth = 200)
        {
            EditorGUILayout.BeginHorizontal();
            var icon = _icons[iconIndex];
            var style = new GUIStyle(GUIStyle.none);
            GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
            toggleValue = GUILayout.Toggle(toggleValue, "", GUILayout.Width(maxWidth));
            GUIStyle firstStyle = new GUIStyle(GUI.skin.label);
            firstStyle.padding.left = 15 - maxWidth;
            if (isFirstRed)
                firstStyle.normal.textColor = _isTypeNameInvalid ? SoapEditorUtils.SoapColor : _validTypeColor;
            GUILayout.Label(typeName, firstStyle);
            GUIStyle secondStyle = new GUIStyle(GUI.skin.label);
            secondStyle.padding.left = -6;
            secondStyle.clipping = TextClipping.Clip;
            if (!isFirstRed)
                secondStyle.normal.textColor = _isTypeNameInvalid ? SoapEditorUtils.SoapColor : _validTypeColor;
            GUILayout.Label(second, secondStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPath()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Destination Folder Path:", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            _destinationFolderIndex = GUILayout.SelectionGrid(_destinationFolderIndex, _destinationFolderOptions, 2);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt(DestinationFolderIndexKey, _destinationFolderIndex);

            if (_destinationFolderIndex == 0)
            {
                var guiStyle = new GUIStyle(EditorStyles.label);
                guiStyle.fontStyle = FontStyle.Italic;
                _path = SoapFileUtils.GetSelectedFolderPathInProjectWindow();
                EditorGUILayout.LabelField($"{_path}", guiStyle);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _path = EditorGUILayout.TextField(_path);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetString(DestinationFolderPathKey, _path);
            }
        }

        private void DrawButtons()
        {
            GUI.enabled = !_isTypeNameInvalid && !_isNamespaceInvalid && HasAnythingSelected();

            if (SoapInspectorUtils.DrawCallToActionButton("Create",SoapInspectorUtils.ButtonSize.Medium))
            {
                if (!SoapTypeUtils.IsTypeNameValid(_typeText))
                    return;

                var isIntrinsicType = SoapTypeUtils.IsIntrinsicType(_typeText);
                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);

                if (_baseClass && !isIntrinsicType)
                {
                    var templateName = _monoBehaviour ? "NewTypeMonoTemplate.cs" : "NewTypeTemplate.cs";
                    if (!SoapEditorUtils.CreateClassFromTemplate(templateName, _namespaceText, _typeText, _path,
                            out newFile))
                    {
                        CloseWindow();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_variable)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("ScriptableVariableTemplate.cs", _namespaceText,
                            _typeText, _path,
                            out newFile, isIntrinsicType, true))
                    {
                        CloseWindow();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_event)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("ScriptableEventTemplate.cs", _namespaceText,
                            _typeText, _path,
                            out newFile, isIntrinsicType, true))
                    {
                        CloseWindow();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_eventListener)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("EventListenerTemplate.cs", _namespaceText, _typeText,
                            _path,
                            out newFile, isIntrinsicType, true))
                    {
                        CloseWindow();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_list)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("ScriptableListTemplate.cs", _namespaceText, _typeText,
                            _path,
                            out newFile, isIntrinsicType, true))
                    {
                        CloseWindow();
                        return;
                    }
                }

                if (_enum && !isIntrinsicType)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("ScriptableEnumTemplate.cs", _namespaceText,
                            _typeText, _path,
                            out newFile, isSoapClass: true))
                    {
                        CloseWindow();
                        return;
                    }
                }
                
                if (_save && !isIntrinsicType)
                {
                    if (!SoapEditorUtils.CreateClassFromTemplate("ScriptableSaveTemplate.cs", _namespaceText,
                            _typeText, _path,
                            out newFile, isSoapClass: true))
                    {
                        CloseWindow();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Completed!", progress);
                EditorUtility.DisplayDialog("Success", $"{_typeText} was created!", "OK");
                CloseWindow(false);
                EditorGUIUtility.PingObject(newFile);
            }
            GUI.enabled = true;
        }

        private bool HasAnythingSelected()
        {
            return _variable || _event || _eventListener || _list || _enum || _save;
        }

        private void CloseWindow(bool hasError = true)
        {
            EditorUtility.ClearProgressBar();
            Close();
            if (hasError)
                EditorUtility.DisplayDialog("Error", $"Failed to create {_typeText}", "OK");
        }

       
    }
}