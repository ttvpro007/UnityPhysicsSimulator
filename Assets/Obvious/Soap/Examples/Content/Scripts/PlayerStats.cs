using Obvious.Soap.Attributes;
using UnityEngine;

namespace Obvious.Soap.Example
{
    [CreateAssetMenu(menuName = "Soap/Examples/SubAssets/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [SerializeField] [SubAsset] private FloatVariable _speed;
        [SubAsset] public FloatVariable Health;
        [SubAsset] public FloatVariable MaxHealth;
    }
}