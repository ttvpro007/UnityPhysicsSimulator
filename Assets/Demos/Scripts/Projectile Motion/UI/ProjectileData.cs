using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Data", menuName = "Demo/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [SerializeField] private float delayDuration;
    [SerializeField] private float baseDamage;
}

