using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace Obvious.Soap.Editor
{
    public class SoapWizardWindow : EditorWindow
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private Dictionary<ScriptableBase, Object> _subAssetsLookup;
        private ScriptableType _currentType = ScriptableType.All;
        private float _tabWidth => position.width / 6;
        private Texture[] _icons;
        private string _searchText = "";
        private UnityEditor.Editor _editor;

        [SerializeField] private string _currentFolderPath = "Assets";
        [SerializeField] private int _selectedScriptableIndex;
        [SerializeField] private int _typeTabIndex = -1;
        [SerializeField] private int _tagMask;
        [SerializeField] private bool _isInitialized;
        [SerializeField] private ScriptableBase _scriptableBase;
        [SerializeField] private ScriptableBase _previousScriptableBase;
        [SerializeField] private FavoriteData _favoriteData;

        private List<ScriptableBase> Favorites => _favoriteData.Favorites;
        internal const string PathKey = "SoapWizard_Path";
        internal const string FavoriteKey = "SoapWizard_Favorites";
        internal const string TagsKey = "SoapWizard_Tags";
        private SoapSettings _soapSettings;
        private readonly float _widthRatio = 0.6f;

        private Dictionary<ScriptableBase, SerializedObject> _serializedObjects =
            new Dictionary<ScriptableBase, SerializedObject>();

        [Serializable]
        private class FavoriteData
        {
            public List<ScriptableBase> Favorites = new List<ScriptableBase>();
        }

        public static bool IsPopupOpen = false;

        private enum ScriptableType
        {
            All,
            Variable,
            Event,
            List,
            Enum,
            Favorite
        }

        [MenuItem("Window/Obvious Game/Soap/Soap Wizard")]
        public new static void Show()
        {
            var window = GetWindow<SoapWizardWindow>(typeof(SceneView));
            window.titleContent = new GUIContent("Soap Wizard", Resources.Load<Texture>("Icons/icon_soapLogo"));
        }

        [MenuItem("Tools/Obvious Game/Soap/Soap Wizard %#w")]
        private static void OpenSoapWizard() => Show();

        private void OnEnable()
        {
            _soapSettings = SoapEditorUtils.GetOrCreateSoapSettings();
            this.wantsMouseMove = true;
            LoadIcons();
            LoadSavedData();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (_isInitialized)
            {
                SelectTab(_typeTabIndex);
                return;
            }

            SelectTab((int)_currentType, true); //default is 0
            _isInitialized = true;
        }

        private void OnDisable()
        {
            var favoriteData = JsonUtility.ToJson(_favoriteData, false);
            EditorPrefs.SetString(FavoriteKey, favoriteData);
            EditorPrefs.SetInt(TagsKey, _tagMask);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            DestroyImmediate(_editor);
        }

        private void LoadIcons()
        {
            _icons = new Texture[12];
            _icons[0] = EditorGUIUtility.IconContent("Favorite On Icon").image;
            _icons[1] = Resources.Load<Texture>("Icons/icon_scriptableVariable");
            _icons[2] = Resources.Load<Texture>("Icons/icon_scriptableEvent");
            _icons[3] = Resources.Load<Texture>("Icons/icon_scriptableList");
            _icons[4] = Resources.Load<Texture>("Icons/icon_scriptableEnum");
            _icons[5] = Resources.Load<Texture>("Icons/icon_scriptableSave");
            _icons[6] = Resources.Load<Texture>("Icons/icon_edit");
            _icons[7] = Resources.Load<Texture>("Icons/icon_duplicate");
            _icons[8] = Resources.Load<Texture>("Icons/icon_delete");
            _icons[9] = EditorGUIUtility.IconContent("Favorite Icon").image;
            _icons[10] = EditorGUIUtility.IconContent("Folder Icon").image;
            _icons[11] = Resources.Load<Texture>("Icons/icon_ping");
        }

        private void LoadSavedData()
        {
            _currentFolderPath = EditorPrefs.GetString(PathKey, "Assets");
            _favoriteData = new FavoriteData();
            var favoriteDataJson = JsonUtility.ToJson(_favoriteData, false);
            var favoriteData = EditorPrefs.GetString(FavoriteKey, favoriteDataJson);
            JsonUtility.FromJsonOverwrite(favoriteData, _favoriteData);
            _tagMask = EditorPrefs.GetInt(TagsKey, 1);
        }

        private void OnGUI()
        {
            if (_soapSettings == null)
                return;


            var padding = 2f;
            var paddedArea = new Rect(padding, padding, position.width - (padding * 2),
                position.height - (padding * 2));

            GUILayout.BeginArea(paddedArea);
            DrawFolder();
            GUILayout.Space(1);
            SoapInspectorUtils.DrawLine();
            GUILayout.Space(1);
            DrawTags();
            GUILayout.Space(2);
            DrawSearchBar();
            SoapInspectorUtils.DrawColoredLine(1, Color.black.Lighten(0.137f));
            DrawTabs();
            DrawScriptableBases(_scriptableObjects);
            DrawBottomButton();
            GUILayout.EndArea();

            if (Event.current.type == EventType.MouseMove && !IsPopupOpen)
            {
                Repaint();
            }
        }

        private void DrawFolder()
        {
            EditorGUILayout.BeginHorizontal();
            var buttonContent = new GUIContent(_icons[10], "Change Selected Folder");
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(0, 2, 0, 0);
            buttonStyle.padding = new RectOffset(4, 4, 4, 4);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxWidth(25f), GUILayout.Height(20f)))
            {
                var path = EditorUtility.OpenFolderPanel("Select folder to set path.", _currentFolderPath, "");

                //remove Application.dataPath from path & replace \ with / for cross-platform compatibility
                path = path.Replace(Application.dataPath, "Assets").Replace("\\", "/");

                if (!AssetDatabase.IsValidFolder(path))
                    EditorUtility.DisplayDialog("Error: File Path Invalid",
                        "Make sure the path is a valid folder in the project.", "Ok");
                else
                {
                    _currentFolderPath = path;
                    EditorPrefs.SetString(PathKey, _currentFolderPath);
                    OnTabSelected(_currentType, true);
                }
            }

            var displayedPath = $"{_currentFolderPath}/";
            EditorGUILayout.LabelField(displayedPath);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTags()
        {
            var height = EditorGUIUtility.singleLineHeight;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(height));
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(2, 2, 0, 0);
            buttonStyle.padding = new RectOffset(4, 4, 4, 4);
            var buttonContent = new GUIContent(_icons[6], "Edit Tags");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxWidth(25), GUILayout.MaxHeight(20)))
                PopupWindow.Show(new Rect(), new TagPopUpWindow(position));
            EditorGUILayout.LabelField("Tags", GUILayout.MaxWidth(70));
            var tags = _soapSettings.Tags.ToArray();
            _tagMask = EditorGUILayout.MaskField(_tagMask, tags);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();

            var tabNames = Enum.GetNames(typeof(ScriptableType));

            var defaultStyle = SoapInspectorUtils.Styles.ToolbarButton;
            var selectedStyle = new GUIStyle(defaultStyle);
            selectedStyle.normal.textColor = Color.white;

            for (int i = 0; i < tabNames.Length; i++)
            {
                var isSelected = i == _typeTabIndex;

                var style = isSelected ? selectedStyle : defaultStyle;

                if (GUILayout.Button(tabNames[i], style, GUILayout.Width(_tabWidth)))
                {
                    _typeTabIndex = i;
                    OnTabSelected((ScriptableType)_typeTabIndex, true);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Draw the bottom line
            var lastRect = GUILayoutUtility.GetLastRect();
            var width = lastRect.width / tabNames.Length;
            var x = lastRect.x + _typeTabIndex * width;
            EditorGUI.DrawRect(new Rect(x, lastRect.yMax - 2, width, 2), Color.white);
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            _searchText = GUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("", GUI.skin.FindStyle("SearchCancelButton")))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawScriptableBases(List<ScriptableBase> scriptables)
        {
            if (scriptables is null)
                return;

            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            //Cache styles
            var entryStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white }
            };
            var iconStyle = new GUIStyle(GUIStyle.none);
            var guiContent = new GUIContent();

            EditorGUIUtility.hierarchyMode = true;

            // Use a list to store indices to be drawn
            List<int> indicesToDraw = new List<int>();

            // Reverse iteration to safely handle deletions
            var count = scriptables.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (scriptables[i] != null)
                    indicesToDraw.Add(i);
            }

            count = indicesToDraw.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                GUILayout.Space(2f);
                DrawScriptable(indicesToDraw[i]);
            }

            EditorGUIUtility.hierarchyMode = false;

            // Handle deselection with a single check
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var totalRect = GUILayoutUtility.GetLastRect();
                if (!totalRect.Contains(Event.current.mousePosition))
                {
                    Deselect();
                    Repaint();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            //The end

            void DrawScriptable(int i)
            {
                var scriptable = scriptables[i];
                if (scriptable == null)
                    return;

                //filter tags
                if ((_tagMask & (1 << scriptable.TagIndex)) == 0)
                    return;

                var entryName = GetNameFor(scriptable);
                //filter search
                if (entryName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    return;

                //var rect = GUILayoutUtility.GetRect(new GUIContent(entryName), entryStyle);
                var rect = EditorGUILayout.GetControlRect();
                var selected = _selectedScriptableIndex == i;

                //Draw Background
                var backgroundRect = new Rect(rect)
                {
                    height = rect.height + 2f,
                    width = rect.width * 1.2f,
                    x = rect.x - 10f
                };

                if (selected)
                    EditorGUI.DrawRect(backgroundRect, new Color(0.172f, 0.365f, 0.529f));
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(backgroundRect, new Color(0.3f, 0.3f, 0.3f));

                //Draw icon
                var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite
                    ? GetIconFor(scriptable)
                    : _icons[(int)_currentType];

                guiContent.image = icon;
                var iconRect = new Rect(rect) { width = 18f, height = 18f };
                GUI.Box(iconRect, guiContent, iconStyle);

                var entryNameRect = new Rect(rect)
                {
                    width = rect.width * _widthRatio,
                    x = rect.x + 18f
                };

                // Handle right-click here
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 &&
                    rect.Contains(Event.current.mousePosition))
                {
                    ShowContextMenu(scriptable);
                    Event.current.Use();
                }

                // Draw Label or button
                if (selected)
                {
                    GUI.Label(entryNameRect, entryName, entryStyle);
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    DrawEditor(scriptable);
                    EditorGUILayout.EndVertical();
                }
                else if (GUI.Button(entryNameRect, entryName, EditorStyles.label)) // Select
                {
                    Deselect();
                    _selectedScriptableIndex = i;
                    _scriptableBase = scriptable;
                }

                // Draw Shortcut
                var shortcutRect = new Rect(rect)
                {
                    x = rect.x + rect.width * _widthRatio,
                    height = EditorGUIUtility.singleLineHeight,
                    width = rect.width * (1 - _widthRatio)
                };
                DrawShortcut(shortcutRect, scriptable);
            }

            string GetNameFor(ScriptableBase scriptableBase)
            {
                if (_subAssetsLookup != null && _subAssetsLookup.TryGetValue(scriptableBase, out var mainAsset))
                {
                    var prefix = $"[{mainAsset.name}] ";
                    var subAssetName = prefix + scriptableBase.name;
                    return subAssetName;
                }

                return scriptableBase.name;
            }

            void ShowContextMenu(ScriptableBase scriptableBase)
            {
                var menu = new GenericMenu();
                var favoriteText = Favorites.Contains(scriptableBase) ? "Remove from favorite" : "Add to favorite";
                var favoriteIcon = Favorites.Contains(scriptableBase) ? "\u2730 " : "\u2605 ";
                menu.AddItem(new GUIContent(favoriteIcon + favoriteText), false, () =>
                {
                    if (!Favorites.Contains(scriptableBase))
                        Favorites.Add(scriptableBase);
                    else
                        Favorites.Remove(scriptableBase);
                });
                menu.AddItem(new GUIContent("\ud83c\udfaf Ping"), false, () =>
                {
                    Selection.activeObject = scriptableBase;
                    EditorGUIUtility.PingObject(scriptableBase);
                });
                menu.AddItem(new GUIContent("\u270f\ufe0f Rename"), false,
                    () => { PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, scriptableBase)); });

                if (AssetDatabase.IsMainAsset(scriptableBase))
                {
                    menu.AddItem(new GUIContent("\ud83d\udcc4 Duplicate"), false, () =>
                    {
                        SoapEditorUtils.CreateCopy(scriptableBase);
                        Refresh(_currentType);
                    });
                }

                menu.AddItem(new GUIContent("\ud83d\udd0d Find References"), false,
                    () => { ReferencesPopupWindow.ShowWindow(position, scriptableBase); });
                menu.AddItem(new GUIContent("\u274c Delete"), false, () =>
                {
                    var isDeleted = SoapEditorUtils.DeleteObjectWithConfirmation(scriptableBase);
                    if (isDeleted)
                    {
                        _scriptableBase = null;
                        OnTabSelected(_currentType, true);
                    }
                });
                menu.ShowAsContext();
            }
        }

        private void DrawShortcut(Rect rect, ScriptableBase scriptable)
        {
            if (!_serializedObjects.TryGetValue(scriptable, out var serializedObject))
            {
                serializedObject = new SerializedObject(scriptable);
                _serializedObjects[scriptable] = serializedObject;
            }

            if (serializedObject == null) //could be destroyed
                return;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (scriptable is ScriptableVariableBase scriptableVariableBase)
                DrawVariableValue(scriptableVariableBase);
            else if (scriptable is ScriptableEventNoParam scriptableEventNoParam)
                DrawEventNoParam(scriptableEventNoParam);
            else if (scriptable is ScriptableListBase scriptableListBase)
                DrawList(scriptableListBase);
            else if (scriptable is ScriptableEventBase scriptableEventBase)
                DrawEventWithParam(scriptableEventBase);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            void DrawVariableValue(ScriptableVariableBase variableBase)
            {
                var valuePropertyDrawer = new ScriptableVariablePropertyDrawer(serializedObject, variableBase);
                valuePropertyDrawer.DrawShortcut(rect);
            }

            void DrawEventNoParam(ScriptableEventNoParam scriptableEventNoParam)
            {
                var propertyDrawer = new ScriptableEventNoParamPropertyDrawer();
                propertyDrawer.DrawShortcut(rect, scriptableEventNoParam);
            }

            void DrawEventWithParam(ScriptableEventBase scriptableEventBase)
            {
                var propertyDrawer = new ScriptableEventPropertyDrawer(serializedObject, scriptableEventBase);
                propertyDrawer.DrawShortcut(rect);
            }

            void DrawList(ScriptableListBase scriptableListBase)
            {
                var propertyDrawer = new ScriptableListPropertyDrawer(serializedObject, scriptableListBase);
                propertyDrawer.DrawShortcut(rect);
            }
        }

        private void DrawEditor(ScriptableBase scriptableBase)
        {
            EditorGUILayout.BeginVertical();
            if (_editor == null)
                UnityEditor.Editor.CreateCachedEditor(scriptableBase, null, ref _editor);
            if (scriptableBase == null)
            {
                DestroyImmediate(_editor);
                return;
            }

            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.BeginHorizontal();
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(0, 0, 3, 3);
            var lessPaddingStyle = new GUIStyle(buttonStyle);
            lessPaddingStyle.padding = new RectOffset(0, 0, 1, 1);

            var buttonHeight = 20;

            var icon = Favorites.Contains(_scriptableBase) ? _icons[9] : _icons[0];
            var tooltip = Favorites.Contains(_scriptableBase) ? "Remove from favorite" : "Add to favorite";
            var buttonContent = new GUIContent("Favorite", icon, tooltip);
            if (GUILayout.Button(buttonContent, lessPaddingStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                if (Favorites.Contains(_scriptableBase))
                    Favorites.Remove(_scriptableBase);
                else
                    Favorites.Add(_scriptableBase);
            }

            buttonContent = new GUIContent("Ping", _icons[11], "Pings the asset in the project");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                Selection.activeObject = _scriptableBase;
                EditorGUIUtility.PingObject(_scriptableBase);
            }

            buttonContent = new GUIContent("Rename", _icons[6]);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
                PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, _scriptableBase));

            EditorGUI.BeginDisabledGroup(!AssetDatabase.IsMainAsset(_scriptableBase));
            buttonContent = new GUIContent("Duplicate", _icons[7], "Create Copy");
            if (GUILayout.Button(buttonContent, lessPaddingStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                SoapEditorUtils.CreateCopy(_scriptableBase);
                Refresh(_currentType);
            }

            EditorGUI.EndDisabledGroup();

            buttonContent = new GUIContent("Delete", _icons[8]);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                var isDeleted = SoapEditorUtils.DeleteObjectWithConfirmation(_scriptableBase);
                if (isDeleted)
                {
                    _scriptableBase = null;
                    OnTabSelected(_currentType, true);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawBottomButton()
        {
            if (GUILayout.Button("Create New Type", GUILayout.Height(25f)))
            {
                SoapTypeCreatorWindow.Show();
            }
        }

        private void OnTabSelected(ScriptableType type, bool deselectCurrent = false)
        {
            Refresh(type);
            _currentType = type;
            if (deselectCurrent)
            {
                Deselect();
            }
        }

        private void Deselect()
        {
            _scriptableBase = null;
            _selectedScriptableIndex = -1;
            GUIUtility.keyboardControl = 0; //remove focus
            DestroyImmediate(_editor);
        }

        private void Refresh(ScriptableType type)
        {
            switch (type)
            {
                case ScriptableType.All:
                    _scriptableObjects =
                        SoapEditorUtils.FindAll<ScriptableBase>(_currentFolderPath, out _subAssetsLookup);
                    break;
                case ScriptableType.Variable:
                    var variables =
                        SoapEditorUtils.FindAll<ScriptableVariableBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = variables.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Event:
                    var events = SoapEditorUtils.FindAll<ScriptableEventBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = events.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.List:
                    var lists = SoapEditorUtils.FindAll<ScriptableListBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = lists.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Enum:
                    var enums = SoapEditorUtils.FindAll<ScriptableEnumBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = enums.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Favorite:
                    _scriptableObjects = Favorites;
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            _typeTabIndex = index;
            OnTabSelected((ScriptableType)_typeTabIndex, deselect);
        }

        private Texture GetIconFor(ScriptableBase scriptableBase)
        {
            var iconIndex = 0;
            switch (scriptableBase)
            {
                case ScriptableVariableBase _:
                    iconIndex = 1;
                    break;
                case ScriptableEventBase _:
                    iconIndex = 2;
                    break;
                case ScriptableListBase _:
                    iconIndex = 3;
                    break;
                case ScriptableEnumBase _:
                    iconIndex = 4;
                    break;
                case ScriptableSaveBase _:
                    iconIndex = 5;
                    break;
            }

            return _icons[iconIndex];
        }

        #region Repaint

        private void OnPlayModeStateChanged(PlayModeStateChange pm)
        {
            if (pm == PlayModeStateChange.EnteredPlayMode)
            {
                foreach (var scriptableBase in _scriptableObjects)
                {
                    if (scriptableBase != null)
                        scriptableBase.RepaintRequest += OnRepaintRequested;
                }
            }
            else if (pm == PlayModeStateChange.EnteredEditMode)
            {
                foreach (var scriptableBase in _scriptableObjects)
                {
                    if (scriptableBase != null)
                        scriptableBase.RepaintRequest -= OnRepaintRequested;
                }
            }
        }

        private void OnRepaintRequested()
        {
            //Debug.Log("Repaint Wizard " + _scriptableBase.name);
            Repaint();
        }

        #endregion
    }
}