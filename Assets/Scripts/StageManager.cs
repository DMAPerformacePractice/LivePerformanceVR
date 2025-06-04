using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all matters of the stage, such as the lights and keeping track of when a performance starts and ends.
/// </summary>
public class StageManager : MonoBehaviour
{
    /// <summary>
    /// The center stage light.
    /// </summary>
    [Tooltip("The center stage light.")]
    [SerializeField] private Light centerStageLight;

    /// <summary>
    /// The general room light.
    /// </summary>
    [Tooltip("The general room light.")]
    [SerializeField] private Light roomLight;

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

    private bool lightsOn = true;

    /// <summary>
    /// An array containing all the AudienceInterruptions the StageManager has saved.
    /// </summary>
    private static AudienceInterruption[] audienceInterruptions;

    /// <summary>
    /// An array containing all the clap-type AudienceInterruptions the StageManager has saved.
    /// </summary>
    private static AudienceInterruption[] claps;

    private AudioSource beginningAnnouncementSource;

    private void Awake()
    {
        beginningAnnouncementSource = GetComponent<AudioSource>();

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

        // Load all the assets in the Resources/Interruptions/Claps folder
        interruptions = Resources.LoadAll("Interruptions/Claps");

        // Make sure the claps array is the proper length
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

            // Load all the assets in the Resources/Animator Overrides folder
            var animatorOverrideResources = Resources.LoadAll("Animator Overrides");

            // Make an array to hold all the AnimatorOverrideControllers from the assets we just loaded
            AnimatorOverrideController[] animatorOverrides = new AnimatorOverrideController[animatorOverrideResources.Length];

            // Go through all the loaded assets and save all the AnimatorOverideControllers into the appropriate array
            // (They should all be AnimatorOverrideControllers)
            for (int i = 0; i < animatorOverrideResources.Length; i++)
            {
                if (animatorOverrideResources[i] is AnimatorOverrideController)
                {
                    animatorOverrides[i] = (AnimatorOverrideController)animatorOverrideResources[i];
                }
            }

            List<GameObject> unusedAudienceMemberPrefabs = new List<GameObject>(audienceMemberPrefabs);

            // Loop through spawn points
            // Skip i = 0 because that is the audienceSpawnPoints object, due to how GetComponentsInChildren() work
            for (int i = 1; i < spawnPoints.Length && unusedAudienceMemberPrefabs.Count != 0; i++)
            {
                // Randomly (psudeo-random, of course) decide which audience member to spawn
                int audienceMemberNum = UnityEngine.Random.Range(0, unusedAudienceMemberPrefabs.Count);

                // Spawn the audience member
                GameObject temp = Instantiate(unusedAudienceMemberPrefabs[audienceMemberNum], spawnPoints[i]);

                // Randomly chose which set of animations to use for this new audience member
                int animatorOverrideNum = UnityEngine.Random.Range(0, animatorOverrides.Length + 1);

                // If the animaterOverrideNum == animatorOverrides.Length, just use the base animations
                if (animatorOverrideNum < animatorOverrides.Length)
                {
                    // Change the set of animations the audience member will use
                    temp.GetComponent<Animator>().runtimeAnimatorController = animatorOverrides[animatorOverrideNum];
                }

                unusedAudienceMemberPrefabs.RemoveAt(audienceMemberNum);
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
        StartCoroutine(StartPerformance());
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Performs all tasks regarding starting the performance, such as sending the <see cref="OnPerformanceStartEvent"/>,
    /// turning <see cref="performanceStarted"/> to true, and dimming the lights (See: <see cref="DimLights"/>.
    /// <para>
    ///     Only does the above if the user is not already performing.
    /// </para>
    /// </summary>
    private IEnumerator StartPerformance()
    {
        if (performanceStarted == false)
        {
            yield return new WaitForSeconds(0.5f);

            beginningAnnouncementSource.Play();

            yield return new WaitUntil(() =>
            {
                return !beginningAnnouncementSource.isPlaying;
            });

            yield return new WaitForSeconds(0.5f);

            StartAudienceClapping(this);

            yield return new WaitForSeconds(5);

            StopAudienceClapping(this);



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

            StartCoroutine(WaitToSwitchScene());
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
            roomLight.intensity = Mathf.Lerp(1.35f, 0.2f, t / dimTime);
            centerStageLight.intensity = Mathf.Lerp(3, 6f, t / dimTime);

            t += Time.deltaTime;

            yield return null;
        }

        lightsDimming = false;
        lightsOn = false;
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
            roomLight.intensity = Mathf.Lerp(0.2f, 1.35f, t / brightenTime);
            centerStageLight.intensity = Mathf.Lerp(6f, 3, t / brightenTime);

            t += Time.deltaTime;

            yield return null;
        }

        lightsOn = true;
    }

    private IEnumerator WaitToSwitchScene()
    {
        yield return new WaitUntil(() =>
        {
            if (lightsOn)
                return true;

            return false;
        });

        var t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime;

            if (t > 0.5f)
            {
                roomLight.intensity = Mathf.Lerp(1.35f, 0f, (t - 0.5f) / 0.5f);
                centerStageLight.intensity = Mathf.Lerp(3, 0f, (t - 0.5f) / 0.5f);
            }

            yield return null;
        }

        if (audienceSpawnPoints != null)
        {
            // Get the transforms of all the children of audienceSpawnPoints (this will be spawn points, as well as the audience members themselves)
            Transform[] spawnPoints = audienceSpawnPoints.GetComponentsInChildren<Transform>();

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].gameObject.tag == "Audience Member")
                {
                    Destroy(spawnPoints[i].gameObject);
                }
            }
        }

        SceneManager.LoadScene(0);
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
