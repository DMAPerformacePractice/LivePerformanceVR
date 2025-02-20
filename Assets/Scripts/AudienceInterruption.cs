using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interruption", menuName = "Custom Objects/Audience Interruption", order = 1)]
public class AudienceInterruption : ScriptableObject
{
    [SerializeField] private Animation movementAnimation;

    [SerializeField] private AudioClip noise;

    public AudienceInterruption()
    {

    }

    public Animation getAnimation()
    {
        return movementAnimation;
    }

    public AudioClip getNoise()
    {
        return noise;
    }
}
