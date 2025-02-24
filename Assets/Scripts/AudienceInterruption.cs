using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interruption", menuName = "Custom Objects/Audience Interruption", order = 1)]
/// <summary>
/// A ScriptableObject type made to make the creation & introduction of audience interruptions into the program easier.
/// </summary>
public class AudienceInterruption : ScriptableObject
{
    /// <summary>
    /// The animation, if any, to play when this interruption is triggered.
    /// </summary>
    [Tooltip("The animation, if any, to play when this interruption is triggered.")]
    [SerializeField] private Animation movementAnimation;

    /// <summary>
    /// The noise, if any, to play when this interruption is triggered.
    /// </summary>
    [Tooltip("The noise, if any, to play when this interruption is triggered.")]
    [SerializeField] private AudioClip noise;

    public AudienceInterruption()
    {

    }

    /// <summary>
    /// Returns the <c>movementAnimation</c> attribute of this AudienceInterruption.
    /// </summary>
    public Animation getAnimation()
    {
        return movementAnimation;
    }

    /// <summary>
    /// Returns the <c>noise</c> attribute of this AudienceInterruption.
    /// </summary>
    public AudioClip getNoise()
    {
        return noise;
    }
}
