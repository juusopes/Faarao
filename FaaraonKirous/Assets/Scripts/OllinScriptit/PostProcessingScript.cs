using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingScript : MonoBehaviour
{
    public Volume volume;
    private LevelController levelController;
    private ColorAdjustments cA;

    // Start is called before the first frame update
    void Start()
    {
        volume = this.gameObject.GetComponent<Volume>();
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager._instance.IsFullyLoaded)
        {
            CamAdjustments();
        }
    }

    private void CamAdjustments()
    {
        if (levelController.currentCharacter != null)
        {
            if (levelController.currentCharacter.GetComponent<PlayerController>().IsDead)
            {
                if (volume.profile.TryGet<ColorAdjustments>(out cA))
                {
                    if (cA.saturation.value > -100)
                    {
                        cA.saturation.value -= Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.saturation.value = -100;
                    }
                }
            }
            if (!levelController.currentCharacter.GetComponent<PlayerController>().IsDead)
            {
                if (volume.profile.TryGet<ColorAdjustments>(out cA))
                {
                    if (cA.saturation.value < 0)
                    {
                        cA.saturation.value += Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.saturation.value = 0;
                    }
                }
            }
            if (levelController.currentCharacter.GetComponent<PlayerController>().IsInvisible)
            {
                if (volume.profile.TryGet<ColorAdjustments>(out cA))
                {
                    if (cA.hueShift.value < 180)
                    {
                        cA.hueShift.value += Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.hueShift.value = 180;
                    }
                    if (cA.saturation.value < 100)
                    {
                        cA.saturation.value += Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.saturation.value = 100;
                    }
                }
            }
            if (!levelController.currentCharacter.GetComponent<PlayerController>().IsInvisible && !levelController.currentCharacter.GetComponent<PlayerController>().IsDead)
            {
                if (volume.profile.TryGet<ColorAdjustments>(out cA))
                {
                    if (cA.hueShift.value > 0)
                    {
                        cA.hueShift.value -= Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.hueShift.value = 0;
                    }
                    if (cA.saturation.value > 0)
                    {
                        cA.saturation.value -= Time.deltaTime * 100;
                    }
                    else
                    {
                        cA.saturation.value = 0;
                    }
                }
            }
        }
    }
}
