using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource invisibilityBuzz;
    public PlayerController playercontroller;
    public PharaohAbilities pharaohTimers;
    public int soundTimes;

    // Start is called before the first frame update
    void Start()
    {
        invisibilityBuzz = GetComponent<AudioSource>();
        soundTimes = 0;
    }

    // Update is called once per frame
    void Update()
    {
        bool currentplayer = playercontroller.IsCurrentPlayer;
        bool invisible = playercontroller.isInvisible;
        float invisibilityTime = pharaohTimers.invisibilityTimer;

        if(invisible)
        {
            print("invisible");
        }

        if (currentplayer)
        {
            if(soundTimes == 0 && invisible)
            {
                invisibilityBuzz.Play();
                soundTimes++;

                StartCoroutine(WaitForSeconds(2.25f));
            }
        }
    }

    public IEnumerator WaitForSeconds(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);

            soundTimes--;
        }
    }
}
