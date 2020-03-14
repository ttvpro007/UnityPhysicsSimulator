using UnityEngine;
using System.Collections;

namespace AI.PathFinding
{
    public class CustomNavMeshAgent : MonoBehaviour
    {
        private ArrayList pathArray;
        public ArrayList Path { get { return pathArray; } }
        public Node startNode { get; set; } = null;
        public Node targetNode { get; set; } = null;

        //[SerializeField] private Transform targetTransform;
        [SerializeField] private float pathUpdateInterval = 1.0f;
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float stoppingDistance = 2.0f;
        [SerializeField] private float turnSmoothingFactor = 12.5f;
        [SerializeField] private float agentHeight = 0.5f;
        [SerializeField] private float groundHeight = 0f;

        private bool isPathInitialized = false;
        private bool hasReset = false;

        //private float elapsedTime = 0.0f;
        private GridManager gridManager;

        private void Start()
        {
            gridManager = FindObjectOfType<GridManager>();

            //Calculate the path using our AStart code.
            pathArray = new ArrayList();
            //FindPath();
        }

        private ArrayList FindPath(Vector3 destination)
        {
            Node Temp = startNode;
            startNode = new Node(gridManager.GetGridCellCenter(gridManager.GetGridIndex(transform.position)));
            targetNode = new Node(gridManager.GetGridCellCenter(gridManager.GetGridIndex(destination)));

            if (!isPathInitialized)
            {
                pathArray = AStar.FindPath(startNode, targetNode);
                isPathInitialized = true;

                return pathArray;
            }

            ArrayList path = pathArray;

            if ((Temp != null && Temp.position != startNode.position) || path == null)
            {
                Debug.Log("Finding Path...");
                path = AStar.FindPath(startNode, targetNode);
            }

            return path;
        }

        public void MoveTo(Vector3 destination)
        {
            if (HasReachedDestination(destination))
            {
                if (!hasReset)
                    Reset();

                return;
            }
            else
            {
                hasReset = false;
                pathArray = FindPath(destination);

                Node targetNode = GetTargetNode();

                if (targetNode == null) return;

                Vector3 direction = GetMoveDirection(targetNode.position);
                transform.position += speed * Time.deltaTime * direction;
                transform.LookAt
                (
                    Vector3.Slerp(transform.position + transform.forward,
                                    transform.position + direction,
                                    turnSmoothingFactor * Time.deltaTime)
                );
            }
        }

        private Vector3 GetMoveDirection(Vector3 targetPosition)
        {
            targetPosition.y = agentHeight + groundHeight;
            Vector3 direction = (targetPosition - transform.position).normalized;
            return direction;
        }

        private Node GetTargetNode()
        {
            Node targetNode = null;

            if (pathArray != null && pathArray.Count > 0)
            {
                targetNode = pathArray.Count == 1 ? (Node)pathArray[0] : (Node)pathArray[1];
            }

            return targetNode;
        }

        private bool HasReachedDestination(Vector3 position)
        {
            return Vector3.Distance(transform.position, position) < stoppingDistance;
        }

        private bool HasReachedDestination(Vector2 position)
        {
            Vector2 a = new Vector2(transform.position.x, transform.position.z);
            return Vector2.Distance(a, position) < stoppingDistance;
        }

        public void SetPathUpdateInterval(float pathUpdateInterval)
        {
            this.pathUpdateInterval = pathUpdateInterval;
        }

        private void Reset()
        {
            pathArray.Clear();
            startNode = null;
            targetNode = null;
            hasReset = true;
            isPathInitialized = false;
        }

        private void OnDrawGizmos()
        {
            if (pathArray == null)
            {
                return;
            }

            if (pathArray.Count > 0)
            {
                int index = 1;
                foreach (Node node in pathArray)
                {
                    if (index < pathArray.Count)
                    {
                        Node nextNode = (Node)pathArray[index];
                        Debug.DrawLine(node.position, nextNode.position, Color.green);
                        index++;
                    }
                };
            }
        }
    }
}