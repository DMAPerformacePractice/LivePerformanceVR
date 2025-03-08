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

    /// <summary>
    /// The Animator with which the audience member will play animations
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Whether the audience member is currently running and interruption animation or not
    /// </summary>
    private bool inInterruption = false;

    /// <summary>
    /// Changed based on signals from the StageManager. Whether the audience member thinks that performance mode is on or not
    /// </summary>
    public bool inPerformance = false;

    /// <summary>
    /// How many seconds between each audience interruption.
    /// </summary>
    [Tooltip("How many seconds between each audience interruption.")]
    [SerializeField] private float interruptionDelayTime = 30;

    /// <summary>
    /// Add randomness to interruption time. Interruptions will happen randomly based on a number of seconds outlined by interval [interruptionDelayTime - 2, interruptionDelayTime + 2].
    /// </summary>
    [Tooltip("Add randomness to interruption time. Interruptions will happen randomly based on a number of seconds outlined by the interval [interruptionDelayTime - 2, interruptionDelayTime + 2].")]
    [SerializeField] private float interruptionVariability = 20;

    /// <summary>
    /// How long the audience will take to reach max clapping volume.
    /// </summary>
    [Tooltip("How long the audience will take to reach max clapping volume.")]
    [SerializeField] private float clapTime = 5;
    // Used to keep track of whether or not the audience member is already clapping
    private Coroutine clappingCoroutine;

    /// <summary>
    /// Whether the clapping volume is increasing or not
    /// </summary>
    private bool clapIncreasing = false;
    /// <summary>
    /// Whether the clapping volume is decreasing or not
    /// </summary>
    private bool clapDecreasing = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Variables
        audienceInterruptions = StageManager.GetAudienceInterruptions();
        claps = StageManager.GetClaps();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
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
        //animator.SetBool("Clapping", false);
        audioSource.loop = false;
        StopClapping(stageManager);
    }

    /// <summary>
    /// Runs the AudienceMember. Will automatically play a random interruption every <see cref="interruptionDelayTime"/> with a variablity range of plus-minus <see cref="interruptionVariability"/>.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunAudienceMember()
    {
        while (inPerformance)
        {
            yield return null;
            if (inInterruption == false)
            {
                // Wait the appropriate amount of time
                yield return new WaitForSeconds(interruptionDelayTime + Random.Range(-interruptionVariability, interruptionVariability));
                if (inPerformance)
                {
                    // Choose and play a random interruption
                    float rand = Random.value;
                    int interruptionNumber = Mathf.CeilToInt(rand * audienceInterruptions.Length);
                    // Play the animation noise, if there is one
                    audioSource.PlayOneShot(audienceInterruptions[interruptionNumber - 1].getNoise());
                    // Play the animation itself, if there is one
                    animator.SetInteger("Interruption Number", audienceInterruptions[interruptionNumber - 1].getAnimationNumber());
                    animator.SetTrigger("Animation Change");
                    inInterruption = true;
                    // Start checking for when the audience member returns to idle
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    StartCoroutine(MonitorAnimationState(stateInfo));
                }
            }
        }
    }

    /// <summary>
    /// Monitors whether the audience member has returned to the Idle animation. Used to detect when the member is no longer <c>inInterruption</c>.
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <returns></returns>
    private IEnumerator MonitorAnimationState(AnimatorStateInfo stateInfo)
    {
        yield return new WaitUntil(() =>
        {
            if (stateInfo.IsName("Idle"))
            {
                return true;
            }

            return false;
        });

        inInterruption = false;
    }

    /// <summary>
    /// Start having the audience member clap & gradually increase the volume.
    /// </summary>
    /// <param name="stageManager"></param>
    private void StartClapping(StageManager stageManager)
    {
        // If the audience member isn't already clapping, start them clapping
        if (clappingCoroutine == null)
        {
            // Make sure the audience member won't do a random interruption
            StopAllCoroutines();
            // Choose a random clap and play it
            float rand = Random.value;
            int clapNumber = Mathf.CeilToInt(rand * claps.Length);
            // Play clapping noise
            audioSource.PlayOneShot(claps[clapNumber - 1].getNoise());
            // Play clapping animation
            animator.SetInteger("Interruption Number", claps[clapNumber - 1].getAnimationNumber());
            animator.SetTrigger("Animation Trigger");

            // Start the clapping coroutine
            clappingCoroutine = StartCoroutine(ManageClapping());
        }
        // Set the audience member clapping volume to increasing
        clapIncreasing = true;
        clapDecreasing = false;
    }

    /// <summary>
    /// Manages the audience member's clapping loop. Switchs between increasing and decreasing the clapping volume based on <see cref="clapIncreasing"/> and <see cref="clapDecreasing"/>.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ManageClapping()
    {
        var t = 0f;

        // Main logic loop
        while (t < clapTime && t >= 0)
        {
            // Necessary to run as a separate coroutine
            yield return null;

            // Lerp volume based on how far along the time period we are (so halfway through the time period of clapTime, the volume should be at 0.5)
            audioSource.volume = Mathf.Lerp(0f, 1, t / clapTime);
            animator.speed = Mathf.Lerp(0f, 1, t / clapTime);

            // Either increase or decrease t, based on whether the volume should be increasing or decreasing
            if (clapIncreasing && t < clapTime)
            {
                t += Time.deltaTime;
            }
            else if (clapDecreasing)
            {
                t -= Time.deltaTime;
            }
        }

        animator.speed = 1;

        // If volume went to zero, then we must be done clapping, so resume normal running procedures
        if (t <= 0)
        {
            animator.SetInteger("Interruption Number", 0);
            StartCoroutine(RunAudienceMember());
        }
    }

    /// <summary>
    /// Slowly stop having the audience member clap & gradually decrease the volume.
    /// </summary>
    /// <param name="stageManager"></param>
    private void StopClapping(StageManager stageManager)
    {
        // Set the audience member clapping volume to decreasing
        clapDecreasing = true;
        clapIncreasing = false;
    }
}
