using UnityEngine;

namespace Player.Control
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] float speed = 0;
        [SerializeField] float gravity = -9.8f;
        [SerializeField] float jumpHeight = 3;
        [SerializeField] CharacterController controller = null;
        [SerializeField] Transform groundCheck = null;
        [SerializeField] float groundDistance = 0;
        [SerializeField] LayerMask groundLayer = new LayerMask();

        private Vector3 velocity = Vector3.zero;
        private bool isGrounded = false;

        private void Update()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector3 moveDirection = transform.right * x + transform.forward * y;
            controller.Move(moveDirection * speed * Time.deltaTime);

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
            if (isGrounded && velocity.y < 0)
                velocity.y = -2;
            if (Input.GetKeyDown(KeyCode.Space))
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}