using UnityEngine;

namespace Obvious.Soap.Editor
{
    using UnityEditor;

    [InitializeOnLoad]
    static class ScriptableBaseEditorHeader
    {
        private static SoapSettings _soapSettings;

        static ScriptableBaseEditorHeader()
        {
            _soapSettings = SoapEditorUtils.GetOrCreateSoapSettings();
            Editor.finishedDefaultHeaderGUI += DrawHeader;
        }

        private static void DrawHeader(Editor editor)
        {
            if (!EditorUtility.IsPersistent(editor.target))
                return;

            if (editor.targets.Length > 1)
            {
                //If there is more than one target, we check if they are all ScriptableBase
                foreach (var target in editor.targets)
                {
                    var scriptableBase = target as ScriptableBase;
                    if (scriptableBase == null)
                        return;
                }

                //Only draws the category for the selected target
                var scriptableTarget = editor.target as ScriptableBase;
                if (DrawTag(scriptableTarget))
                {
                    //Assign the category to all the targets
                    foreach (var target in editor.targets)
                    {
                        if (target == editor.target)
                            continue;
                        var scriptableBase = target as ScriptableBase;
                        Undo.RecordObject(scriptableBase, "Change Tag");
                        scriptableBase.TagIndex = scriptableTarget.TagIndex;
                        EditorUtility.SetDirty(scriptableBase);
                    }
                }
            }
            else
            {
                var scriptableBase = editor.target as ScriptableBase;
                if (scriptableBase == null)
                    return;

                DrawDescriptionAndCategory(scriptableBase);
            }

            void DrawDescriptionAndCategory(ScriptableBase scriptableBase)
            {
                EditorGUILayout.BeginHorizontal();
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Description:", labelStyle, GUILayout.Width(65));
                GUILayout.FlexibleSpace();
                DrawTag(scriptableBase);
                EditorGUILayout.EndHorizontal();
                
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
                textAreaStyle.wordWrap = true;
                EditorGUI.BeginChangeCheck();
              
                var description = EditorGUILayout.TextArea(scriptableBase.Description);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(scriptableBase, "Change Description");
                    scriptableBase.Description = description;
                    EditorUtility.SetDirty(scriptableBase);
                    Debug.Log("Description changed, assign it to the asset");
                }
            }

            bool DrawTag(ScriptableBase scriptableBase)
            {
                if (_soapSettings == null)
                    _soapSettings = SoapEditorUtils.GetOrCreateSoapSettings();

                var hasChanged = false;
                var tags = _soapSettings.Tags.ToArray();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tag:", EditorStyles.miniBoldLabel, GUILayout.Width(55f));
                EditorGUI.BeginChangeCheck();
                int newTagIndex = EditorGUILayout.Popup(scriptableBase.TagIndex, tags,GUILayout.MaxWidth(175));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(scriptableBase, "Change Tag");
                    scriptableBase.TagIndex = newTagIndex;
                    EditorUtility.SetDirty(scriptableBase);
                    hasChanged = true;
                }

                EditorGUILayout.EndHorizontal();
                return hasChanged;
            }
        }
    }
}