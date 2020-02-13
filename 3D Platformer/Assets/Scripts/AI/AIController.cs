using UnityEngine;
using FiniteStateMachine;
using AI.PatrolPath;
using AI.States;
using PhysicsSimulation;
using PhysicsSimulation.Helper;
using Core;

namespace AI
{
    public class AIController : MonoBehaviour
    {
        // private
        // editable in inspector
        // constant run-time value
        [SerializeField] private float halfFOVAngle = 60;
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
        private bool isPlayerInFOV = false;

        // private
        // constant run-time value
        private FSM controller = new FSM();
        private PlatformerPhysicsSim ps = null;
        private Rigidbody rb = null;
        private RaycastHitInfo hitInfo = null;
        private DamageDealer damageDealer = null;

        // public
        // readonly
        public bool IsGrounded { get { return ps.IsGrounded; } }
        public bool CanSeePlayer { get { return isPlayerInFOV; } }
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
        public RaycastHitInfo HitInfo { get { return hitInfo; } }
        public DamageDealer DamageDealer { get { return damageDealer; } }

        // public
        // read-write
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
            //DistanceToPlayer = GetHorizontalDistanceToTaget(transform.position, playerTransform.position);
            DistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerInFOV = IsTargetInFOV(transform.forward, (playerTransform.position - transform.position).normalized, halfFOVAngle);
            TimeSinceLastSawPlayer += Time.deltaTime;

            controller.Update(Time.deltaTime);
        }

        private void Initiate()
        {
            if (!playerTransform) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            if (!movement) movement = GetComponent<AIMovement>();
            if (!ps) ps = GetComponent<PlatformerPhysicsSim>();
            if (!rb) rb = GetComponent<Rigidbody>();
            if (!hitInfo) hitInfo = GetComponentInChildren<RaycastHitInfo>();
            damageDealer = GetComponent<DamageDealer>();

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
            return // canSeePlayer &&
                DistanceToPlayer <= chaseDistance
                && ps.IsGrounded;
        }
        private bool ChaseToAttackCondition()
        {
            return // canSeePlayer &&
                DistanceToPlayer <= attackRange
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
            return // canSeePlayer &&
                DistanceToPlayer > attackRange
                && ps.IsGrounded;
        }
        private bool InvestigateToChaseCondition()
        {
            return // canSeePlayer &&
                DistanceToPlayer <= chaseDistance
                && ps.IsGrounded;
        }
        private bool InvestigateToPatrolCondition()
        {
            return TimeSinceLastSawPlayer >= investigationTime
                && ps.IsGrounded;
        }
        #endregion

        public static bool IsTargetInFOV(Vector3 forwardDirection, Vector3 toTargetDirection, float halfAngle)
        {
            float fov = Mathf.Cos(halfAngle * Mathf.PI / 180);
            float dot = Vector3.Dot(toTargetDirection, forwardDirection);
            return dot > fov;
        }

        public static float GetHorizontalDistanceToTaget(Vector3 fromPosition, Vector3 targetPosition)
        {
            float distance = Vector3.ProjectOnPlane(targetPosition - fromPosition, Vector3.up).magnitude;
            return distance;
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