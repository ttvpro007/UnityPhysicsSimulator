using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Obvious.Soap.Attributes;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Obvious.Soap.Editor
{
    public static class SoapEditorUtils
    {
        const string CustomCreatePathKey = "SoapWizard_CreateFolderPath";

        internal static string CustomCreatePath
        {
            get => EditorPrefs.GetString(CustomCreatePathKey, "Assets");
            set => EditorPrefs.SetString(CustomCreatePathKey, value);
        }

        internal static Color SoapColor =>
            ColorUtility.TryParseHtmlString(SoapColorHtml, out var color) ? color : Color.magenta;

        internal const string SoapColorHtml = "#f75369";

        internal static Color Lighten(this Color color, float amount)
        {
            return new Color(color.r + amount, color.g + amount, color.b + amount, color.a);
        }

        internal static SoapSettings GetOrCreateSoapSettings()
        {
            var settings = Resources.Load<SoapSettings>("SoapSettings");
            if (settings != null)
                return settings;

            var paths = SoapFileUtils.GetResourcesDirectories();
            foreach (var path in paths)
            {
                var relative = SoapFileUtils.GetRelativePath(path);
                if (!relative.Contains("Obvious") || !relative.Contains("Editor"))
                    continue;
                var forwardSlashPath = relative.Replace('\\', '/');
                var finalPath = forwardSlashPath + "/SoapSettings.asset";
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(SoapSettings)), finalPath);
                settings = Resources.Load<SoapSettings>("SoapSettings");
            }

            return settings;
        }

        /// <summary>
        /// Deletes an object after showing a confirmation dialog.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns> true if the deletion is confirmed</returns>
        public static bool DeleteObjectWithConfirmation(Object obj)
        {
            var confirmDelete = EditorUtility.DisplayDialog("Delete " + obj.name + "?",
                "Are you sure you want to delete '" + obj.name + "'?", "Yes", "No");
            if (confirmDelete)
            {
                if (AssetDatabase.IsSubAsset(obj))
                {
                    DeleteSubAsset(obj);
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    AssetDatabase.DeleteAsset(path);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a copy of an object. Also adds a number to the copy name.
        /// </summary>
        /// <param name="obj"></param>
        public static void CreateCopy(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var copyFilePath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CopyAsset(path, copyFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadMainAssetAtPath(copyFilePath);
            EditorGUIUtility.PingObject(newAsset);
        }

        /// <summary> Creates a Soap Scriptable object at a certain path </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        public static ScriptableObject CreateScriptableObjectAt(Type type, string name, string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var endPath = path.Replace("Assets/", "");
                AssetDatabase.CreateFolder("Assets", endPath);
            }

            var instance = ScriptableObject.CreateInstance(type);
            instance.name = name == "" ? type.ToString().Replace("Obvious.Soap.", "") : name;
            var creationPath = $"{path}/{instance.name}.asset";
            var uniqueFilePath = AssetDatabase.GenerateUniqueAssetPath(creationPath);
            var isAuto = GetOrCreateSoapSettings().NamingOnCreationMode == ENamingCreationMode.Auto;
            if (isAuto)
            {
                AssetDatabase.CreateAsset(instance, uniqueFilePath);
                EditorGUIUtility.PingObject(instance);
            }
            else
            {
                ProjectWindowUtil.CreateAsset(instance, uniqueFilePath);
            }

            return instance;
        }

        /// <summary> Renames an asset </summary>
        /// <param name="obj"></param
        /// <param name="newName"></param>
        public static void RenameAsset(Object obj, string newName)
        {
            if (AssetDatabase.IsSubAsset(obj))
            {
                obj.name = newName;
                EditorUtility.SetDirty(obj);
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.RenameAsset(path, newName);
            }

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(obj);
        }

        internal static bool CreateClassFromTemplate(string template, string nameSpace, string type, string path,
            out TextAsset newFile, bool isIntrinsic = false, bool isSoapClass = false)
        {
            var folderName = "Templates/";
            folderName +=
                isIntrinsic
                    ? "1_"
                    : string.Empty; //select another template for intrinsic types as they cannot work with nameof(T)
            folderName += template;
            var capitalizedName = type.CapitalizeFirstLetter();

            var fileName = capitalizedName + ".cs";
            if (isSoapClass)
            {
                //variables follow a different naming rules
                fileName = template.Contains("Variable")
                    ? $"{capitalizedName}Variable.cs"
                    : template.Replace("Template", capitalizedName);
            }

            newFile = CreateNewClass(folderName, nameSpace, type, fileName, path);
            return newFile != null;
        }

        private static TextAsset CreateNewClass(string templateName, string nameSpace, string type, string fileName,
            string path)
        {
            if (!SoapTypeUtils.IsTypeNameValid(type) || !SoapTypeUtils.IsNamespaceValid(nameSpace))
                return null;

            var template = Resources.Load<TextAsset>(templateName);
            if (template is null)
            {
                Debug.LogError($"Failed to find {templateName} in a Resources folder");
                return null;
            }

            var templateCode = template.text;
            templateCode = templateCode.Replace("#TYPE#", type);
            templateCode = templateCode.Replace("#TYPENAME#", type.CapitalizeFirstLetter());

            //wrap namespace if needed
            if (!string.IsNullOrEmpty(nameSpace))
            {
                var templateParts = SplitStringAfter(templateCode, "using Obvious.Soap;");
                var partToWrap = string.IsNullOrEmpty(templateParts.Item2)
                    ? templateParts.Item1
                    : templateParts.Item2;

                templateCode = partToWrap.WrapInNamespace(nameSpace);

                //if there was two parts, add the first part back at the top + spaces
                if (!string.IsNullOrEmpty(templateParts.Item2))
                {
                    var partToInsert = templateParts.Item1 + Environment.NewLine + Environment.NewLine;
                    templateCode = templateCode.Insert(0, partToInsert);
                }
            }

            try
            {
                var newFile = CreateTextFile(templateCode, fileName, path);
                return newFile;
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Could not create class", e.Message, "OK");
                return null;
            }
        }

        private static string WrapInNamespace(this string input, string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return input;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"namespace {nameSpace}");
            stringBuilder.AppendLine("{");

            // Split the input into lines for indentation
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
                stringBuilder.AppendLine("    " + line);

            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private static (string, string) SplitStringAfter(string input, string splitAfter)
        {
            var index = input.IndexOf(splitAfter);
            if (index != -1)
            {
                // Include the length of splitAfter to get the rest of the string after it
                var splitPoint = index + splitAfter.Length;
                var before = input.Substring(0, splitPoint);
                var after = input.Substring(splitPoint);
                return (before, after);
            }

            return (input, string.Empty);
        }

        /// <summary> Creates a new text asset at a certain location. </summary>
        /// <param name="content"></param>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <exception cref="IOException"></exception>
        private static TextAsset CreateTextFile(string content, string fileName, string path)
        {
            var filePath = path + "/" + fileName;
            var folderPath = Directory.GetParent(Application.dataPath).FullName + "/" + path;
            var fullPath = folderPath + "/" + fileName;

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (File.Exists(fullPath))
                throw new IOException($"A file with the name {filePath} already exists.");

            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            return textAsset;
        }

        /// <summary> Returns all ScriptableObjects at a certain path. </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        public static List<T> FindAll<T>(string path = "") where T : ScriptableObject
        {
            var scriptableObjects = new List<T>();
            var searchFilter = $"t:{typeof(T).Name}";
            var soNames = path == ""
                ? AssetDatabase.FindAssets(searchFilter)
                : AssetDatabase.FindAssets(searchFilter, new[] { path });

            //we need this to make sure we load the sub assets only once.
            var mainAssetPath = new HashSet<string>();

            foreach (var soName in soNames)
            {
                var soPath = AssetDatabase.GUIDToAssetPath(soName); //sub assets all return the same path
                var asset = AssetDatabase.LoadAssetAtPath<T>(soPath);
                if (mainAssetPath.Contains(soPath))
                    continue;

                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(soPath);
                //if there are no subAssets, the asset is the main asset, and is of T
                if (subAssets.Length == 0)
                {
                    scriptableObjects.Add(asset);
                    continue;
                }

                foreach (var subAsset in subAssets)
                {
                    //still need to check if the sub asset is of the correct type
                    var subAssetCasted = subAsset as T; 
                    if (subAssetCasted != null)
                        scriptableObjects.Add(subAssetCasted);
                }

                mainAssetPath.Add(soPath);
            }

            return scriptableObjects;
        }


        internal static List<T> FindAll<T>(string path, out Dictionary<ScriptableBase, Object> subAssetLookUp)
            where T : ScriptableBase
        {
            var scriptableObjects = new List<T>();
            var searchFilter = $"t:{typeof(T).Name}";
            var soNames = path == ""
                ? AssetDatabase.FindAssets(searchFilter)
                : AssetDatabase.FindAssets(searchFilter, new[] { path });

            //we need this to make sure we load the sub assets only once.
            var mainAssetPath = new HashSet<string>();
            //returns the subAsset as key and the parent asset as value
            subAssetLookUp = new Dictionary<ScriptableBase, Object>();

            foreach (var soName in soNames)
            {
                var soPath = AssetDatabase.GUIDToAssetPath(soName); //sub assets all return the same path
                var asset = AssetDatabase.LoadAssetAtPath<T>(soPath);
                if (mainAssetPath.Contains(soPath))
                    continue;

                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(soPath);
                //if there are no subAssets, the asset is the main asset, and is of T
                if (subAssets.Length == 0)
                {
                    scriptableObjects.Add(asset);
                    continue;
                }

                foreach (var subAsset in subAssets)
                {
                    scriptableObjects.Add(subAsset as T);
                    var parent = AssetDatabase.LoadMainAssetAtPath(soPath);
                    subAssetLookUp.Add(subAsset as ScriptableBase, parent);
                }

                mainAssetPath.Add(soPath);
            }

            return scriptableObjects;
        }

        internal static void DrawSerializationError(Type type, Rect position = default)
        {
            if (position == default)
            {
                EditorGUILayout.HelpBox($"{type} Value field cannot be shown as it it not marked as serializable." +
                                        "\n Add [System.Serializable] attribute.", MessageType.Warning);
            }
            else
            {
                var icon = EditorGUIUtility.IconContent("Error").image;
                GUI.DrawTexture(position, icon, ScaleMode.ScaleToFit);
            }
        }

        public static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        internal static List<(GameObject, Type, string, string)> FindReferencesInScene(ScriptableBase scriptableBase)
        {
            var sceneReferences = new List<(GameObject, Type, string, string)>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded)
                    continue;

                var rootObjects = s.GetRootGameObjects();
                foreach (var rootObject in rootObjects)
                {
                    FindReferencesInGameObject(rootObject, scriptableBase, ref sceneReferences);
                }
            }

            return sceneReferences;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }

            return path;
        }

        private static void FindReferencesInGameObject(GameObject gameObject, ScriptableBase scriptableBase,
            ref List<(GameObject, Type, string, string)> sceneReferences)
        {
            var components = gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) //a missing scripts
                    continue;

                var serializedObject = new SerializedObject(component);
                var serializedProperty = serializedObject.GetIterator();
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                        serializedProperty.objectReferenceValue is ScriptableBase target)
                    {
                        if (target != scriptableBase)
                            continue;
                        var entry = (gameObject, component.GetType(), serializedProperty.name,
                            GetGameObjectPath(gameObject));
                        sceneReferences.Add(entry);
                    }
                }
            }

            foreach (Transform child in gameObject.transform)
                FindReferencesInGameObject(child.gameObject, scriptableBase, ref sceneReferences);
        }

        internal static Dictionary<string, int> FindReferencesInAssets(ScriptableBase scriptableBase)
        {
            var assetPaths = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.StartsWith("Assets/") && !path.EndsWith(".unity"));
            Dictionary<string, int> references = new Dictionary<string, int>();
            var pathHash = new HashSet<string>();
            foreach (var assetPath in assetPaths)
            {
                if (pathHash.Contains(assetPath)) //needed to filter sub assets as they all share the same path
                    continue;

                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var asset in assets)
                {
                    if (asset == null || AssetDatabase.IsSubAsset(asset))
                        continue;

                    var serializedObject = new SerializedObject(asset);
                    var property = serializedObject.GetIterator();
                    var referenceCount = 0;
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue == scriptableBase)
                        {
                            referenceCount++;
                        }
                    }

                    if (referenceCount > 0)
                    {
                        if (references.ContainsKey(assetPath))
                            references[assetPath] += referenceCount;
                        else
                            references.Add(assetPath, referenceCount);
                    }
                }

                pathHash.Add(assetPath);
            }

            return references;
        }
        
        internal static bool CanBeSubAsset(SerializedProperty property, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) //for Odin dictionary serialization
                return false;
            
            //main Asset has to be a SO
            var mainAsset = property.serializedObject.targetObject;
            if (mainAsset is ScriptableObject)
            {
                var isScriptableBase = fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableBase));
                if (!isScriptableBase)
                    return false;
                
                var hasSubAsset = HasAttribute<SubAssetAttribute>(fieldInfo);
                return hasSubAsset;
            }

            return false;
        }

        private static bool HasAttribute<T>(FieldInfo fieldInfo) where T : Attribute
        {
            if (fieldInfo != null)
            {
                T attribute = (T)Attribute.GetCustomAttribute(fieldInfo, typeof(T));
                return attribute != null;
            }

            return false;
        }

        internal static List<Object> GetAllSubAssets(Object mainAsset)
        {
            var mainAssetPath = AssetDatabase.GetAssetPath(mainAsset);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath);
            var subAssets = new List<Object>();
            foreach (var asset in allAssets)
            {
                if (asset == mainAsset || asset == null)
                    continue;

                subAssets.Add(asset);
            }

            return subAssets;
        }

        internal static void DeleteSubAsset(Object subAsset)
        {
            Object.DestroyImmediate(subAsset, true);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Clear editor Prefs for Soap.
        /// </summary>
        internal static void ClearEditorPrefs()
        {
            EditorPrefs.DeleteKey(SoapWizardWindow.PathKey);
            EditorPrefs.DeleteKey(SoapWizardWindow.FavoriteKey);
            EditorPrefs.DeleteKey(SoapWizardWindow.TagsKey);
            EditorPrefs.DeleteKey(SoapTypeCreatorWindow.DestinationFolderIndexKey);
            EditorPrefs.DeleteKey(SoapTypeCreatorWindow.DestinationFolderPathKey);
            EditorPrefs.DeleteKey(SoapWindow.LastCategoryKey);
            EditorPrefs.DeleteKey(SoapWindowInitializer.HasShownWindowKey);
            EditorPrefs.DeleteKey(CustomCreatePathKey);
            Debug.Log("Editor prefs deleted");
        }
    }
}