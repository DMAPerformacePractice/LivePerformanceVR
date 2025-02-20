using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudienceMemberManager : MonoBehaviour
{
    private AudienceInterruption[] audienceInterruptions;

    private AudioSource audioSource;

    public bool inPerformance = false;

    // How many seconds between each audience interruption
    [SerializeField] private float interruptionDelayTime = 10;
    // Add randomness to interruption time. Interruptions will happen randomly based on a number of seconds outlined by interval [interruptionDelayTime - 2, interruptionDelayTime + 2].
    [SerializeField] private float interruptionVariability = 2;

    // Start is called before the first frame update
    void Start()
    {
        audienceInterruptions = StageManager.GetAudienceInterruptions();
        audioSource = GetComponent<AudioSource>();
        StageManager.OnPerformaceStartEvent += StartAudienceMember;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartAudienceMember(StageManager stageManager)
    {
        inPerformance = true;
        StartCoroutine(RunAudienceMember());
    }

    // Other option could be something like it has a 5% chance of an interruption every 10 seconds.
    // Should discuss with team
    private IEnumerator RunAudienceMember()
    {
        Debug.Log("Running Audience Member!");

        while (inPerformance)
        {
            yield return new WaitForSeconds(interruptionDelayTime + Random.Range(-interruptionVariability, interruptionVariability));
            float rand = Random.value;
            int interruptionNumber = Mathf.CeilToInt(rand * audienceInterruptions.Length);
            audioSource.PlayOneShot(audienceInterruptions[interruptionNumber - 1].getNoise());
        }
    }
}
