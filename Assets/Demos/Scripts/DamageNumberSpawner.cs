using UnityEngine;
using DamageNumbersPro;

public class DamageNumberSpawner : MonoBehaviour
{
    //Assign prefab in inspector.
    [SerializeField] private DamageNumber numberPrefab;
    [SerializeField] private DamageNumber critNumberPrefab;

    public void SpawnNumber(int value)
    {
        //Spawn new popup at transform.position.
        numberPrefab.Spawn(transform.position, value);
    }

    public void SpawnCritNumber(int value)
    {
        //Spawn new popup at transform.position.
        critNumberPrefab.Spawn(transform.position, value);
    }
}