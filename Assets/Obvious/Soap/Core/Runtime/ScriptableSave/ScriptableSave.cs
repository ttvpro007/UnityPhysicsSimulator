using System;
using System.Diagnostics;
using System.IO;
using Obvious.Soap.Attributes;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Obvious.Soap
{
    public abstract class ScriptableSave<T> : ScriptableSaveBase where T : class, new()
    {
        [SerializeField] protected bool _debugLogEnabled = false;
        [SerializeField] protected ELoadMode _loadMode = ELoadMode.Automatic;
        public ELoadMode LoadMode => _loadMode;
        [SerializeField] protected ESaveMode _saveMode = ESaveMode.Manual;
        public ESaveMode SaveMode => _saveMode;
#if ODIN_INSPECTOR
        [ShowIf("_saveMode", ESaveMode.Interval)]
#else
        [ShowIf(nameof(_saveMode), ESaveMode.Interval)] 
#endif
        [SerializeField]
        protected double _secondsBetweenSave = 120f;

        public double SecondsBetweenSave { get => _secondsBetweenSave; set => _secondsBetweenSave = value; }

        [Header("Runtime Data")] [SerializeField]
        protected T _saveData = new T();

        protected double _lastSaveTime;

        public string SavePath => $"{Application.persistentDataPath}" +
                                   $"{Path.AltDirectorySeparatorChar}" +
                                   $"{name}.json";

        private string SaveDirectory => Path.GetDirectoryName(SavePath);
        public string LastJsonString { get; private set; }
        public event Action OnSaved = null;
        public event Action OnLoaded = null;
        public event Action OnDeleted = null;

        public bool SaveFileExists => File.Exists(SavePath);
        public override Type GetGenericType => typeof(T);

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

#else
            if (_loadMode == ELoadMode.Automatic)
            {
                Load();
            }
             if (_saveMode == ESaveMode.Interval)
            {
                ScriptableObjectUpdateSystem.RegisterObject(this);
            }
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

#else
            ScriptableObjectUpdateSystem.UnregisterObject(this);
#endif
        }

        public override void Save()
        {
            LastJsonString = JsonUtility.ToJson(_saveData, prettyPrint: true);

            try
            {
                using (StreamWriter writer = new StreamWriter(SavePath))
                {
                    writer.Write(LastJsonString);
                }

                OnSaved?.Invoke();
                if (_debugLogEnabled)
                    Debug.Log("<color=#f75369>Save Saved:</color> \n" + LastJsonString);

                _lastSaveTime = Time.time;
#if UNITY_EDITOR
                RepaintRequest?.Invoke();
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving file: {e.Message}");
            }
        }

        public override void Load()
        {
            if (!SaveFileExists)
            {
                Save();
            }

            try
            {
                using (StreamReader reader = new StreamReader(SavePath))
                {
                    LastJsonString = reader.ReadToEnd();
                    if (_debugLogEnabled)
                        Debug.Log("<color=#f75369>Save Loaded:</color> \n" + LastJsonString);
                    var saveData = JsonUtility.FromJson(LastJsonString, GetGenericType);
                    RestoreSaveData(saveData);
                    OnLoaded?.Invoke();
#if UNITY_EDITOR
                    RepaintRequest?.Invoke();
#endif
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file: {e.Message}");
            }
        }

        private void RestoreSaveData(object data)
        {
            var saveData = data as T;
            if (NeedsUpgrade(saveData))
            {
                UpgradeData(saveData);
            }

            _saveData = saveData;
        }

        public virtual void Delete()
        {
            if (SaveFileExists)
                File.Delete(SavePath);

            Debug.Log("<color=#f75369>Save Deleted: </color>" + SavePath);
            LastJsonString = string.Empty;
            ResetSaveData();
            OnDeleted?.Invoke();
#if UNITY_EDITOR
            RepaintRequest?.Invoke();
#endif
        }

        public virtual void PrintToConsole()
        {
            Debug.Log($"<color=#f75369>Save Data:</color>\n{LastJsonString}");
        }

        public override void CheckAndSave()
        {
            if (Time.time - _lastSaveTime >= _secondsBetweenSave)
            {
                Save();
                _lastSaveTime = Time.time;
            }
        }

        public void OpenSaveLocation()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Debug.LogWarning("Save directory does not exist yet.");
                return;
            }

            try
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Process.Start("explorer.exe", SaveDirectory);
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                Process.Start("open", SaveDirectory);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                Process.Start("xdg-open", SaveDirectory);
#else
                Debug.LogWarning("Opening save location is not supported on this platform.");
#endif

                if (_debugLogEnabled)
                    Debug.Log($"Opened save location: {SaveDirectory}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open save location: {e.Message}");
            }
        }

        internal override void Reset()
        {
            base.Reset();
            _debugLogEnabled = false;
            _loadMode = ELoadMode.Automatic;
            _saveMode = ESaveMode.Manual;
            ResetSaveData();
        }

        private void ResetSaveData() => _saveData = new T();
        protected virtual bool NeedsUpgrade(T saveData) => false;

        protected virtual void UpgradeData(T oldData)
        {
        }
        
#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                if (_loadMode == ELoadMode.Automatic)
                {
                    Load();
                }

                if (_saveMode == ESaveMode.Interval)
                {
                    ScriptableObjectUpdateSystem.RegisterObject(this);
                }
            }
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                ScriptableObjectUpdateSystem.UnregisterObject(this);
            }
        }
#endif
    }
}