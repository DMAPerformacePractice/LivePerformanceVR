using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Light stageLight;

    private bool lightsDimming = false;

    public static bool userPerforming = false;
    public static event Action<StageManager> OnPerformanceStartEvent;
    public static event Action<StageManager> OnPerformaceEndEvent;

    [SerializeField] private float dimTime = 2;
    [SerializeField] private float brightenTime = 2;

    [SerializeField] static private AudienceInterruption[] audienceInterruptions;

    [SerializeField] private AudioLoudnessDetection loudnessDetector;
    private float loudnessSensitivity = 100;
    private float loudnessThreshold = 0.2f;
    // How long it should be quiet before the performance is ended
    private float endPerformanceTime = 10f;
    private float endPerformanceTimer = 0;

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
        loudnessDetector = GetComponent<AudioLoudnessDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartPerformance();
        }

        if (userPerforming)
        {
            float loudness = loudnessDetector.GetLoudnessFromMicrophone() * loudnessSensitivity;

            if (loudness < loudnessThreshold)
            {
                endPerformanceTimer += Time.deltaTime;
                if (endPerformanceTimer >= endPerformanceTime)
                {
                    EndPerformance();
                }
            }
            else
            {
                endPerformanceTimer = 0;
            }
        }
    }

    public void StartPerformance()
    {
        if (userPerforming == false)
        {
            OnPerformanceStartEvent(this);
            userPerforming = true;
            if (lightsDimming == false)
            {
                StartCoroutine(DimLights());
            }
        }
    }

    public void EndPerformance()
    {
        if (userPerforming == true)
        {
            OnPerformaceEndEvent(this);
            userPerforming = false;
            StartCoroutine(BrightenLights());
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

    private IEnumerator BrightenLights()
    {
        var t = 0f;

        while (t < brightenTime)
        {
            stageLight.intensity = Mathf.Lerp(0.5f, 1, t / brightenTime);

            t += Time.deltaTime;

            yield return null;
        }
    }

    public static AudienceInterruption[] GetAudienceInterruptions()
    {
        return audienceInterruptions;
    }
}
