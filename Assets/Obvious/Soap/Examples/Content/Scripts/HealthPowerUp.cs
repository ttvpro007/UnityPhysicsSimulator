using System;
using UnityEngine;

namespace Obvious.Soap.Example
{
    public class HealthPowerUp : MonoBehaviour
    {
        [SerializeField] private PlayerStats _playerStats;

        private void OnTriggerEnter(Collider other)
        {
            _playerStats.Health.Add(30);
        }
    }
}