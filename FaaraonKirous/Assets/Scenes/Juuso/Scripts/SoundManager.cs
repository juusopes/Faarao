using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource invisibilitySound;
    public AudioSource distractSound;
    public AudioSource flashBangSound;
    public AudioSource possessSound;
    public AudioSource warpSound;

    public AudioSource attackSound;
    public AudioSource resurrectSound;
    public AudioSource deathSound;

    public void InvisibilitySound()
    {
        invisibilitySound.Play();
    }

    public void DistractSound()
    {
        distractSound.Play();
    }

    public void FlashBangSound()
    {
        flashBangSound.Play();
    }

    public void PossessSound()
    {
        possessSound.Play();
    }

    public void WarpSound()
    {
        warpSound.Play();
    }

    public void AttackSound()
    {

    }
    public void ResurrectSound()
    {

    }
    public void DeathSound()
    {

    }
}
