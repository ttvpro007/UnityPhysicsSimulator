using UnityEngine;
using FiniteStateMachine;

namespace AI.States
{
    public class AttackState : IState
    {
        private AIController controller = null;
        private float attackDamage = 0;
        private float attackInterval = 0;
        private float timeSinceLastAttack = Mathf.Infinity;
        private Transform playerTransform = null;

        public AttackState(AIController stateController)
        {
            controller = stateController;
        }

        public void OnStateEnter()
        {
            if (!playerTransform) playerTransform = controller.PlayerTransform;

            attackDamage = controller.AttackDamage;
            attackInterval = controller.AttackInterval;

            controller.State = AIState.Attack;
            Debug.Log("[" + controller.gameObject.name + "] Started Attacking");
        }

        public void Update(float deltaTime)
        {
            AttackBehaviour(deltaTime);
        }

        public void OnStateExit()
        {
            Debug.Log("[" + controller.gameObject.name + "] Ended Attacking");
        }

        private void AttackBehaviour(float deltaTime)
        {
            if (TimeToAttack())
            {
                timeSinceLastAttack = 0;
                Attack(attackDamage);
            }

            controller.TimeSinceLastSawPlayer = 0;
            timeSinceLastAttack += deltaTime;
        }

        private bool TimeToAttack()
        {
            return timeSinceLastAttack >= attackInterval;
        }

        private void Attack(float damage)
        {
            Debug.Log("[" + controller.gameObject.name + "] Dealt [" + damage + "] Damage");
        }
    }
}