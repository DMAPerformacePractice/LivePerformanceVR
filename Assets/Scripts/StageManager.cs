using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Light stageLight;

    private bool lightsDimming = false;

    public static bool userPerforming = false;
    public static event Action<StageManager> OnPerformaceStartEvent;

    [SerializeField] private float dimTime = 2;

    [SerializeField] static private AudienceInterruption[] audienceInterruptions;

    private void Awake()
    {
        var interruptions = Resources.LoadAll("Interruptions");

        audienceInterruptions = new AudienceInterruption[interruptions.Length];

        for (int i = 0; i < interruptions.Length; i++)
        {
            if (interruptions[i] is AudienceInterruption)
            {
                audienceInterruptions[i] = (AudienceInterruption) interruptions[i];
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartPerformance();
        }
    }

    public void StartPerformance()
    {
        if (userPerforming == false)
        {
            OnPerformaceStartEvent(this);
            if (lightsDimming == false)
            {
                StartCoroutine(DimLights());
            }
        }
    }

    private IEnumerator DimLights()
    {
        lightsDimming = true;

        var t = 0f;

        while (t < dimTime) {
            stageLight.intensity = Mathf.Lerp(1, 0.5f, t / dimTime);

            t += Time.deltaTime;

            yield return null;
        }

        lightsDimming = false;
    }

    public static AudienceInterruption[] GetAudienceInterruptions()
    {
        return audienceInterruptions;
    }
}
