using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceIndicatorTextUpdator : MonoBehaviour
{
    [SerializeField] Text distanceText = null;

    private void Start()
    {
        distanceText.fontSize = 40;
        distanceText.text = string.Format("{0:0.0} m", transform.position.z);
    }
}
