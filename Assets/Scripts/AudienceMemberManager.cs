using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all the task involved in running an audience member, primarily randomly playing an interruption.
/// </summary>
public class AudienceMemberManager : MonoBehaviour
{
    // All the saved AudienceInterruptions
    private AudienceInterruption[] audienceInterruptions;

    private AudienceInterruption[] claps;

    /// <summary>
    /// The AudioSource from which to play interruption sounds
    /// </summary>
    [Tooltip("The AudioSource from which to play interruption sounds")]
    private AudioSource audioSource;

    public bool inPerformance = false;

    /// <summary>
    /// How many seconds between each audience interruption.
    /// </summary>
    [Tooltip("How many seconds between each audience interruption.")]
    [SerializeField] private float interruptionDelayTime = 10;

    /// <summary>
    /// Add randomness to interruption time. Interruptions will happen randomly based on a number of seconds outlined by interval [interruptionDelayTime - 2, interruptionDelayTime + 2].
    /// </summary>
    [Tooltip("Add randomness to interruption time. Interruptions will happen randomly based on a number of seconds outlined by the interval [interruptionDelayTime - 2, interruptionDelayTime + 2].")]
    [SerializeField] private float interruptionVariability = 2;

    private float clapTime = 5;
    private Coroutine clappingCoroutine;

    private bool clapIncreasing = false;
    private bool clapDecreasing = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Variables
        audienceInterruptions = StageManager.GetAudienceInterruptions();
        claps = StageManager.GetClaps();
        audioSource = GetComponent<AudioSource>();
        // Add Methods to StageManager Events
        StageManager.OnPerformanceStartEvent += StartAudienceMember;
        StageManager.OnPerformaceEndEvent += StopAudienceMember;
        StageManager.StartAudienceClapping += StartClapping;
        StageManager.StopAudienceClapping += StopClapping;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Start running the AudienceMember.
    /// </summary>
    /// <param name="stageManager"></param>
    private void StartAudienceMember(StageManager stageManager)
    {
        inPerformance = true;
        StartCoroutine(RunAudienceMember());
    }

    /// <summary>
    /// Stop running the AudienceMember.
    /// </summary>
    /// <param name="stageManager"></param>
    private void StopAudienceMember(StageManager stageManager)
    {
        inPerformance = false;
        StopCoroutine(RunAudienceMember());
    }

    /// <summary>
    /// Runs the AudienceMember. Will automatically play a random interruption every <see cref="interruptionDelayTime"/> with a variablity range of plus-minus <see cref="interruptionVariability"/>.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunAudienceMember()
    {
        while (inPerformance)
        {
            // Wait the appropriate amount of time
            yield return new WaitForSeconds(interruptionDelayTime + Random.Range(-interruptionVariability, interruptionVariability));
            // Choose and play a random interruption
            float rand = Random.value;
            int interruptionNumber = Mathf.CeilToInt(rand * audienceInterruptions.Length);
            audioSource.PlayOneShot(audienceInterruptions[interruptionNumber - 1].getNoise());
        }
    }

    private void StartClapping(StageManager stageManager)
    {
        if (clappingCoroutine != null)
        {
            StopAllCoroutines();
            float rand = Random.value;
            int clapNumber = Mathf.CeilToInt(rand * claps.Length);
            audioSource.PlayOneShot(claps[clapNumber - 1].getNoise());
            clappingCoroutine = StartCoroutine(ManageClapping());
        }
        clapIncreasing = true;
        clapDecreasing = false;
    }

    private IEnumerator ManageClapping()
    {
        var t = 0f;

        while (t < clapTime && t >= 0)
        {
            yield return null;

            audioSource.volume = Mathf.Lerp(0f, 1, t / clapTime);

            if (clapIncreasing)
            {
                t += Time.deltaTime;
            }
            else if (clapDecreasing)
            {
                t -= Time.deltaTime;
            }
        }

        if (t <= 0)
        {
            StartCoroutine(RunAudienceMember());
        }
    }

    private void StopClapping(StageManager stageManager)
    {
        clapDecreasing = true;
        clapIncreasing = false;
    }
}
