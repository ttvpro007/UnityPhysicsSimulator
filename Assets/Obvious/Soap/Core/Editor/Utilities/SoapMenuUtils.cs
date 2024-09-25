using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public static class SoapMenuUtils
    {
        [MenuItem("Tools/Obvious Game/Soap/Delete Player Pref %#d", priority = 0)]
        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log($"<color={SoapEditorUtils.SoapColorHtml}>--Player Prefs deleted--</color>");
        }

        [MenuItem("Tools/Obvious Game/Soap/ToggleFastPlayMode %l", priority = 1)]
        public static void ToggleFastPlayMode()
        {
            EditorSettings.enterPlayModeOptionsEnabled = !EditorSettings.enterPlayModeOptionsEnabled;
            AssetDatabase.Refresh();
            var text = EditorSettings.enterPlayModeOptionsEnabled
                ? "<color=#55efc4>Enabled"
                : $"<color={SoapEditorUtils.SoapColorHtml}>Disabled";
            text += "</color>";
            Debug.Log("Fast Play Mode " + text);
        }

        [MenuItem("CONTEXT/ScriptableVariableBase/Reset Value", false, 2)]
        private static void ResetValue(MenuCommand command) => ResetValue(command.context);

        private static void ResetValue(Object unityObject)
        {
            var reset = unityObject as IReset;
            reset.ResetValue();
        }

        [MenuItem("CONTEXT/ScriptableBase/Reset All", false, 1)]
        private static void Reset(MenuCommand command) => ResetAll(command.context);

        private static void ResetAll(Object unityObject)
        {
            var scriptableBase = unityObject as ScriptableBase;
            scriptableBase.Reset();
            EditorUtility.SetDirty(unityObject);
        }

        [MenuItem("CONTEXT/ScriptableObject/Delete All SubAssets", false, 0)]
        private static void DeleteAllSubAssets(MenuCommand command) => DeleteAllSubAssets(command.context);

        [MenuItem("Assets/Soap/Delete All SubAssets")]
        private static void DeleteAllSubAssets() => DeleteAllSubAssets(Selection.activeObject);

        [MenuItem("CONTEXT/ScriptableObject/Delete All SubAssets", true)]
        private static bool ValidateDeleteAllSubAssets(MenuCommand command) => CanDeleteAllSubAssets(command.context);

        [MenuItem("Assets/Soap/Delete All SubAssets", true)]
        private static bool ValidateDeleteAllSubAssets() => CanDeleteAllSubAssets(Selection.activeObject);

        private static bool CanDeleteAllSubAssets(Object obj)
        {
            var isScriptable = obj is ScriptableObject;
            if (!isScriptable)
                return false;
            var subAssets = SoapEditorUtils.GetAllSubAssets(Selection.activeObject);
            return subAssets.Count > 0;
        }

        private static void DeleteAllSubAssets(Object unityObject)
        {
            var subAssets = SoapEditorUtils.GetAllSubAssets(unityObject);
            foreach (var subAsset in subAssets)
                Object.DestroyImmediate(subAsset, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(unityObject), ImportAssetOptions.ForceUpdate);
        }

        [MenuItem("CONTEXT/ScriptableBase/\ud83d\udd0d Find References", false, 0)]
        private static void FindReferences(MenuCommand command) => FindReferenceFor(command.context);

        [MenuItem("Assets/Soap/\ud83d\udd0d Find References")]
        private static void FindReferences() => FindReferenceFor(Selection.activeObject);

        [MenuItem("CONTEXT/ScriptableBase/Find References", true)]
        private static bool ValidateFindReference(MenuCommand command) => CanFindReferenceFor(command.context);

        [MenuItem("Assets/Soap/\ud83d\udd0d Find References", true)]
        private static bool ValidateFindReference() => CanFindReferenceFor(Selection.activeObject);

        private static bool CanFindReferenceFor(Object obj)
        {
            var isScriptable = obj is ScriptableBase;
            return isScriptable;
        }

        private static void FindReferenceFor(Object unityObject)
        {
            var scriptableBase = unityObject as ScriptableBase;
            if (scriptableBase == null)
                return;
            EditorWindow mouseOverWindow = EditorWindow.mouseOverWindow;
            if (mouseOverWindow == null)
                return;

            ReferencesPopupWindow.ShowWindow(mouseOverWindow.position, scriptableBase);
        }
    }
}