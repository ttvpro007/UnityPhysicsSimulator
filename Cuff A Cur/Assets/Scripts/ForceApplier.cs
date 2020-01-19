using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceApplier : MonoBehaviour
{
    // event delegate
    public delegate void OnStartedEvent();
    public static event OnStartedEvent OnStarted;
    public delegate void OnStoppedEvent();
    public static event OnStoppedEvent OnStopped;

    // static variables
    private static ForceApplier instance = null;
    public static ForceApplier Instance { get { return instance; } }

    // public field
    public float Velocity { get { return velocity; } }

    // private variables
    private Rigidbody rb = null;
    private float mass = 0;
    private static float forceMagnitude = 0;
    private float acceleration = 0;
    private float velocity = 0;
    private float deltaTime = 0;
    private float gravityMagnitude = -9.8f;
    private float friction = 0;
    private float bounciness = 0;
    //private Vector3 normal = new Vector3(0, 0, 0);
    //private Vector3 gravity = new Vector3(0, -9.8f, 0);
    private static Vector3 moveDirection = Vector3.forward; // set as moving backward when got punched

    private static bool hasStarted = false;
    private static bool hasStopped = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InputRegister.OnReset += Reset;
        Initiate();
    }

    private void OnDisable()
    {
        InputRegister.OnReset -= Reset;
    }

    private void Initiate()
    {
        rb = GetComponent<Rigidbody>();

        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("There was no rigidbody component. Rigidbody component has been added.");
        }

        mass = GameManager.Instance.Mass;

        if (mass <= 0)
        {
            mass = 1;
            Debug.Log("Mass value <= 0. Mass is set to 1.");
        }

        deltaTime = Time.fixedDeltaTime;
        friction = GameManager.Instance.Friction;
        bounciness = GameManager.Instance.Bounciness;
    }

    private void FixedUpdate()
    {
        if (forceMagnitude <= 0)
        {
            OnStopped.Invoke();
            hasStopped = true;
            return;
        }

        if (!hasStarted)
        {
            OnStarted.Invoke();
            hasStarted = true;
        }

        Move();
        UpdateForce();
    }

    private void UpdateForce()
    {
        // friction
        forceMagnitude += gravityMagnitude * mass * friction;
        // clamp to 0
        forceMagnitude = Mathf.Max(0, forceMagnitude);
    }

    private void Move()
    {

#if UNITY_EDITOR
        // in case value changed in GameManager
        mass = Mathf.Max(1, GameManager.Instance.Mass);
        friction = GameManager.Instance.Friction;
        bounciness = GameManager.Instance.Bounciness;
#endif

        // a = F / m
        acceleration = forceMagnitude / mass;

        // v = a * t
        velocity = acceleration * deltaTime;

        // accelTime = 0.5 * a * t^2
        float accelTime = 0.5f * acceleration * deltaTime * deltaTime;

        // veloTime = v * t
        float veloTime = velocity * deltaTime;

        // d = v * t + 0.5 a * t^2 = veloTime + accelTime
        Vector3 newPos = (veloTime + accelTime) * moveDirection;

        // move!!!
        rb.MovePosition(transform.position + newPos);
    }

    public static void GetForceFromInputRegister(float value)
    {
        forceMagnitude = value;
    }

    public static void Reset()
    {
        forceMagnitude = 0;
        moveDirection = Vector3.forward;
        hasStarted = false;
        hasStopped = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Wall") return;
        forceMagnitude *= bounciness;
        moveDirection *= -1;
    }
}
