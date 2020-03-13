using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment.Behaviours
{
    public class PickUp : MonoBehaviour
    {
        [SerializeField] private float pickUpValue = 0f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag != "Player") return;
            GameManager.Instance.AddToScore(pickUpValue);
            Destroy(gameObject);
        }
    }
}