using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all the task involved in running an audience member, primarily randomly playing an interruption.
/// </summary>
public class AudienceMemberManager : MonoBehaviour
{
    /// <summary>
    /// All the saved AudienceInterruptions
    /// </summary>
    private AudienceInterruption[] audienceInterruptions;

    /// <summary>
    /// All the save clap-type AudienceInterruptions
    /// </summary>
    private AudienceInterruption[] claps;

    /// <summary>
    /// Which clap type the audience members will do
    /// </summary>
    private static int clapType = 0;

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

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Variables
        audienceInterruptions = StageManager.GetAudienceInterruptions();
        claps = StageManager.GetClaps();
        if (clapType == 0)
        {
            float rand = Random.value;
            clapType = Mathf.CeilToInt(rand * claps.Length);
        }
        audioSource = GetComponentInChildren<AudioSource>();
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
        animator.SetBool("Not Clapping", true);
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
            // Need to call this here so that the while loop will be called concurently to everything else
            yield return null;

            if (inInterruption == false)
            {
                // Wait the appropriate amount of time
                yield return new WaitForSeconds(interruptionDelayTime + Random.Range(-interruptionVariability, interruptionVariability));

                if (inPerformance)
                {
                    animator.SetBool("Not Clapping", true);

                    // Choose and play a random interruption
                    float rand = Random.value;
                    int interruptionNumber = Mathf.CeilToInt(rand * audienceInterruptions.Length);

                    // Play the animation noise, if there is one
                    if (audienceInterruptions[interruptionNumber - 1].getNoise() != null)
                    {
                        audioSource.PlayOneShot(audienceInterruptions[interruptionNumber - 1].getNoise());
                    }

                    // Play the animation itself, if there is one
                    if (audienceInterruptions[interruptionNumber - 1].getAnimationNumber() != 0)
                    {
                        animator.SetInteger("Interruption Number", audienceInterruptions[interruptionNumber - 1].getAnimationNumber());
                        animator.SetTrigger("Animation Change");
                        inInterruption = true;
                    }

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
        // Wait until the idle animation is the current animation
        yield return new WaitUntil(() =>
        {
            if (stateInfo.IsName("Idle"))
            {
                return true;
            }

            return false;
        });

        // That means the interruption being played must have ended
        inInterruption = false;
    }

    /// <summary>
    /// Start having the audience member clap (animation & sound).
    /// </summary>
    /// <param name="stageManager"></param>
    private void StartClapping(StageManager stageManager)
    {
        // Make sure we aren't currently playing a clap animation
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Clap"))
        {
            // Choose a random clap and play it
            // Play clapping noise
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.clip = claps[1].getNoise();
                audioSource.Play();
                
            }

            // Play clapping animation
            animator.SetInteger("Interruption Number", claps[1].getAnimationNumber());
            animator.SetTrigger("Animation Change");
            animator.SetBool("Not Clapping", false);
        }
    }

    /// <summary>
    /// Stop having the audience member clap (animation & sound).
    /// </summary>
    /// <param name="stageManager"></param>
    private void StopClapping(StageManager stageManager)
    {
        // Only trigger if the current animation is actually a clapping animation
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Clap"))
        {
            // Stop the clapping sound
            audioSource.loop = false;
            audioSource.Stop();

            // Stop the clapping animation
            animator.SetBool("Not Clapping", true);
        }
    }
}
