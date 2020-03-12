using UnityEngine;
using FiniteStateMachine;

namespace AI.States
{
    public class InvestigateState : IState
    {
        private AIController controller = null;

        public InvestigateState(AIController stateController)
        {
            controller = stateController;
        }

        public void OnStateEnter()
        {
            controller.State = AIState.Investigate;
            Debug.Log("[" + controller.gameObject.name + "] Started Investigating");
        }

        public void Update(float deltaTime)
        {
            InvestigateBehaviour(deltaTime);
        }

        private void InvestigateBehaviour(float deltaTime)
        {
            // TODO do something with this
        }

        public void OnStateExit()
        {
            Debug.Log("[" + controller.gameObject.name + "] Ended Investigating");
        }
    }
}