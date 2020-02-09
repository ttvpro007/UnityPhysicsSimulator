using UnityEngine;
using FiniteStateMachine;
using AI.PatrolPath;
using AI.States;
using PhysicsSimulation;

namespace AI
{
    public class AIController : MonoBehaviour
    {
        // private
        // editable in inspector
        // constant run-time value
        [SerializeField] private float waypointDwellTime = 0;
        [SerializeField] private float waypointTolerance = 1;
        [SerializeField] private float verticalDistanceToJump = 5;
        [SerializeField] private float chaseDistance = 5;
        [SerializeField] private float chaseStoppingDistance = 3;
        [SerializeField] private float attackDamage = 10;
        [SerializeField] private float attackRange = 25;
        [SerializeField] private float attackInterval = 2;
        [SerializeField] private float investigationTime = 5;
        [SerializeField] private Transform playerTransform = null;
        [SerializeField] private PatrolPathGraph patrolPath = null;
        [SerializeField] private AIMovement movement = null;

        // private
        // constant run-time value
        private FSM controller = new FSM();
        private PlatformerPhysicsSim ps = null;
        private float minProjectionScalarOnXZPlane = 0;
        private float minProjectionScalarOnYZPlane = 0;

        // public
        // readonly
        // constant run-time value
        public bool IsGrounded { get { return ps.IsGrounded; } }
        public float WaypointDwellTime { get { return waypointDwellTime; } }
        public float WaypointTolerance { get { return waypointTolerance; } }
        public float VerticalDistanceToJump { get { return verticalDistanceToJump; } }
        public float ChaseDistance { get { return chaseDistance; } }
        public float ChaseStoppingDistance { get { return chaseStoppingDistance; } }
        public float AttackDamage { get { return attackDamage; } }
        public float AttackRange { get { return attackRange; } }
        public float AttackInterval { get { return attackInterval; } }
        public float InvestigationTime { get { return investigationTime; } }
        public Transform PlayerTransform { get { return playerTransform; } }
        public PatrolPathGraph PatrolPath { get { return patrolPath; } }
        public AIMovement Movement { get { return movement; } }

        // public
        // read-write
        // changable run-time value
        public AIState State { get; set; } = AIState.Patrol;
        public float DistanceToPlayer { get; set; } = 0;
        public float TimeSinceLastSawPlayer { get; set; } = Mathf.Infinity;

        public AIState DebugState = AIState.Patrol;

        private void Start()
        {
            Initiate();
        }

        private void Update()
        {
            DebugState = State;
            //DistanceToPlayer = GetHorizontalDistanceToTaget(transform, playerTransform);
            DistanceToPlayer = GetHorizontalDistanceToTaget(transform.position, playerTransform.position);
            //DistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            TimeSinceLastSawPlayer += Time.deltaTime;

            controller.Update(Time.deltaTime);
        }

        private void Initiate()
        {
            if (!playerTransform) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            if (!movement) movement = GetComponent<AIMovement>();
            if (!ps) ps = transform.GetComponent<PlatformerPhysicsSim>();

            // attack range to be <= to chase distance
            attackRange = Mathf.Min(attackRange, chaseDistance);
            InitiateAIBrain();
        }

        private void InitiateAIBrain()
        {
            controller.AddState("Patrol", new PatrolState(this));
            controller.AddState("Chase", new ChaseState(this));
            controller.AddState("Attack", new AttackState(this));
            controller.AddState("Investigate", new InvestigateState(this));

            controller.AddTransition("Patrol", "Chase", PatrolToChaseCondition);
            controller.AddTransition("Chase", "Attack", ChaseToAttackCondition);
            controller.AddTransition("Chase", "Investigate", ChaseToInvestigateCondition);
            controller.AddTransition("Attack", "Chase", AttackToChaseCondition);
            controller.AddTransition("Investigate", "Chase", InvestigateToChaseCondition);
            controller.AddTransition("Investigate", "Patrol", InvestigateToPatrolCondition);
        }

        #region TRANSITION CONDITIONS
        private bool PatrolToChaseCondition()
        {
            return DistanceToPlayer <= chaseDistance
                && ps.IsGrounded;
        }
        private bool ChaseToAttackCondition()
        {
            return DistanceToPlayer <= attackRange
                && DistanceToPlayer <= chaseDistance
                && ps.IsGrounded;
        }
        private bool ChaseToInvestigateCondition()
        {
            return DistanceToPlayer > chaseDistance
                && ps.IsGrounded;
        }
        private bool AttackToChaseCondition()
        {
            return DistanceToPlayer > attackRange
                && ps.IsGrounded;
        }
        private bool InvestigateToChaseCondition()
        {
            return DistanceToPlayer <= chaseDistance
                && ps.IsGrounded;
        }
        private bool InvestigateToPatrolCondition()
        {
            return TimeSinceLastSawPlayer >= investigationTime
                && ps.IsGrounded;
        }
        #endregion

        public static float GetHorizontalDistanceToTaget(Vector3 fromPosition, Vector3 targetPosition)
        {
            float distance = Vector3.ProjectOnPlane(targetPosition - fromPosition, Vector3.up).magnitude;
            return distance;
        }

        public static float GetHorizontalDistanceToTaget(Transform fromTransform, Transform targetTransform)
        {
            float distance = Vector3.Dot(targetTransform.position - fromTransform.position, fromTransform.TransformDirection(Vector3.forward));
            return Mathf.Abs(distance);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}