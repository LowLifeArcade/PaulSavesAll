using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour
{

    //Config

    // run
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float rampUp = 0.5f;        
    [SerializeField] float deceleration = 0.5f;  
    [SerializeField] float runClampNumber = -1f;
    [SerializeField] float slowMo = .5f;
    

    // jump
    [SerializeField] float jumpHeight = 5f; 
    [SerializeField] float jumpPressedRememberedTime = 0.3f;
    [SerializeField] float jumpingUp = 2.2f;
    [SerializeField] float jumpFallSpeed = Mathf.Sign(2.2f);
    [SerializeField] float jumpHeightBeforeFall = Mathf.Sign(1f);
    [SerializeField] float jumpSideSpeed = 2f;
    [SerializeField] float jumpSideSpeedToAdd = 20f;
    [SerializeField] float fallClamp = 10f;
    [SerializeField] float lowJumpHeight = 1f;
    [SerializeField] float drag = 3f;
    [SerializeField] float gravity = 10f;
    [SerializeField] Vector2 rygarJump = new Vector2(25f, 25f);
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float turnDeceleration = 30f;
    [SerializeField] float timer = .5f;



    // State
    bool isAlive = true;
    bool running = false;



    // Cached componet references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider2D;
    BoxCollider2D myFeet;
    PolygonCollider2D myDetector2D;
    CircleCollider2D myLanding2D;
    float jumpPressedRemembered = 0f;
    float jumpPressedRememberedUp = 0f;
    float beforeWallPosition;
    float speed;
    float facing;
    float facingFixed;
    float jumpingFixedY;
    float jumpingFixedX;
    //float oldPos;



    //Collider2D myCollider2D;
    float gravityScaleAtStart;


    // Message then methods
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider2D = GetComponent<CapsuleCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
        myFeet = GetComponent<BoxCollider2D>();
        myDetector2D = GetComponent<PolygonCollider2D>();
        myLanding2D = GetComponent<CircleCollider2D>();
    }


    private void FixedUpdate()
    {
        //float beforeWallPosition = myRigidBody.position.x;
        //Vector2 direction;
        
        //bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        //if (playerHasHorizontalSpeed)
        //{
        //    direction = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        //    facingFixed = direction.x;
        //}
    }


    void Update()
    {
        if (!isAlive) { return; }
        Run();
        Jump();
        FlipSprite();
        Rygar();

    }


    //find a way to take speed back to base number so you can't press horizontal back and forth gaining speed

    private void Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxisRaw("Horizontal");
        Invoke("getPosBeforeWall", .1f);

        //myAnimator.speed = 1f;


        // deceleration 
        if (Mathf.Abs(controlThrow) <= .9f)
        {
            speed = Mathf.MoveTowards(speed, 0f, deceleration * Time.deltaTime);
        }


        // turn deceleration 
        else if (facingFixed != facing && feetOnGround())
        {
            speed = Mathf.MoveTowards(speed, 0f, deceleration * 1f);
            facingFixed = facing;
        }


        // acceleration
        else
        {
            speed += Mathf.Abs(controlThrow) * rampUp * Time.deltaTime;
        }


        // run
        speed = Mathf.Clamp(speed, runClampNumber, Mathf.Abs(runClampNumber));
        Vector2 playerVelocity = new Vector2(controlThrow * speed * runSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;




        // run faster
        if (CrossPlatformInputManager.GetButtonDown("Run") && running == false)
        {
            running = true;
            runSpeed *= 1.3f;
            myAnimator.speed = 1.15f;
        }
        else if (CrossPlatformInputManager.GetButtonUp("Run") && running == true)
        {
            runSpeed /= 1.3f;
            running = false;
            myAnimator.speed = 1f;

        }


        // run and stop against wall animation 
        if (feetOnGround())
        {
            myAnimator.SetBool("jump", false);
            myAnimator.SetBool("running", myRigidBody.position.x != beforeWallPosition && myRigidBody.velocity.x != 0 && !myDetector2D.IsTouchingLayers(LayerMask.GetMask("Ground")));

        }

        //if (running == false)
        //{
        //    speed /= 3f;
        //}


        //if (feetOnGround() && myRigidBody.position.x != beforeWallPosition && myRigidBody.velocity.x != 0 && !myDetector2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        //{
        //    myAnimator.SetBool("jump", false);
        //    myAnimator.SetFloat("running1", 1f);
        //}


        // fall animation
        else
        {
            myAnimator.SetBool("running", false);
            myAnimator.SetBool("jump", true);
            myAnimator.speed = 1.5f;

        }


    }


    // a lazy way of doing things tbh
    void getPosBeforeWall()
    {
        float beforeWallPosition;

        beforeWallPosition = myRigidBody.position.x;
    }


    // add something that has the player keep his momentum when y is current he also gets x movement more than already in run
    // something that takes the horizontal input at time of jump, captures it, disables horizontal input effecting it, and
    // keeps that momentum for the jump until feet land on ground kinda like supermetroid 

    private void Jump()
    {
        bool jumpActive = false;
        myFeet.enabled = true;
        myRigidBody.gravityScale = gravityScaleAtStart;

        // used to detect if jumping or falling
        jumpPressedRememberedUp -= Time.deltaTime;
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            jumpPressedRememberedUp = jumpingUp;
        }

        // disables ability to jump until descending // took jump active from here
        if (myRigidBody.velocity.y >= 0.01f)
        {
            myFeet.enabled = !myFeet.enabled;
            myRigidBody.gravityScale = gravity;
        }

        //modifies descending speed 
        else if (CrossPlatformInputManager.GetButton("Jump") && myRigidBody.velocity.y <= jumpHeightBeforeFall && !feetOnGround())
        {
            float fallSpeed = (Physics2D.gravity.y * (jumpFallSpeed - 1)*timer* Time.deltaTime);
            float clamped = Mathf.Clamp(fallSpeed, 0f, -15f);
            Mathf.MoveTowards(myRigidBody.velocity.y, clamped, 4f);

            myRigidBody.velocity += new Vector2(0f, fallSpeed)  ;
        }

        // falling off something with no jump
        else if (!feetOnGround())
        {
            myRigidBody.velocity += Vector2.up + Physics2D.gravity * 1.1f * Time.deltaTime;
        }


        //pre jump timer
        jumpPressedRemembered -= Time.deltaTime;
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            jumpPressedRemembered = jumpPressedRememberedTime;
        }

        Invoke("quickLookCompareJump", .01f);

        //actual jump
        if (jumpPressedRemembered > 0 && feetOnGround()) // !Input.GetKeyDown("space")) || CrossPlatformInputManager.GetButtonDown("Jump"))//jumpPressedRemembered > 0 
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpHeight);

     

            // full jump
            if (myRigidBody.velocity.y <= .001f)
            {
                myRigidBody.velocity += jumpVelocityToAdd - myRigidBody.velocity;
                myRigidBody.gravityScale = gravityScaleAtStart;

                //// wall jump
                if (myRigidBody.position.y != jumpingFixedY )
                {
                
                    Invoke("afterJumpSideSpeed", .03f);

                }
            }
        }

        if (myRigidBody.velocity.y <= -.001f)
        {
            jumpActive = true;
        }


        // short hop
        if (!CrossPlatformInputManager.GetButton("Jump") && myRigidBody.velocity.y > 1f)
        {
            //myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * -1f * (lowJumpHeight - 1) * Time.deltaTime;

            myRigidBody.velocity += Vector2.up * -1f * lowJumpHeight;
            Debug.Log("let go of  jump!");
        }

        // landing and slow down or trying to at least
        if (myLanding2D.IsTouchingLayers(LayerMask.GetMask("Ground")) && jumpActive)
        {
            //var yVelocity = myRigidBody.velocity.y;
            Vector2 landingVelocity = new Vector2(facing * 3f, myRigidBody.velocity.y);
            //yVelocity = myRigidBody.velocity.x;
            //speed = Mathf.MoveTowards(speed, 0f, deceleration * 1f);
            myRigidBody.velocity = landingVelocity;
            //print("landed");
            jumpActive = false;
            myAnimator.speed = 1f;

            return;
        }
    }

    private void afterJumpSideSpeed()
    {
        
        Vector2 jumpVelocityToAdd = new Vector2(facing * jumpSideSpeedToAdd, myRigidBody.velocity.y);
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
            GetComponent<Rigidbody2D>().velocity = rygarJump;
            Jump();
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

            //Invoke("quickLookCompare()",.001f);
        }

    }


    private void quickLookCompare()
    {
        Vector2 direction;
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            direction = new Vector2(Mathf.Sign(myRigidBody.velocity.x), Mathf.Sign(myRigidBody.velocity.y));
            facingFixed = direction.x;
        }

    }


    private void quickLookCompareJump()
    {
        jumpingFixedY = myRigidBody.position.y;
        jumpingFixedX = myRigidBody.position.x;

    }
    //private void ClimbLadder()
    //{
    //    float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
    //    bool playerIsClimbing = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

    //    if (!myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Ladders")))
    //    {
    //        myAnimator.SetBool("climb", false);
    //        myRigidBody.gravityScale = gravityScaleAtStart;
    //        return;
    //    }

    //    if (controlThrow > .1f || controlThrow < -.1f)
    //    {
    //        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * need to readd this --> climbSpeed);
    //        myRigidBody.velocity = climbVelocity;
    //        myRigidBody.gravityScale = 0f;
    //        myAnimator.SetBool("climb", playerIsClimbing);
    //    }

    //}
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


