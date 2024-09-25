using UnityEngine;

namespace Obvious.Soap.Example
{
    public class HealVignette : MonoBehaviour
    {
        [SerializeField] private ScriptableEventInt _onPlayerHealedEvent = null;
        [SerializeField] private FadeOut _fadeOut = null;

        private void Awake()
        {
            _onPlayerHealedEvent.OnRaised += OnPlayerHealed;
        }

        private void OnDestroy()
        {
            _onPlayerHealedEvent.OnRaised -= OnPlayerHealed;
        }

        private void OnPlayerHealed(int amount)
        {
            _fadeOut.Activate();
        }
    }
}