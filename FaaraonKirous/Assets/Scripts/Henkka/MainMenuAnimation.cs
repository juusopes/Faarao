using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAnimation : MonoBehaviour
{
    //Dust cloud going right, eft
    //Pergament flying right/left goon running after it
    //Dust from the ceiling
    //Screen shake and a lot of dust from the ceiling
    //Scaling pharaoh shadow
    //Random flying object?
    // Start is called before the first frame update
    private const string TEXTURE = "_MainText";
    private const string COLOR = "_Color";
    private const string OPACITY = "_Opacity";
    private const string SATURATION = "_Saturation";

    private float lightOpacity = 0.98f;
    private float hieroglyphSaturation = 0.4f;
    private float lightAnimationSpeed = 0.09f;
    private float hieroglyphAnimationSpeed = 0.11f;
    private float menuTitleSpeed = 0.035f;
    private float titleOpacity = 1.0f;


    public RawImage title;
    public GameObject start;
    public GameObject mid;
    public GameObject end;
    public GameObject goon;
    public ParticleSystem fallingDust;
    public GameObject blood;
    public SkinnedMeshRenderer goon_shadow;
    public GameObject ghost;
    public MeshRenderer fakeLight;
    public SpriteRenderer hieroglyphs;

    private void Start()
    {
        fallingDust.Stop();
        blood.SetActive(false);
        ghost.transform.localScale = Vector3.zero;
        StartCoroutine(MenuAnimation());
        StartCoroutine(LightAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        title.material.SetFloat(OPACITY, titleOpacity);
        fakeLight.material.SetFloat(OPACITY, lightOpacity);
        hieroglyphs.material.SetFloat(SATURATION, hieroglyphSaturation);

    }

    public IEnumerator LightAnimation()
    {
        while (true)
        {
            yield return StartCoroutine(AssignFloat(value => lightOpacity = value, new float[] { 0.98f, 0.93f, 0.99f, 0.9f, 0.97f, 0.92f, 0.98f }, 0.05f / lightAnimationSpeed));
        }
    }

    public IEnumerator HieroGlyphAnimation()
    {
        while (true)
        {
            yield return StartCoroutine(AssignFloat(value => hieroglyphSaturation = value, new float[] { 0.8f, 1.2f, 0.8f }, 0.05f / hieroglyphAnimationSpeed));
        }
    }

    public IEnumerator MenuAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(GoonMovement(goon, 6.0f));
            yield return new WaitForSeconds(3f);
            fallingDust.Play();
            yield return StartCoroutine(AssignFloat(value => hieroglyphSaturation = value, new float[] { 0.5f, 1f, 0.6f, 1.2f, 0.6f, 1.2f, 0.6f, 1.2f, 0.6f, 1.2f, 0.6f, 1.2f, 0.6f, 1.2f, 0.6f, 1.2f }, 0.05f / hieroglyphAnimationSpeed));
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(ScaleOverSeconds(ghost, new Vector3(30f, 30f, 30f), 6.0f));
            yield return new WaitForSeconds(5f);
            blood.SetActive(true);
            yield return StartCoroutine(GoonThrow(goon, 1.5f));
            //StartCoroutine(HieroGlyphAnimation());
            StartCoroutine(AssignFloat(value => titleOpacity = value, new float[] { 1f, 0.1f }, 0.05f / menuTitleSpeed));
            yield return new WaitForSeconds(2f);
            fallingDust.Stop();
            yield return new WaitForSeconds(1f);

            StartCoroutine(AssignFloat(value => hieroglyphSaturation = value, new float[] {1.2f, 0.5f }, 0.05f / hieroglyphAnimationSpeed));

            yield return new WaitForSeconds(1f);
            ResetValues();
            yield return new WaitForSeconds(3f);
            StartCoroutine(AssignFloat(value => titleOpacity = value, new float[] { 0.1f, 1f }, 0.05f / menuTitleSpeed));
            yield return new WaitForSeconds(10f);
        }
    }

    private void ResetValues()
    {
        blood.SetActive(false);
        lightOpacity = 0.98f;
        hieroglyphSaturation = 0.5f;
        lightAnimationSpeed = 0.09f;
        hieroglyphAnimationSpeed = 0.11f;
    }

    public IEnumerator GoonMovement(GameObject objectToScale, float seconds)
    {
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            float percentage = elapsedTime / seconds;
            //Fake depth write for shadow
            if (percentage < 0.2 || percentage > 0.73f)
                goon_shadow.enabled = false;
            else
                goon_shadow.enabled = true;

            objectToScale.transform.localScale = Vector3.Lerp(start.transform.localScale, mid.transform.localScale, (percentage));
            objectToScale.transform.position = Vector3.Lerp(start.transform.position, mid.transform.position, (percentage));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator GoonThrow(GameObject objectToScale, float seconds)
    {
        float elapsedTime = 0;
        Quaternion startingRot = objectToScale.transform.localRotation;
        while (elapsedTime < seconds)
        {
            float percentage = elapsedTime / seconds;
            //Fake depth write for shadow
            goon_shadow.enabled = false;

            objectToScale.transform.localRotation = Quaternion.Lerp(startingRot, end.transform.rotation, (percentage));
            objectToScale.transform.localScale = Vector3.Lerp(mid.transform.localScale, end.transform.localScale, (percentage));
            objectToScale.transform.position = Vector3.Lerp(mid.transform.position, end.transform.position, (percentage));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectToScale.transform.localRotation = startingRot;
    }

    public IEnumerator ScaleOverSeconds(GameObject objectToScale, Vector3 scaleTo, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingScale = Vector3.zero;
        while (elapsedTime < seconds)
        {
            objectToScale.transform.localScale = Vector3.Lerp(startingScale, scaleTo, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectToScale.transform.localScale = Vector3.zero;
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
