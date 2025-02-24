using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all matters of the stage, such as the lights and keeping track of when a performance starts and ends.
/// </summary>
public class StageManager : MonoBehaviour
{
    /// <summary>
    /// The primary stage light, may be swapped to an array later on when more lights are added.
    /// </summary>
    [Tooltip("The primary stage light.")]
    [SerializeField] private Light stageLight;

    /// <summary>
    /// Are the stage lights currently dimming?
    /// </summary>
    private bool lightsDimming = false;

    /// <summary>
    /// Is the user currently performing?
    /// </summary>
    public static bool userPerforming = false;
    public static event Action<StageManager> OnPerformanceStartEvent;
    public static event Action<StageManager> OnPerformaceEndEvent;

    /// <summary>
    /// How long it should take for the lights to dim at the start of the performance.
    /// </summary>
    [Tooltip("How long it should take for the lights to dim at the start of the performance.")]
    [SerializeField] private float dimTime = 2;
    /// <summary>
    /// How long it should take for the lights to brighten at the end of the performance.
    /// </summary>
    [Tooltip("How long it should take for the lights to brighten at the end of the performance.")]
    [SerializeField] private float brightenTime = 2;

    /// <summary>
    /// An array containing all the AudienceInterruptions the StageManager has saved.
    /// </summary>
    private static AudienceInterruption[] audienceInterruptions;

    [SerializeField] private AudioLoudnessDetection loudnessDetector;
    /// <summary>
    /// Adjusts the sensitivity of the microphone.
    /// <para>
    /// By default microphone audio comes in super low, so it is recommended to set this value high to get any sort of feedback.
    /// </para>
    /// </summary>
    private float loudnessSensitivity = 100;
    private float loudnessThreshold = 0.2f;

    /// <summary>
    /// How long it should be quiet before the performance is ended
    /// </summary>
    private float endPerformanceTime = 10f;
    /// <summary>
    /// Keeps track of how close we are to meeting <see cref="endPerformanceTime"/>
    /// </summary>
    private float endPerformanceTimer = 0;

    private void Awake()
    {
        // Load all the assets in the Resources/Interruptions folder
        var interruptions = Resources.LoadAll("Interruptions");

        // Make sure the audienceInterruptions array is the proper length
        audienceInterruptions = new AudienceInterruption[interruptions.Length];

        // Go through all the loaded assets and save all the AudienceInterruptions into the appropriate array
        // (They should all be AudienceInterruptions)
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
        // Intialize loudnessDetector
        loudnessDetector = GetComponent<AudioLoudnessDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        TrackMicrophoneAudio();
    }

    /// <summary>
    /// Tracks the audio from the microphone, and automatically ends the performance if there is no sound for <see cref="endPerformanceTime"/> seconds.
    /// </summary>
    private void TrackMicrophoneAudio()
    {
        if (userPerforming)
        {
            // Get the loudness and boost it to more reasonable levels
            float loudness = loudnessDetector.GetLoudnessFromMicrophone() * loudnessSensitivity;

            // If little sound is detected, user probably isn't playing
            if (loudness < loudnessThreshold)
            {
                // If they don't make sound for an extended period of time, they definitely aren't playing, so end the performance
                endPerformanceTimer += Time.deltaTime;
                if (endPerformanceTimer >= endPerformanceTime)
                {
                    EndPerformance();
                }
            }
            // If we detect sound again, they probably just stopped playing for a second or two, so reset timer
            else
            {
                endPerformanceTimer = 0;
            }
        }
    }

    /// <summary>
    /// Performs all tasks regarding starting the performance, such as sending the <see cref="OnPerformanceStartEvent"/>,
    /// turning <see cref="userPerforming"/> to true, and dimming the lights (See: <see cref="DimLights"/>.
    /// <para>
    ///     Only does the above if the user is not already performing.
    /// </para>
    /// </summary>
    public void StartPerformance()
    {
        if (userPerforming == false)
        {
            // Trigger OnPerformanceStartEvent event
            OnPerformanceStartEvent(this);
            userPerforming = true;
            if (lightsDimming == false)
            {
                StartCoroutine(DimLights());
            }
        }
    }

    /// <summary>
    /// Performs all tasks regarding ending the performance, such as triggering the <see cref="OnPerformaceEndEvent"/>,
    /// turning <see cref="userPerforming"/> to false, and brightening the lights (See: <see cref="BrightenLights"/>).
    /// <para>
    ///     Only does the above if the user is currently performing.
    /// </para>
    /// </summary>
    public void EndPerformance()
    {
        if (userPerforming == true)
        {
            // Trigger OnPerformanceEndEvent event
            OnPerformaceEndEvent(this);
            userPerforming = false;
            StartCoroutine(BrightenLights());
        }
    }

    /// <summary>
    /// Over <see cref="dimTime"/> seconds, reduce the intensity of the lights from 1 to 0.5f.
    /// </summary>
    private IEnumerator DimLights()
    {
        lightsDimming = true;

        // Keeps track of how far along the process of dimming the lights is
        var t = 0f;

        // Over the course of dimTime seconds, reduce the intensity of the lights from 1 to 0.5f
        while (t < dimTime) {
            stageLight.intensity = Mathf.Lerp(1, 0.5f, t / dimTime);

            t += Time.deltaTime;

            yield return null;
        }

        lightsDimming = false;
    }

    /// <summary>
    /// Over <see cref="brightenTime"/> seconds, increase the intensity of the lights from 0.5f to 1.
    /// </summary>
    private IEnumerator BrightenLights()
    {
        // Keeps track of how far along the process of brightening the lights is
        var t = 0f;

        // Over the course of brightenTime seconds, increase the intensity of the lights from 0.5f to 1
        while (t < brightenTime)
        {
            stageLight.intensity = Mathf.Lerp(0.5f, 1, t / brightenTime);

            t += Time.deltaTime;

            yield return null;
        }
    }

    /// <summary>
    /// Gets all of the AudienceInterruptions the StageManager class has saved.
    /// </summary>
    /// <returns>The array of all AudienceInterruptions the StageManager class has saved.</returns>
    public static AudienceInterruption[] GetAudienceInterruptions()
    {
        return audienceInterruptions;
    }
}
