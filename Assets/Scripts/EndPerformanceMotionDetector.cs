using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPerformanceMotionDetector : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;

    private void OnTriggerEnter(Collider other)
    {
        stageManager.EndPerformance();
    }
}
