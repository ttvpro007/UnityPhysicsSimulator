using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] float maxForce = 0;
    [Range(0f, 1f)]
    [SerializeField] float friction = 0;
    [Range(0f, 1f)]
    [SerializeField] float punchObjectBounciness = 0;
    [SerializeField] float punchObjectMass = 1;

    // public fields
    public float MaxForce { get { return maxForce; } }
    public float Friction { get { return friction; } }
    public float Bounciness { get { return punchObjectBounciness; } }
    public float Mass { get { return punchObjectMass; } }

    // static variables
    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }
    
    void Awake()
    {
        if (instance && instance == this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
