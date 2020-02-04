using UnityEngine;
using ViTiet.DataStructure;
using ViTiet.DataStructure.Binary;

public class TestBinaryTree : MonoBehaviour
{
    private BinaryTree<Data> myBTree = new BinaryTree<Data>();

    // Start is called before the first frame update
    void Start()
    {
        myBTree.Add(new Data(2.5f));

        for (int i = 0; i < 10; i++)
        {
            Data data = new Data(Random.Range(0f, 5f));
            Debug.Log(data.Value + " i: " + i);
            myBTree.Add(data);
        }

        foreach (var n in myBTree.Nodes)
        {
            Debug.Log("[Parent ID " + (n.Key.Parent != null ? n.Key.Parent.ID : "NULL") + "] ");
            Debug.Log("[ID " + n.Key.ID + "] " +
                "[Node " + n.Value.Value + "] " +
                "[Level " + n.Value.Key + "] " +
                "[Value " + n.Key.Data.Value + "]");
        }

        return;
    }
}
