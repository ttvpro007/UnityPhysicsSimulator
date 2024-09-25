using UnityEngine;

namespace Obvious.Soap.Example
{
    public class RuntimeHealth : MonoBehaviour
    {
        [Tooltip("Leave this null, it will be instantiate at runtime")] [SerializeField]
        private FloatVariable _runtimeHpVariable;

        [SerializeField] private FloatReference _maxHealth = null;

        private void Awake()
        {
            //If the variable is not set in the inspector, create a runtime instance.
            if (_runtimeHpVariable == null)
                _runtimeHpVariable = SoapUtils.CreateRuntimeInstance<FloatVariable>($"{gameObject.name}_Hp");

            //Initialize the health bar only after the variable has been created.
            //You can use events to decouple components if your health bar is in another scene (UI scene for example). 
            //In this case, as it's a local Health bar, a direct reference is fine. 
            GetComponentInChildren<HealthBarSprite>().Init(_runtimeHpVariable, _maxHealth.Value);
        }

        private void Start()
        {
            //For the runtime variables, you can do this in Awake, after creating the runtime instance.
            //However, because doing this in Start works for both cases (when creating a runtime variable and referencing an existing one), it's better to do it in Start.
            //Indeed, for the Boss, its value is reset OnSceneLoaded and because of the execution order:
            //Awake-> SceneLoaded -> Start , the value should be reset to the max health.
            //Note that you could also remove Awake entirely and have its logic in Start() before these lines.
            _runtimeHpVariable.Value = _maxHealth.Value;
            _runtimeHpVariable.OnValueChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _runtimeHpVariable.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float newValue)
        {
            if (newValue <= 0f)
                gameObject.SetActive(false);
        }

        //In this example, this is called when the enemy collides with the Player.
        public void TakeDamage(int amount) => _runtimeHpVariable.Add(-amount);
    }
}