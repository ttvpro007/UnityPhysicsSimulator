using UnityEngine;
using FiniteStateMachine;
using AI.PatrolPath;

namespace AI.States
{
    public class PatrolState : IState
    {
        private AIController controller = null;
        private AIMovement movement = null;
        private PatrolPathGraph patrolPath = null;
        public Vector3 guardPosition = Vector3.zero;
        private float waypointTolerance = 0;
        private float waypointDwellTime = 0;
        private float chaseDistance = 0;
        private float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        private int currentWaypointIndex = 0;

        public PatrolState(AIController stateController)
        {
            controller = stateController;
        }

        public void OnStateEnter()
        {
            if (!movement) movement = controller.Movement;
            if (!patrolPath) patrolPath = controller.PatrolPath;

            guardPosition = controller.transform.position;
            waypointTolerance = controller.WaypointTolerance;
            waypointDwellTime = controller.WaypointDwellTime;
            chaseDistance = controller.ChaseDistance;

            controller.State = AIState.Patrol;
            Debug.Log("[" + controller.gameObject.name + "] Started Patrolling");
        }

        public void Update(float deltaTime)
        {
            PatrolBehaviour(deltaTime);
        }

        public void OnStateExit()
        {
            Debug.Log("[" + controller.gameObject.name + "] Ended Patrolling");
        }

        private void PatrolBehaviour(float deltaTime)
        {
            Vector3 nextPosition = guardPosition;

            if (controller.IsGrounded)
            {
                if (patrolPath)
                {
                    nextPosition = GetCurrentWaypoint();

                    if (AtWaypoint())
                    {
                        timeSinceArrivedAtWaypoint = 0;
                        CycleWaypoint();
                    }
                }

                if (timeSinceArrivedAtWaypoint >= waypointDwellTime)
                {
                    movement.MoveTo(nextPosition);
                }
            }


            timeSinceArrivedAtWaypoint += deltaTime;
        }

        private bool AtWaypoint()
        {
            return Vector3.Distance(controller.transform.position, GetCurrentWaypoint()) <= waypointTolerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextWaypointIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath ? patrolPath.GetWaypoint(currentWaypointIndex) : Vector3.zero;
        }
    }
}