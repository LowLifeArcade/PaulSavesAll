using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paul : MonoBehaviour
{

    Animator myAnimator;

    // Start is called before the first frame update
    void Start()
    {

        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Kick();

    }

    private void Kick()
    {
        myAnimator.SetBool("kick", true);
    }
}
