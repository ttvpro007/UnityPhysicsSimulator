using UnityEngine;

namespace Obvious.Soap.Example
{
    public class VfxSpawner : MonoBehaviour
    {
        [SerializeField] private ScriptableListPlayer scriptableListPlayer = null;

        [SerializeField] private GameObject _spawnVFXPrefab = null;
        [SerializeField] private GameObject _destroyVFXPrefab = null;
        
        public void Awake()
        {
            scriptableListPlayer.OnItemRemoved += OnPlayerDestroyed;
            scriptableListPlayer.OnItemAdded += OnPlayerSpawned;
        }
        
        public void OnDestroy()
        {
            scriptableListPlayer.OnItemRemoved -= OnPlayerDestroyed;
            scriptableListPlayer.OnItemAdded -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Player player)
        {
            Instantiate(_spawnVFXPrefab, player.transform.position, Quaternion.identity, transform);
        }

        private void OnPlayerDestroyed(Player player)
        {
            Instantiate(_destroyVFXPrefab, player.transform.position, Quaternion.identity, transform);
        }

       
    }
}