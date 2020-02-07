using UnityEngine;

namespace AI
{
    public class AILook : MonoBehaviour
    {
        [SerializeField] private float rotationSmoothFactor = 0;
        [SerializeField] private Transform targetTransform = null;
        [SerializeField] private AIController controller = null;
        [SerializeField] private AIMovement movement = null;

        private void Start()
        {
            if (!targetTransform) targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
            if (!controller) controller = GetComponentInParent<AIController>();
            if (!movement) movement = controller.GetComponent<AIMovement>();
        }

        private void Update()
        {
            switch (controller.State)
            {
                case AIState.Chase:
                case AIState.Attack:
                    transform.LookAt(targetTransform);
                    break;
                case AIState.Patrol:
                case AIState.Investigate:
                    transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + movement.Direction, rotationSmoothFactor * Time.deltaTime));
                    break;
                default:
                    break;
            }
        }
    }
}