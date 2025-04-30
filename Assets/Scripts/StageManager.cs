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
    /// The list of audience members who can be spawned in.
    /// </summary>
    [Tooltip("The list of audience members who can be spawned in.")]
    [SerializeField] private GameObject[] audienceMemberPrefabs;

    /// <summary>
    /// A parent object. The children of this GameObject are just Transforms to store where to spawn the audience members.
    /// </summary>
    private GameObject audienceSpawnPoints;

    /// <summary>
    /// Are the stage lights currently dimming?
    /// </summary>
    private bool lightsDimming = false;

    public static bool performanceStarted = false;

    /// <summary>
    /// Is the user currently performing?
    /// </summary>
    public bool userPerforming = false;

    /// <summary>
    /// Actions, primarily to attach audience member functions to
    /// </summary>
    public static event Action<StageManager> OnPerformanceStartEvent;
    public static event Action<StageManager> OnPerformaceEndEvent;
    public static event Action<StageManager> StartAudienceClapping;
    public static event Action<StageManager> StopAudienceClapping;

    private bool audienceClapping = false;

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

    /// <summary>
    /// An array containing all the clap-type AudienceInterruptions the StageManager has saved.
    /// </summary>
    private static AudienceInterruption[] claps;

    [SerializeField] private AudioLoudnessDetection loudnessDetector;

    /// <summary>
    /// Adjusts the sensitivity of the microphone.
    /// <para>
    /// By default microphone audio comes in super low, so it is recommended to set this value high to get any sort of feedback.
    /// </para>
    /// </summary>
    [SerializeField] private float loudnessSensitivity = 100;

    /// <summary>
    /// How loud should the audio through the microphone be for the user to be considered playing.
    /// </summary>
    [SerializeField] private float loudnessThreshold = 0.01f;

    /// <summary>
    /// How long it should be quiet before the performance is ended
    /// </summary>
    private float endPerformanceTime = 10f;

    /// <summary>
    /// Keeps track of how close we are to meeting <see cref="endPerformanceTime"/>
    /// </summary>
    [SerializeField] private float endPerformanceTimer = 0;

    /// <summary>
    /// How long should audio be above <c>loudnessThreshold</c> to conclude that there wasn't just a spike in audio when checking on whether to reset endPerformanceTimer or not.
    /// </summary>
    private float continuePerformanceTime = 2f;

    /// <summary>
    /// Timer to keep track of when the <c>continuePerformanceTime</c> passes.
    /// </summary>
    [SerializeField] private float continuePerformanceTimer = 0;

    /// <summary>
    /// How long to wait before starting to track microphone audio
    /// </summary>
    private float startPerformanceTime = 30f;

    /// <summary>
    /// Timer to keep track of when the <c>startPerformanceTime</c> passes.
    /// </summary>
    private float startPerformanceTimer = 0;

    // Used for debug purposes
    private bool automaticEndPerformance = true;

    private void Awake()
    {
        // Load all the assets in the Resources/Interruptions folder
        var interruptions = Resources.LoadAll("Interruptions/General");

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

        // Load all the assets in the Resources/Interruptions folder
        interruptions = Resources.LoadAll("Interruptions/Claps");

        // Make sure the audienceInterruptions array is the proper length
        claps = new AudienceInterruption[interruptions.Length];

        // Go through all the loaded assets and save all the AudienceInterruptions into the appropriate array
        // (They should all be AudienceInterruptions)
        for (int i = 0; i < interruptions.Length; i++)
        {
            if (interruptions[i] is AudienceInterruption)
            {
                claps[i] = (AudienceInterruption)interruptions[i];
            }
        }

        // Set audienceSpawnPoints to the first object with the "Audience Spawn Points" tag found in the scene
        // There should be only one of those
        audienceSpawnPoints = GameObject.FindGameObjectsWithTag("Audience Spawn Points")[0];

        if (audienceSpawnPoints != null)
        {
            // Get the transforms of all the audience spawn points (which are the children of audienceSpawnPoints)
            Transform[] spawnPoints = audienceSpawnPoints.GetComponentsInChildren<Transform>();

            // Loop through spawn points
            // Skip i = 0 because that is the audienceSpawnPoints object, due to how GetComponentsInChildren() work
            for (int i = 1; i < spawnPoints.Length; i++)
            {
                // Randomly (psudeo-random, of course) decide which audience member to spawn
                int audienceMemberNum = UnityEngine.Random.Range(0, audienceMemberPrefabs.Length);

                // Spawn the audience member
                GameObject temp = Instantiate(audienceMemberPrefabs[audienceMemberNum], spawnPoints[i]);
            }
        }
        else
        {
            Debug.LogError("No audience spawn point holder found. Audience members have not been spawned.");
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
        if (automaticEndPerformance)
        {
            TrackMicrophoneAudio();
        }
    }

    /// <summary>
    /// Tracks the audio from the microphone, and automatically ends the performance if there is no sound for <see cref="endPerformanceTime"/> seconds.
    /// </summary>
    private void TrackMicrophoneAudio()
    {
        if (performanceStarted)
        {
            // Get the loudness and boost it to more reasonable levels
            float loudness = loudnessDetector.GetLoudnessFromMicrophone() * loudnessSensitivity;
            
            // Only start tracking audio after a certain amount of time (so that the user can actually start playing)
            if (startPerformanceTimer < startPerformanceTime)
            {
                startPerformanceTimer += Time.deltaTime;
                return;
            }

            // If little sound is detected, user probably isn't playing
            if (loudness < loudnessThreshold)
            {
                // If the audience isn't already clapping & the endPerformanceTimer is at least 20% done, start clapping
                if (audienceClapping == false && (endPerformanceTimer / endPerformanceTime) >= 0.2f)
                {
                    audienceClapping = true;
                    StartAudienceClapping(this);
                }

                // Reset the continuePerformanceTimer
                continuePerformanceTimer = 0;

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
                // Check there wasn't just a spike in audio by running a short timer
                continuePerformanceTimer += Time.deltaTime;
                if (continuePerformanceTimer >= continuePerformanceTime)
                {
                    if (audienceClapping == true)
                    {
                        audienceClapping = false;
                        StopAudienceClapping(this);
                    }
                    endPerformanceTimer = 0;
                }
            }
        }
    }

    /// <summary>
    /// Performs all tasks regarding starting the performance, such as sending the <see cref="OnPerformanceStartEvent"/>,
    /// turning <see cref="performanceStarted"/> to true, and dimming the lights (See: <see cref="DimLights"/>.
    /// <para>
    ///     Only does the above if the user is not already performing.
    /// </para>
    /// </summary>
    public void StartPerformance()
    {
        if (performanceStarted == false)
        {
            // Trigger OnPerformanceStartEvent event
            OnPerformanceStartEvent(this);
            performanceStarted = true;

            // If we aren't dimming the lights, dim them
            if (lightsDimming == false)
            {
                StartCoroutine(DimLights());
            }
        }
    }

    /// <summary>
    /// Performs all tasks regarding ending the performance, such as triggering the <see cref="OnPerformaceEndEvent"/>,
    /// turning <see cref="performanceStarted"/> to false, and brightening the lights (See: <see cref="BrightenLights"/>).
    /// <para>
    ///     Only does the above if the user is currently performing.
    /// </para>
    /// </summary>
    public void EndPerformance()
    {
        if (performanceStarted == true)
        {
            // Trigger OnPerformanceEndEvent event
            OnPerformaceEndEvent(this);
            performanceStarted = false;

            // Turn the lights back on
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
    /// Toggles whether the performance should automatically end based on user sound levels or not.
    /// <para>
    ///     Meant for debug purposes only.
    /// </para>
    /// </summary>
    public void ToggleAutomaticEnding()
    {
        automaticEndPerformance = !automaticEndPerformance;
    }

    /// <summary>
    /// Gets all of the AudienceInterruptions the StageManager class has saved.
    /// </summary>
    /// <returns>The array of all AudienceInterruptions the StageManager class has saved.</returns>
    public static AudienceInterruption[] GetAudienceInterruptions()
    {
        return audienceInterruptions;
    }

    /// <summary>
    /// Gets all of the clap-type AudienceInterruptions the StageManager class has saved.
    /// </summary>
    /// <returns>The array of all clap-type AudienceInterruptions the StageManager class has saved.</returns>
    public static AudienceInterruption[] GetClaps()
    {
        return claps;
    }
}
