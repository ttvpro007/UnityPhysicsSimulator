using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer cell1 = null;
    [SerializeField] private SpriteRenderer cell2 = null;
    [SerializeField] private SpriteRenderer cell3 = null;
    [SerializeField] private SpriteRenderer cell4 = null;
    [SerializeField] private SpriteRenderer cell5 = null;

    public void SetCell(int cellNumber, bool enable)
    {
        switch (cellNumber)
        {
            case 1:
                cell1.enabled = enable;
                break;
            case 2:
                cell2.enabled = enable;
                break;
            case 3:
                cell3.enabled = enable;
                break;
            case 4:
                cell4.enabled = enable;
                break;
            case 5:
                cell5.enabled = enable;
                break;
            default:
                break;
        }
    }
}
