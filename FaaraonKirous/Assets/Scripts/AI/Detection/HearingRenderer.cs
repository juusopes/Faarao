using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Using this class causes a lot of Draw calls potentially with material instancing!
/// </summary>
public class HearingRenderer : MonoBehaviour
{
    #region Constants
    private const string TEXTURE = "_MainText";
    private const string COLOR = "_Color";
    private const string SCALE = "_Scale";
    private const string TRANSPARENCY = "_Transparency";
    private const string THICKNESS = "_Thickness";
    private const string POLAR = "_Polar";
    private const string POLAR_RADIALSCALE = "_PolarRadialScale";
    private const string POLAR_LENGHTSCALE = "_PolarLenghtScale";
    #endregion

    //Editing material parameters with script... HenkkasUltimateCircle
    public MeshRenderer OuterCircle;
    public MeshRenderer InnerCircle;
    private Material OuterMaterial;
    private Material InnerMaterial;
    private float animationSpeed = 1.5f;
    private float outerCircleRotationSpeed = 50;

    private float innerScale;
    private float innerThickness;
    private float outerTransparency;
    private float outerThickness;

    public float memberValue;

    // Start is called before the first frame update
    void Start()
    {
        if (OuterCircle == null || InnerCircle == null)
            return;
        OuterMaterial = OuterCircle.material;
        InnerMaterial = InnerCircle.material;
        StartEffect();
    }

    private void StartEffect()
    {
        DefaultValues();
        StartCoroutine(RunEffect());
    }

    private void DefaultValues()
    {
        innerThickness = 0.8f;
        innerScale = 0f;
        outerTransparency = 0.2f;
        outerThickness = 0.05f;
    }

    private IEnumerator RunEffect()
    {
        yield return new WaitForSeconds(2);

        StartCoroutine(AssignFloat(value => innerScale = value, new float[] { 0f, 1f }, 1f / animationSpeed));
        yield return new WaitForSeconds(0.5f / animationSpeed);
        StartCoroutine(AssignFloat(value => innerThickness = value, new float[] { 0.8f, 0f }, 1.5f / animationSpeed));
        yield return new WaitForSeconds(0.5f / animationSpeed);
        StartCoroutine(AssignFloat(value => outerTransparency = value, new float[] { 0.2f, 0.5f, 0f}, 0.2f / animationSpeed));
        yield return new WaitForSeconds(3f / animationSpeed);
        StartCoroutine(AssignFloat(value => outerThickness = value, new float[] { 0.05f, 0.1f, 0.05f }, 1f / animationSpeed));
        yield return new WaitForSeconds(1f / animationSpeed);
        StartCoroutine(AssignFloat(value => outerTransparency = value, new float[] { 0.6f, 1f }, 1f / animationSpeed));
        yield return StartCoroutine(AssignFloat(value => innerScale = value, new float[] { 1f, 0f }, 0.05f / animationSpeed));
        yield return null;
        yield return new WaitForSeconds(2f);
        StartEffect();
    }

    // Update is called once per frame
    void Update()
    {
        InnerCircle.material.SetFloat(SCALE, innerScale);
        InnerCircle.material.SetFloat(THICKNESS, innerThickness);
        OuterCircle.material.SetFloat(TRANSPARENCY, outerTransparency);
        OuterCircle.material.SetFloat(THICKNESS, outerThickness);
        //Debug.Log(innerScale);
    }



    private IEnumerator AssignFloat(Action<float> assigner, float[] arr, float duration)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            float startVal = arr[i - 1];
            float endVal = arr[i];
            float time = 0.0f;
            float result;
            while (time < duration)
            {
                result = Mathf.Lerp(startVal, endVal, time / duration);
                time += Time.deltaTime;
                assigner(result);
                yield return null;
            }
        }
        yield return null;
    }
}
