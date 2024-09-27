using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityPBR.Modules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityPBR
{
    [CustomEditor(typeof(AudioClipCombiner))]
    [CanEditMultipleObjects]
    [Serializable]
    public class AudioClipCombinerEditor : Editor
    {
        private AudioClipCombiner Target;
        private int _updateData = -1;
        
        private Dictionary<GameObject, float> OneShotAudios = new Dictionary<GameObject, float>();
        private float _timeToCleanUp = -1f;

        private void OnEnable() => Target = (AudioClipCombiner) target;
        private void OnValidate() => Target.Validations();

        private void CleanUp()
        {
            List<GameObject> objectsToRemove = new List<GameObject>();
    
            foreach(var oneShotAudio in OneShotAudios)
            {
                if (oneShotAudio.Value < Time.realtimeSinceStartup)
                {
                    string filePath = $"Assets/Audio Clip Exports/{oneShotAudio.Key.name}.wav";
                    
                    // Check if file exists before trying to delete
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                
                        // If there is a .meta file, we should delete this too
                        if(File.Exists(filePath + ".meta"))
                        {
                            File.Delete(filePath + ".meta");
                        }
#if UNITY_EDITOR
                        // Make sure Unity refreshes and acknowledges the file deletion
                        AssetDatabase.Refresh();
#endif
                    }
            
                    objectsToRemove.Add(oneShotAudio.Key);
                }
            }

            foreach (GameObject gameObject in objectsToRemove)
            {
                OneShotAudios.Remove(gameObject);
                DestroyImmediate(gameObject);
            }
        }

        
        public override void OnInspectorGUI()
        {
            CleanUp();
            UpdateData();
            Target.Validations();
            //ShowInstructions();
            https://infinitypbr.gitbook.io/infinity-pbr/audio-clip-combiner
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button($"Docs & Tutorials"))
                Application.OpenURL("https://infinitypbr.gitbook.io/infinity-pbr/audio-clip-combiner");
            GUI.backgroundColor = Color.white;
            
            Undo.RecordObject(Target, "Undo Changes");

            if (Target.audioLayers.Count == 0)
            {
                EditorGUILayout.HelpBox($"Drag a folder into this field. Any audio clips which are direct children of the folder will be added to a " +
                                        $"new layer. Any folders that are direct children, and have audio clips, will each be added to a new layer.", MessageType.Info);
                AddNewFolderAndSubfolders();
                EditorGUILayout.HelpBox($"If you prefer, you can also create your first two audio layers. Two audio layers minimum are required.", MessageType.Info);
                AddTwoAudioLayers();
            }
            else
            {
                ShowMainDetails();
                ShowAudioLayers();
            }
            
            
            ShowFullInspector();
            
            EditorUtility.SetDirty(Target);
        }

        private void UpdateData()
        {
            if (_updateData < 0) return;

            UpdateData(Target.audioLayers[_updateData]);
            _updateData = -1;
        }

        private void ShowMainDetails()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Name", GUILayout.Width(100));
            var tempName = Target.outputName;
            Target.outputName = EditorGUILayout.DelayedTextField(Target.outputName, GUILayout.Width(150));
            if (Target.outputName != tempName)
            {
                Target.name = Target.outputName;
                var targetPath = AssetDatabase.GetAssetPath(Target);
                AssetDatabase.RenameAsset(targetPath, Target.outputName);
                DirtyAndRefresh();
            }
            EditorGUILayout.ObjectField(Target, typeof(AudioClipCombiner), false, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            ExportButton();
            ExportData();
        }

        private void ExportButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = CanExport() ? Color.green : Color.black;
            GUI.contentColor = CanExport() ? Color.white : Color.grey;
            if(GUILayout.Button($"Export {Target.TotalExports()} Clip Combinations") && CanExport()) Target.ExportNow();
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            if (GUILayout.Button($"Play Random Sample"))
            {
                var randomName = GUID.Generate().ToString();
                Target.ExportOneRandom(randomName);
                PlaySample(randomName);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ExportData()
        {
            if (!CanExport()) return;
            
            EditorGUILayout.LabelField($"Exported Clips: {Target.TotalExports()} files from {Target.audioLayers.Count} Audio Layers");
            EditorGUILayout.LabelField($"Clip Durations: {Target.shortestClipLength} to {Target.longestClipLength} seconds");
        }

        public bool CanExport()
        {
            if (Target.audioLayers.Count == 0) return false;
            if (!Target.audioLayers.All(x => x.HasClips())) return false;
            if (Target.audioLayers.Count < 2) return false;

            return true;
        }

        private void ShowInstructions()
        {
            EditorGUILayout.HelpBox($"At least two Audio Layers are required. All Audio Layers must have at least one Audio Clip.\n\n" +
                                    $"Audio Layers will be combined " +
                                    $"together and exported as stand-alone AudioClip (.wav) files. If you add multiple " +
                                    $"AudioClip files to an Audio Layer, every combination of potential clips will be " +
                                    $"exported.\n\nFiles will be exported to {Target.PathOutput}", MessageType.Info);
            
            if (!File.Exists(Target.PathExample)) return;
            
            EditorGUILayout.HelpBox($"Exports will overwrite existing files in {Target.PathOutput} etc", MessageType.Warning);
        }

        private void ShowAudioLayers()
        {
            EditorGUILayout.Space();

            foreach (AudioClipCombiner.AudioLayer audioLayer in Target.audioLayers)
                ShowAudioLayer(audioLayer);

            ShowAddNewAudioLayer();
        }

        private void PlayRandom(AudioClipCombiner.AudioLayer audioLayer)
        {
            if (audioLayer.clips.Count == 0) return;
            AudioClip clip = audioLayer.clips[UnityEngine.Random.Range(0, audioLayer.clips.Count)];
            var randomName = GUID.Generate().ToString();
            PlayAudioClip(clip, Vector3.zero, randomName);
        }
        
        private void PlaySample(string randomName)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio Clip Exports/{randomName}.wav");
            PlayAudioClip(clip, Vector3.zero, randomName);
        }

        private AudioSource PlayAudioClip(AudioClip clip, Vector3 position, string randomName)
        {
            GameObject audioObject = new GameObject(randomName);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1.0f; // make it 3D audio
            audioObject.transform.position = position;
            audioSource.Play();

            // Save the GameObject for later destruction
            OneShotAudios.Add(audioObject, Time.realtimeSinceStartup + clip.length);

            return audioSource;
        }
        
        private void ShowAudioLayer(AudioClipCombiner.AudioLayer audioLayer)
        {
            CheckName(audioLayer);
            if (audioLayer.expanded) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            //audioLayer.expanded = EditorGUILayout.Foldout(audioLayer.expanded, audioLayer.name);
            if (GUILayout.Button(audioLayer.expanded ? "O" : "-", GUILayout.Width(25)))
                audioLayer.expanded = !audioLayer.expanded;
            
            audioLayer.name = EditorGUILayout.TextField(audioLayer.name, GUILayout.Width(150));
            
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField(new GUIContent("Volume", "Relative to the default volume of the clip"), GUILayout.Width(50));
            audioLayer.volume = EditorGUILayout.FloatField(audioLayer.volume, GUILayout.Width(30));
            
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField(new GUIContent("Delay", "Empty space before this clip comes in"),GUILayout.Width(50));
            var tempDelay = audioLayer.delay;
            audioLayer.delay = EditorGUILayout.FloatField(audioLayer.delay, GUILayout.Width(30));
            if (tempDelay != audioLayer.delay)
                UpdateData(audioLayer);

            EditorGUILayout.LabelField("", GUILayout.Width(20));
            GUI.contentColor = audioLayer.clips.Count == 0 ? Color.red : Color.grey;
            EditorGUILayout.LabelField($"Clips: {audioLayer.clips.Count}", GUILayout.Width(70));
            if (audioLayer.clips.Count > 0)
            {
                EditorGUILayout.LabelField($"Length: {audioLayer.shortestClipLength}{(audioLayer.clips.Count > 1 ? $" - {audioLayer.longestClipLength}" : "")}", GUILayout.Width(200));
            }
            GUI.contentColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            if (!audioLayer.expanded) return;

            for (int i = 0; i < audioLayer.clips.Count; i++)
                ShowClip(audioLayer, i);
            AddNewClip(audioLayer);
            AddNewFolder(audioLayer);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Random", GUILayout.Width(150)))
                PlayRandom(audioLayer);

            GUI.color = Color.red;
            if (GUILayout.Button($"Remove {audioLayer.name}", GUILayout.Width(200)))
            {
                Target.audioLayers.RemoveAll(x => x == audioLayer);
                GUIUtility.ExitGUI();
            }
            
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
        }

        private void ShowClip(AudioClipCombiner.AudioLayer audioLayer, int i)
        {
            EditorGUILayout.BeginHorizontal();
            ShowClipDelete(audioLayer, i);
            ShowClipObject(audioLayer, i);
            ShowClipData(audioLayer, i);
            EditorGUILayout.EndHorizontal();
        }

        private void ShowClipData(AudioClipCombiner.AudioLayer audioLayer, int i)
        {
            var clip = audioLayer.clips[i];
            GUI.color = clip.length == audioLayer.shortestClipLength || clip.length == audioLayer.longestClipLength ? Color.cyan : Color.grey;
            EditorGUILayout.LabelField($"Length: {clip.length}", GUILayout.Width(120));
            GUI.color = Color.grey;
            EditorGUILayout.LabelField($"Frequency: {clip.frequency}", GUILayout.Width(120));
            EditorGUILayout.LabelField($"Channels: {clip.channels}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"Samples: {clip.samples}", GUILayout.Width(120));
            GUI.color = Color.white;
        }

        private void ShowClipObject(AudioClipCombiner.AudioLayer audioLayer, int i)
        {
            var tempClip = audioLayer.clips[i];
            audioLayer.clips[i] = EditorGUILayout.ObjectField(audioLayer.clips[i], typeof(AudioClip), GUILayout.Width(150)) as AudioClip;
            if (tempClip != audioLayer.clips[i])
                UpdateData(audioLayer);
        }

        private void UpdateData(AudioClipCombiner.AudioLayer audioLayer = null)
        {
            if (audioLayer != null)
            {
                audioLayer.UpdateLengths();
            }
            
            Target.UpdateLengths();
        }

        private void ShowClipDelete(AudioClipCombiner.AudioLayer audioLayer, int i)
        {
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                audioLayer.clips.RemoveAt(i);
                _updateData = i;
                GUIUtility.ExitGUI();
            }
            GUI.color = Color.white;
        }

        private void AddNewClip(AudioClipCombiner.AudioLayer audioLayer)
        {
            GUI.color = Color.yellow;
            var newClip = EditorGUILayout.ObjectField("Add Clip", null, typeof(AudioClip), false) as AudioClip;
            GUI.color = Color.white;

            if (newClip == null) return;
            audioLayer.AddClip(newClip);
            Target.UpdateLengths();
            DirtyAndRefresh();
        }
        
        private void AddNewFolderAndSubfolders()
        {
            GUI.color = Color.yellow;
            var newFolder = EditorGUILayout.ObjectField("Add Folder With Sub-folders", null, typeof(DefaultAsset), false);
            GUI.color = Color.white;

            if (newFolder == null) return;

            var path = AssetDatabase.GetAssetPath(newFolder);
            if (!Directory.Exists(path)) 
            {
                Debug.LogWarning("The selected asset is not a folder!");
                return;
            }
            
            string pathName = Path.GetFileName(path);

            // NEW CODE HERE
            
            // Create a new layer for this path if there any clips.
            if (AudioClipsInPath(path) > 0)
            {
                AddNewAudioLayer(false);
                var newAudioLayer = Target.audioLayers[Target.audioLayers.Count - 1];
                AddClipsFromPath(newAudioLayer, path);
            }
            
            // Iterate over each sub-directory directly under the current directory
            string[] subdirectories = Directory.GetDirectories(path);
            foreach (string subdirectory in subdirectories)
            {
                // Process each sub-directory
                string subdirectoryName = Path.GetFileName(subdirectory);
                Debug.Log($"Processing sub-directory: {subdirectoryName}");

                // Now you can perform any operation on the subdirectory, like checking the number of audio clips
                int clipCount = AudioClipsInPath(subdirectory);
                Debug.Log($"Found {clipCount} audio clips in the sub-directory: {subdirectoryName}");

                // Or, you could add the clips from the subdirectory to a new audio layer
                if (clipCount > 0)
                {
                    AddNewAudioLayer(false);
                    var newAudioLayer = Target.audioLayers[Target.audioLayers.Count - 1];
                    AddClipsFromPath(newAudioLayer, subdirectory);
                }
            }

            if (Target.audioLayers.Any() && Target.name.StartsWith("Audio Clip Combiner"))
            {
                Target.name = pathName;
                var targetPath = AssetDatabase.GetAssetPath(Target);
                AssetDatabase.RenameAsset(targetPath, pathName);
                Target.outputName = pathName;
            }
            
            DirtyAndRefresh();
        }

        private void DirtyAndRefresh()
        {
            EditorUtility.SetDirty(Target);  // Marks this asset as "dirty", which means changes have been made
            AssetDatabase.SaveAssets();  // Saves all changed assets to disk
            AssetDatabase.Refresh();  // Imports/Reloads all assets in the AssetDatabase
        }

        private void AddNewFolder(AudioClipCombiner.AudioLayer audioLayer)
        {
            GUI.color = Color.yellow;
            var newFolder = EditorGUILayout.ObjectField("Add Folder", null, typeof(DefaultAsset), false);
            GUI.color = Color.white;

            if (newFolder == null) return;

            var path = AssetDatabase.GetAssetPath(newFolder);
            if (!Directory.Exists(path)) 
            {
                Debug.LogWarning("The selected asset is not a folder!");
                return;
            }

            AddClipsFromPath(audioLayer, path);
        }

        private int AudioClipsInPath(string path)
        {
            // Fetch all the AudioClip objects in this directory (not including subdirectories)
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new string[]{path});
            int clipCount = 0;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string directoryName = Path.GetDirectoryName(assetPath);
                if (directoryName == path) clipCount++;
            }

            return clipCount;
        }

        private void AddClipsFromPath(AudioClipCombiner.AudioLayer audioLayer, string path)
        {
            string folderName = Path.GetFileName(path);
            if (audioLayer.name == "Unnamed Audio Layer")
                audioLayer.name = folderName;
    
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new string[]{path});

            var added = 0;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string directoryName = Path.GetDirectoryName(assetPath);
                if (directoryName != path) continue;

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    added++;
                    audioLayer.AddClip(clip);
                }
            }

            if (added == 0)
            {
                Debug.LogWarning(
                    $"No clips were added. Note, this will only add audio clips that are the direct children " +
                    $"of the selected folder.");
            }

            Target.UpdateLengths();
        }


        private void CheckName(AudioClipCombiner.AudioLayer audioLayer)
        {
            if (string.IsNullOrWhiteSpace(audioLayer.name)) audioLayer.name = "Unnamed Audio Layer";
            audioLayer.name = audioLayer.name.Trim();
        }

        private void ShowAddNewAudioLayer()
        {
            GUI.color = Color.yellow;
            if (GUILayout.Button("Add new Audio Layer", GUILayout.Width(200)))
            {
                AddNewAudioLayer();
            }
            GUI.color = Color.white;
        }
        
        private void AddTwoAudioLayers()
        {
            GUI.color = Color.yellow;
            if (GUILayout.Button("Add Two Audio Layers", GUILayout.Width(200)))
            {
                AddNewAudioLayer(false);
                AddNewAudioLayer();
                DirtyAndRefresh();
            }
            GUI.color = Color.white;
        }

        private void AddNewAudioLayer(bool exitGui = true)
        {
            Target.audioLayers.Add(new AudioClipCombiner.AudioLayer());
            var newObject = Target.audioLayers.Last();
            newObject.volume = 1f;
            newObject.delay = 0f;
            newObject.name = "Unnamed Audio Layer";
            newObject.expanded = true;
            newObject.parent = Target;
            
            if (exitGui)
                GUIUtility.ExitGUI();
        }

        private void ShowFullInspector()
        {
            EditorGUILayout.Space();
            Target.displayInspector =
                EditorGUILayout.ToggleLeft("Show full inspector", Target.displayInspector);

            if (!Target.displayInspector) return;
            
            DrawDefaultInspector();
        }
    }
}
