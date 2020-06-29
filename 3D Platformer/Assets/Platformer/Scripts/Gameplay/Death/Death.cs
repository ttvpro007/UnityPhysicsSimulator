using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    private bool playerIsDead = false;
    public bool PlayerIsDead { get { return playerIsDead; } }

    private void Start()
    {
        playerIsDead = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerIsDead = true;
        }
    }
}
