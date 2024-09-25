using System;

namespace Obvious.Soap
{
    [Serializable]
    public abstract class VariableReference<V, T> where V : ScriptableVariable<T>
    {
        public bool UseLocal = false;
        public T LocalValue;
        public V Variable;

        private Action<T> _onValueChanged;

        public T Value
        {
            get => UseLocal ? LocalValue : Variable.Value;
            set
            {
                if (UseLocal)
                {
                    if (Equals(LocalValue, value))
                        return;

                    LocalValue = value;
                    _onValueChanged?.Invoke(LocalValue);
                }
                else
                {
                    Variable.Value = value;
                }
            }
        }

        /// <summary> Event raised when the variable reference value changes.
        /// Is triggered for both local and variable. </summary>
        public event Action<T> OnValueChanged
        {
            add
            {
                _onValueChanged += value;
                if (Variable != null)
                    Variable.OnValueChanged += value;
            }
            remove
            {
                _onValueChanged -= value;
                if (Variable != null)
                    Variable.OnValueChanged -= value;
            }
        }

        public static implicit operator T(VariableReference<V, T> reference)
        {
            return reference.Value;
        }
    }
}