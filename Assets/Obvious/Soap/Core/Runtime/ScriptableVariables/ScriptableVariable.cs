using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using Obvious.Soap.Attributes;
#endif

namespace Obvious.Soap
{
    public abstract class ScriptableVariable<T> : ScriptableVariableBase, IReset, IDrawObjectsInInspector
    {
        [Tooltip(
            "The value of the variable. This will be reset on play mode exit to the value it had before entering play mode.")]
        [SerializeField]
#if ODIN_INSPECTOR
        [HideInPlayMode][PropertyOrder(-1)]
#endif
        protected T _value;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        protected bool _debugLogEnabled;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        [Tooltip("If true, saves the value to Player Prefs and loads it onEnable.")] [SerializeField]
        protected bool _saved;

        [Tooltip(
            "The default value of this variable. When loading from PlayerPrefs the first time, it will be set to this value.")]
        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf("_saved")]
        [PropertyOrder(2)]
        [Indent]
        [BoxGroup]
#else
        [ShowIf("_saved", true)]
#endif
        private T _defaultValue;

#if ODIN_INSPECTOR
        [PropertyOrder(5)]
#endif
        [Tooltip("Reset to initial value." +
                 " Scene Loaded : when the scene is loaded." +
                 " Application Start : Once, when the application starts." +
                 " None : Never.")]
        [SerializeField]
        private ResetType _resetOn = ResetType.SceneLoaded;

        public override ResetType ResetType
        {
            get => _resetOn;
            set => _resetOn = value;
        }
        
        protected T _initialValue;

        [SerializeField] 
#if ODIN_INSPECTOR
        [HideInEditorMode][PropertyOrder(-1)]
#endif
        protected T _runtimeValue;

        private readonly List<Object> _listenersObjects = new List<Object>();
#if ODIN_INSPECTOR
        [HideInEditorMode]
        [ShowInInspector,EnableGUI]
        [PropertyOrder(100)]
        public IEnumerable<Object> ObjectsReactingToOnValueChangedEvent  => _listenersObjects;
#endif

        protected Action<T> _onValueChanged;

        /// <summary> Event raised when the variable value changes. </summary>
        public event Action<T> OnValueChanged
        {
            add
            {
                _onValueChanged += value;

                var listener = value.Target as Object;
                if (listener != null && !_listenersObjects.Contains(listener))
                    _listenersObjects.Add(listener);
            }
            remove
            {
                _onValueChanged -= value;

                var listener = value.Target as Object;
                if (_listenersObjects.Contains(listener))
                    _listenersObjects.Remove(listener);
            }
        }

        /// <summary>
        /// The previous value just after the value changed.
        /// </summary>
        public T PreviousValue { get; private set; }

        /// <summary>
        /// The default value this variable is reset to. 
        /// </summary>
        public T DefaultValue => _defaultValue;

        /// <summary>
        /// Modify this to change the value of the variable.
        /// Triggers OnValueChanged event.
        /// </summary>
        public virtual T Value
        {
#if UNITY_EDITOR
            get => GetEditorValue;
            set
            {
                if (Equals(GetEditorValue, value))
                    return;
                GetEditorValue = value;
                ValueChanged();
            }
#else
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                ValueChanged();
            }
#endif
        }

        protected virtual void ValueChanged()
        {
#if UNITY_EDITOR
            _onValueChanged?.Invoke(GetEditorValue);
#else
            _onValueChanged?.Invoke(_value);
#endif

            if (_debugLogEnabled)
            {
                var suffix = _saved ? " <color=#f75369>[Saved]</color>" : "";
                Debug.Log($"{GetColorizedString()}{suffix}", this);
            }

            if (_saved)
                Save();

#if UNITY_EDITOR
            PreviousValue = GetEditorValue;
            if (this != null) //for runtime variables, the instance will be destroyed so do not repaint.
                RepaintRequest?.Invoke();
#else
            PreviousValue = _value;
#endif
        }

        public override Type GetGenericType => typeof(T);

        protected virtual void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Init();
#endif
            if (_resetOn == ResetType.SceneLoaded)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //the reset mode can change after the game has started, so we need to check it here.
            if (_resetOn != ResetType.SceneLoaded)
                return;

            if (mode == LoadSceneMode.Single)
            {
                if (_saved)
                    Load();
                else
                    ResetValue();
            }
        }

#if UNITY_EDITOR

        private T GetEditorValue
        {
            get => Application.isPlaying ? _runtimeValue : _value;
            set
            {
                if (Application.isPlaying)
                    _runtimeValue = value;
                else
                    _value = value;
            }
        }

        public void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                Init();
            }
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (!_saved)
                {
                    ResetValue();
                }
                else
                {
                    if (Equals(_value, _runtimeValue))
                        return;

                    // Set the value to the runtime value
                    _value = _runtimeValue;
                    _runtimeValue = _initialValue;
                    EditorUtility.SetDirty(this);
                    RepaintRequest?.Invoke();
                }
            }
        }

        protected virtual void OnValidate()
        {
            //In default play mode, this get called before OnEnable(). Therefore a saved variable can get saved before loading. 
            //This check prevents the latter.
            if (Equals(GetEditorValue, PreviousValue))
                return;
            ValueChanged();
        }

        /// <summary> Reset the SO to default.</summary>
        internal override void Reset()
        {
            base.Reset();
            _listenersObjects.Clear();
            Value = default;
            _initialValue = default;
            _runtimeValue = default;
            PreviousValue = default;
            _saved = false;
            _resetOn = ResetType.SceneLoaded;
            _debugLogEnabled = false;
        }
#endif

        private void Init()
        {
            if (_saved)
                Load();
            
            _initialValue = _value;
            _runtimeValue = _value;
            PreviousValue = _value;
            _listenersObjects.Clear();
        }

        /// <summary> Reset to initial value</summary>
        public void ResetValue()
        {
            Value = _initialValue;
            PreviousValue = _initialValue;
            _runtimeValue = _initialValue;
        }

        public virtual void Save()
        {
        }

        public virtual void Load()
        {
            PreviousValue = _value;

            if (_debugLogEnabled)
                Debug.Log($"{GetColorizedString()} <color=#f75369>[Loaded].</color>", this);
        }

        public override string ToString()
        {
            return $"{name} : {Value}";
        }

        protected virtual string GetColorizedString() => $"<color=#f75369>[Variable]</color> {ToString()}";

        public List<Object> GetAllObjects() => _listenersObjects;

        public static implicit operator T(ScriptableVariable<T> variable) => variable.Value;
    }

    /// <summary>
    /// Defines when the variable is reset.
    /// </summary>
    public enum ResetType
    {
        SceneLoaded,
        ApplicationStarts,
        None
    }

    public enum CustomVariableType
    {
        None,
        Bool,
        Int,
        Float,
        String
    }
}