// from decending jump
//float xFall = myRigidBody.velocity.x + facing * jumpSideSpeed * Time.deltaTime;
//float yFall = myRigidBody.velocity.y + Physics2D.gravity.y * (jumpFallSpeed - 1) * -Time.deltaTime;
//yFall = Mathf.Clamp(yFall, -.001f, -10f);

//myRigidBody.velocity += new Vector2(xFall, yFall);



// player needs to rotate to 146 degrees on y axis but not rigidbody on gameobject actual player prefab

//float jumpPressedRemembered1 = 0f;
//float jumpPressedRememberedTime1 = 1f;
//jumpPressedRemembered1 -= Time.deltaTime;

//if (CrossPlatformInputManager.GetButton("Jump"))
//{
//    Debug.Log("held down");
//}





// trying to add low jump

//float jumpPressedRemembered1 = 0f;
//float jumpPressedRememberedTime1 = 1f;
//jumpPressedRemembered1 -= Time.deltaTime;
//if (CrossPlatformInputManager.GetButtonUp("Jump"))
//{
//    jumpPressedRemembered1 = jumpPressedRememberedTime1;
//}

//if (CrossPlatformInputManager.GetButtonUp("Jump"))
//{
//    //myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * 5f * (lowJumpHeight - 1) * Time.deltaTime;

//    myRigidBody.velocity += Vector2.up * -200f;
//    Debug.Log("let go of  jump!");
//}
//Debug.Log(myRigidBody.velocity.y);

//if (myRigidBody.velocity.y >=0 && myRigidBody.position.x == jumpingFixedX)
//{
//    myRigidBody.velocity *= .5f;
//}
//Debug.Log(myRigidBody.velocity.x);






//if (jumpActive && !feetOnGround()) {
//    if (facingFixed != facing)
//    {
//        float xPos = Mathf.MoveTowards(10f, 5f, (turnDeceleration * Time.deltaTime));
//        myRigidBody.velocity = new Vector2(xPos * facing, myRigidBody.velocity.y);
//        //facingFixed = facing;
//    }
//}