using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPerformanceMotionDetector : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;

    private BoxCollider collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        stageManager.EndPerformance();
    }
}
