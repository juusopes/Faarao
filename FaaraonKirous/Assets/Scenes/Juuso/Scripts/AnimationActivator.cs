using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationActivator : MonoBehaviour
{
    public GameObject opener;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        bool activated = opener.GetComponent<ActivatorScript>().activated;

        if (activated)
        {
            animator.enabled = true;
        }
    }
}
