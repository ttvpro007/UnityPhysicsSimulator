using Obvious.Soap.Attributes;
using UnityEngine;

namespace Obvious.Soap.Example
{
    [CreateAssetMenu(menuName = "Soap/Examples/SubAssets/PlayerEvents")]
    public class PlayerEvents : ScriptableObject
    {
        [SerializeField] [SubAsset] private ScriptableEventInt _onDamaged;
        [SerializeField] [SubAsset] private ScriptableEventInt _onHealed;
        [SerializeField] [SubAsset] private ScriptableEventNoParam _onDeath;
    }
}