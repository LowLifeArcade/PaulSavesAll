using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour
{

    //Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 10f;
    [SerializeField] float jumpPressedRememberedTime = 0.3f;
    [SerializeField] Vector2 deathKick = new Vector2(25f, 25f);
    [SerializeField] float jumpFallSpeed = Mathf.Sign(9f);
    [SerializeField] float jumpHeightBeforeFall = Mathf.Sign(1f);
    [SerializeField] float jumpSideSpeed = 2f;
    [SerializeField] float rampUp = 0.5f;        // How fast we get speed forward/backward
    [SerializeField] float deceleration = 0.5f;  // How fast we decelerate when we are no longer gaining speed
    [SerializeField] float runClampNumber = -1f;
    [SerializeField] float jumpAdd = 20f;
    [SerializeField] float gravity = 10f;
    [SerializeField] float fallClamp = 10f;
    [SerializeField] float lowJumpHeight = 1f;

    //[SerializeField] float testSidespeed = 10f;


    //State
    bool isAlive = true;


    // Cached componet references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    BoxCollider2D myBodyCollider2D;
    BoxCollider2D myFeet;
    float jumpPressedRemembered = 0f;
    float oldPos;
    float speed;
    float facing;
    float facingFixed;


    //Collider2D myCollider2D;
    float gravityScaleAtStart;


    // Message then methods
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider2D = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
        myFeet = GetComponent<BoxCollider2D>();
    }


    private void FixedUpdate()
    {
        oldPos = myRigidBody.position.x;
        Vector2 direction;
        
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            direction = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
            facingFixed = direction.x;
        }
    }


    void Update()
    {
        if (!isAlive) { return; }
        Run();
        ClimbLadder();
        Jump();
        FlipSprite();
        Rygar();

    }


    //find a way to take speed back to base number so you can't press horizontal back and forth gaining speed

    private void Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxisRaw("Horizontal");
        
        // acceleration and deceleration 
        if (Mathf.Abs(controlThrow) <= .9f)
        {
            speed = Mathf.MoveTowards(speed, 0f, deceleration * Time.deltaTime);
        }
        else if (Mathf.Abs(controlThrow) == 1f && facingFixed != facing)
        {
            speed = Mathf.MoveTowards(speed, 0f, deceleration * 1f);
            facingFixed = facing;
        }
        else
        {
            speed += Mathf.Abs(controlThrow) * rampUp * Time.deltaTime;
        }

        speed = Mathf.Clamp(speed, runClampNumber, Mathf.Abs(runClampNumber));
        Vector2 playerVelocity = new Vector2(controlThrow * speed * runSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        //fall and stop against wall animation 
        if (feetOnGround()) 
        {
            myAnimator.SetBool("jump", false);
            myAnimator.SetBool("running", myRigidBody.position.x != oldPos && myRigidBody.velocity.x != 0);
        }
        else
        {
            myAnimator.SetBool("running", false);
            myAnimator.SetBool("jump", true);
        }
    }

     
    // add something that has the player keep his momentum when y is current he also gets x movement more than already in run
    // something that takes the horizontal input at time of jump, captures it, disables horizontal input effecting it, and
    // keeps that momentum for the jump until feet land on ground kinda like supermetroid 

    private void Jump()
    {
        float oldPosJump = myRigidBody.velocity.y;
        // disables ability to jump until descending 
        bool jumpActive = false;
        myFeet.enabled = true;
        myRigidBody.gravityScale = gravityScaleAtStart;


        if (myRigidBody.velocity.y >= 0.01f)
        {
            jumpActive = true;
            myFeet.enabled = !myFeet.enabled;
            myRigidBody.gravityScale = gravity;
        }

        //modifies descending speed 
        else if (myRigidBody.velocity.y <= jumpHeightBeforeFall && !feetOnGround())
        {
            float xFall = myRigidBody.velocity.x + facing * jumpSideSpeed * Time.deltaTime;
            //float yFall = myRigidBody.velocity.y + Physics2D.gravity.y * (jumpFallSpeed - 1) * -Time.deltaTime;
            //yFall = Mathf.Clamp(yFall, -.001f, -10f);

            myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (jumpFallSpeed - 1) * Time.deltaTime;

            Mathf.Clamp(myRigidBody.velocity.y, -.001f, -2f);
            //myRigidBody.velocity += new Vector2(xFall, yFall);
            Debug.Log(myRigidBody.velocity.y);
        }


        //pre jump timer
        jumpPressedRemembered -= Time.deltaTime;
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            jumpPressedRemembered = jumpPressedRememberedTime;
        }

        //actual jump
        if (jumpPressedRemembered > 0 && feetOnGround()) // !Input.GetKeyDown("space"))//|| CrossPlatformInputManager.GetButtonDown("Jump"))//jumpPressedRemembered > 0 
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            if (myRigidBody.velocity.y > 0 && !Input.GetKey("space"))
            {
                myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpHeight - 1) * Time.deltaTime;
            }

            //wall jump
            if (myRigidBody.position.y != oldPosJump && myRigidBody.velocity.y <= -.001f)
            {
                myRigidBody.velocity += jumpVelocityToAdd - new Vector2(myRigidBody.velocity.x, myRigidBody.velocity.y);
                myRigidBody.gravityScale = gravityScaleAtStart;
                Invoke("afterJumpSideSpeed", .03f);
            }

            // full jump
            else if (myRigidBody.velocity.y <= .009f)
            {
                myRigidBody.velocity += jumpVelocityToAdd - myRigidBody.velocity;
                myRigidBody.gravityScale = gravityScaleAtStart;
            }
            //Debug.Log(myRigidBody.velocity.x);
        }

   

        //cancels jump after landing
        if (feetOnGround() && jumpActive)
        {
            //myAnimator.SetBool("jump", false);
            return;
        }

        Debug.Log(jumpActive);

    }

    private void afterJumpSideSpeed()
    {
        
        Vector2 jumpVelocityToAdd = new Vector2(facing * jumpAdd, myRigidBody.velocity.y);
        myRigidBody.velocity += jumpVelocityToAdd - myRigidBody.velocity;
    }

    bool feetOnGround()
    {
        return myFeet.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private void Rygar()
    {
        if (myFeet.IsTouchingLayers(LayerMask.GetMask("enemy"))) 
        {
            GetComponent<Rigidbody2D>().velocity = deathKick;
            if (myRigidBody.velocity.y >= 0.01f)
            {
                myAnimator.SetBool("jump", false);
            }
        }
    }


    private void FlipSprite()
    {
        
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
            facing = transform.localScale.x;
        }
        
    }
    private void ClimbLadder()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        bool playerIsClimbing = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

        if (!myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Ladders")))
        {
            myAnimator.SetBool("climb", false);
            myRigidBody.gravityScale = gravityScaleAtStart;
            return;
        }

        if (controlThrow > .1f || controlThrow < -.1f)
        {
            Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * climbSpeed);
            myRigidBody.velocity = climbVelocity;
            myRigidBody.gravityScale = 0f;
            myAnimator.SetBool("climb", playerIsClimbing);
        }

    }
}



/* private void Die()
{
    if (myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("enemy")))
    {
        print("has died");
        // spin z axis
        myAnimator.SetTrigger("hurt");
        GetComponent<Rigidbody2D>().velocity = deathKick;
    }
} */

/* Xbox controls;

private void Awake()
{
    controls = new Xbox();
    controls.Gameplay.jump.performed += jumpbutton => Jump();
    controls.Gameplay.move.performed += runbutton => Run();
}
private void OnEnable()
{
    controls.Gameplay.Enable();
}

private void OnDisable()
{
    controls.Gameplay.Disable();
} */

//something for scaling gravity when going up and down i need to use
//myRigidBody.gravityScale = 0f;
// myRigidBody.gravityScale = gravityScaleAtStart;
// myRigidBody.gravityScale = 0f;


//don't know if i need this was after first if statement in jump method
//else if (myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
////|| myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
//{
//    jumpActive = false;
//    myAnimator.SetBool("jump", false);
//    //myAnimator.SetBool("falling", false);


//}

// fall time increasing to play with

//reenables ability to jump
//else
//{
//    //myFeet.enabled = true;
//    jumpActive = false;
//}