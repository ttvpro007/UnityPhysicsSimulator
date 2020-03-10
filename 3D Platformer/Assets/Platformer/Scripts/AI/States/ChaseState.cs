using UnityEngine;
using FiniteStateMachine;

namespace AI.States
{
    public class ChaseState : IState
    {
        private AIController controller = null;
        private float verticalDistanceToJump = 0;
        private int jumpCount = 0;
        private Transform playerTransform = null;
        private AIMovement movement = null;

        public ChaseState(AIController stateController)
        {
            controller = stateController;
        }

        public void OnStateEnter()
        {
            if (!playerTransform) playerTransform = controller.PlayerTransform;
            if (!movement) movement = controller.Movement;

            verticalDistanceToJump = controller.VerticalDistanceToJump;

            controller.State = AIState.Chase;
            Debug.Log("[" + controller.gameObject.name + "] Started Chasing");
        }

        public void Update(float deltaTime)
        {
            ChaseBehaviour(deltaTime);
        }

        public void OnStateExit()
        {
            Debug.Log("[" + controller.gameObject.name + "] Ended Chasing");
        }

        private void ChaseBehaviour(float deltaTime)
        {
            if (controller.IsGrounded)
            {
                jumpCount = 0;

                movement.MoveTo(playerTransform.position);

                if (CanJump() && ShouldJump())
                {
                    movement.Jump();
                }
            }

            controller.TimeSinceLastSawPlayer = 0;
        }

        private bool CanJump()
        {
            return jumpCount == 0;
        }

        private bool ShouldJump()
        {
            float verticalDisToPlayer = playerTransform.position.y - controller.transform.position.y;
            return verticalDisToPlayer >= verticalDistanceToJump;
        }
    }
}