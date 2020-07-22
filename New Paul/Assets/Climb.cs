using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Climb : MonoBehaviour
{
    // config
    [SerializeField] float climbSpeed = 10f;

    //cache
    BoxCollider2D myBoxCollider2D;
    Animator myAnimator;
    Rigidbody2D myRigidBody;
    float gravityScaleAtStart;



    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider2D = GetComponent<BoxCollider2D>();
        myAnimator = GetComponent<Animator>();

    }

    void Update()
    {
        ClimbLadder();
    }

    private void ClimbLadder()
    {

        if (!myBoxCollider2D.IsTouchingLayers(LayerMask.GetMask("Ladders")))
        {
            myAnimator.SetBool("climb", false);
            myRigidBody.gravityScale = gravityScaleAtStart;
            return;
        }

        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * climbSpeed);
        myRigidBody.velocity = climbVelocity;
        myRigidBody.gravityScale = 0f;

        bool playerIsClimbing = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("climb", playerIsClimbing);
    }
}
