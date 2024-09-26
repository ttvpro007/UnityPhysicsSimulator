using UnityEngine;
using DamageNumbersPro;

public class DamageNumberSpawner : MonoBehaviour
{
    //Assign prefab in inspector.
    [SerializeField] private DamageNumber numberPrefab;

    public void SpawnNumber(int value)
    {
        //Spawn new popup at transform.position.
        numberPrefab.Spawn(transform.position, value);
    }
}