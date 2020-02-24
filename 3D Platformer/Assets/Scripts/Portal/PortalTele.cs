using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTele : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform receiver;

    private bool playerIsOverlapping = false;

    private void Update()
    {
        if (playerIsOverlapping)
            TeleportPlayer();
    }

    private void TeleportPlayer()
    {
        Vector3 portalToPlayer = player.position - transform.position;
        float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

        if (dotProduct < 0)
        {
            float rotationDiff = -Quaternion.Angle(transform.rotation, receiver.rotation);
            rotationDiff += 180;
            player.Rotate(Vector3.up, rotationDiff);

            Vector3 positionOffset = Quaternion.Euler(0, rotationDiff, 0) * portalToPlayer;
            player.position = receiver.position + positionOffset;
            
            gameObject.SetActive(false);
            receiver.gameObject.SetActive(true);
            playerIsOverlapping = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerIsOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerIsOverlapping = false;
        }
    }
}