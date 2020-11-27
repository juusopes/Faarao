using UnityEngine;
class DetectionPercentageCalculator
    {
    private float lineSpeed;
    private float lineLenght = 0;
    private float maxLenght = 0;
    private const float lineSpeedRangeMultiplier = 2f;
    private const float lineShrinkSpeedMultiplier = 0.4f;
    private float sightPercentage;
    private int scalingDirection = 1;      //Going towards 1 or away -1

    public DetectionPercentageCalculator(float lineSpeed, float maxLenght)
    {
        this.lineSpeed = lineSpeed;
        this.maxLenght = maxLenght;
    }

    /// <summary>
    /// Simulates sight and draws a sight line. Returns true once the enemy detection line end has reached player. If enemy can see player the line expands, otherwise it shrinks.
    /// </summary>
    /// <param name="CanSeeObject">If enemy can see player</param>
    /// <returns></returns>
    public float SimulateFOVPercentage(bool CanSeeObject)
        {
            if (CanSeeObject || sightPercentage > 0)        //Only run if we see player or line is out
            {
                scalingDirection = CanSeeObject ? 1 : -1;
                float lineSpeedScale = scalingDirection == 1 ? lineSpeed : lineSpeed * lineShrinkSpeedMultiplier;
                lineSpeedScale = lineSpeedScale + lineSpeedRangeMultiplier * sightPercentage;

                lineLenght = Mathf.Min(lineLenght + scalingDirection * lineSpeedScale * Time.deltaTime, maxLenght);

                sightPercentage = lineLenght / maxLenght;

                //Debug.Log(sightPercentage);

                if (CanSeeObject && Mathf.Approximately(1, sightPercentage))
                    return 1f;
            }

            return sightPercentage;
        }
    }
