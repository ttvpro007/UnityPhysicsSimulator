using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] float speed = 0;
    [SerializeField] float jumpForce = 0;
    [SerializeField] Transform camRotation = null;
    private Rigidbody rb = null;
    private Vector3 gravity = new Vector3(0, -9.8f, 0);
    private float yaw;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Turn();
    }

    private void Turn()
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = camRotation.eulerAngles.y;
        transform.eulerAngles = angles;
    }

    private void FixedUpdate()
    {
        Move();

        if (Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Move()
    {
        float horizontalf = Input.GetAxisRaw("Horizontal");
        float verticalf = Input.GetAxisRaw("Vertical");

        if (horizontalf == 0 && verticalf == 0) return;

        Vector3 horizontal = horizontalf * transform.right;
        Vector3 vertical = verticalf * transform.forward;
        Vector3 newPosition = (horizontal + vertical) * speed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + newPosition);
    }

    private void Jump()
    {
        Vector3 newPosition = transform.up * jumpForce * Time.fixedDeltaTime;
        rb.AddForce(newPosition, ForceMode.Impulse);
    }

}
