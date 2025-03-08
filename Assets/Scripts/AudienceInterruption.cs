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
    /// The number of the animation, if any, to play when this interruption is triggered.
    /// </summary>
    [Tooltip("The number of the animation, if any, to play when this interruption is triggered.")]
    [SerializeField] private int animationNumber;

    /// <summary>
    /// The noise, if any, to play when this interruption is triggered.
    /// </summary>
    [Tooltip("The noise, if any, to play when this interruption is triggered.")]
    [SerializeField] private AudioClip noise;

    public AudienceInterruption()
    {

    }

    /// <summary>
    /// Returns the <c>animationNumber</c> attribute of this AudienceInterruption.
    /// </summary>
    public int getAnimationNumber()
    {
        return animationNumber;
    }

    /// <summary>
    /// Returns the <c>noise</c> attribute of this AudienceInterruption.
    /// </summary>
    public AudioClip getNoise()
    {
        return noise;
    }
}
