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

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Variables
        audienceInterruptions = StageManager.GetAudienceInterruptions();
        audioSource = GetComponent<AudioSource>();
        // Add Methods to StageManager Events
        StageManager.OnPerformanceStartEvent += StartAudienceMember;
        StageManager.OnPerformaceEndEvent += StopAudienceMember;
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
}
