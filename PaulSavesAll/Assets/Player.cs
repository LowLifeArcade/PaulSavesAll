using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour
    
{

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

    //Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 10f;
    [SerializeField] float jumpPressedRememberedTime = 0.3f;
    [SerializeField] Vector2 deathKick = new Vector2(25f, 25f);
    

    //State
    bool isAlive = true;

    // Cached componet references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    BoxCollider2D myBodyCollider2D;
    BoxCollider2D myFeet;
    float jumpPressedRemembered = 0f;

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

    
    void Update()
    {
        if (!isAlive) { return; }
        Run();
        ClimbLadder();
        Jump();
        FlipSprite();
        Die();
        
    }

    private void Run()
    {

        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // value is between -1 to +1
        Vector2 playerVelocity = new Vector2(controlThrow * runSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;
        //print(playerVelocity);

        // transition code I still need to fully understand what this first line is doing
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("running", playerHasHorizontalSpeed);

    }

    private void ClimbLadder()
    {

        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        //bool verticalInput = myRigidBody.velocity.y <= (controlThrow);
        bool playerIsClimbing = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

        //&& !myFeet.IsTouchingLayers(LayerMask.GetMask("Ladders"))
        if (!myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Ladders")) )
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
        //myRigidBody.velocity = Vector3.zero;
        //myRigidBody.angularVelocity = Vector3.zero;


        /*bool playerIsClimbing = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

        if (playerIsClimbing)
        {
            myAnimator.SetBool("climb", playerIsClimbing);
        } */



    }

    /*if (CrossPlatformInputManager.GetButtonDown("Vertical"))
    {
        Vector2 climbLadder = new Vector2(0f, climbSpeed);
        myRigidBody.velocity += climbLadder;
    }*/



    //I tried to make it so if you press jump a little befoer hitting ground you would jump. I failed.
    /* private void Jump()
    {

        fJumpPressedRemembered -= Time.deltaTime;

       
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            fJumpPressedRemembered = fJumpPressedRememberedTime;
        }

        if ((fJumpPressedRemembered > 0) && !myFeet.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }

        Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
        myRigidBody.velocity += jumpVelocityToAdd;
    } */

    /* private void Jump()
    {
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidBody.gravityScale = 0f;
            myRigidBody.gravityScale = gravityScaleAtStart;

            return;
        }
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            // myRigidBody.gravityScale = 0f;
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd;
        }
    } */

    // most recent where i fixed wall jumps
    /* private void Jump()
    {
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            //myRigidBody.gravityScale = 0f;
            // myRigidBody.gravityScale = gravityScaleAtStart;

            return;
        }
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            // myRigidBody.gravityScale = 0f;
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd - myRigidBody.velocity;
        }
    } */

    private void Jump()
    {

        // added this so that wall jump didn't just go erratic by adding jumps buffered
        // this also creates a cool mechanic where you can only wall jump when decending
        bool jumpActive = false;
        if (myRigidBody.velocity.y >= 0.01f)
        {
            jumpActive = true;
        }
        else
        {
            jumpActive = false;
        }

        // if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground"))) {return}
        // was in if statement after jumpressed 
        if (myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")) && jumpActive) { return; }

        jumpPressedRemembered -= Time.deltaTime;
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            jumpPressedRemembered = jumpPressedRememberedTime;
        }

        if (jumpPressedRemembered > 0 && myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            //myRigidBody.gravityScale = 0f;
            // myRigidBody.gravityScale = gravityScaleAtStart;
            // myRigidBody.gravityScale = 0f;
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd - myRigidBody.velocity;


            // this hangs unity or not. I just tried it and it freezes even without this code
            // yup just checked again and it hangs it. Something about it being open too.

            /*
           bool playerIsJumping;
            while (playerIsJumping = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon)
            {
                myAnimator.SetBool("jump", playerIsJumping);

            }
            myAnimator.SetBool("jump", false);
            */


        }
        // StartCoroutine(pause());

        // make iEnum that waits a little bit before you can jump again because wall jumping gets erra


    }

    /*IEnumerator pause()
    {
        yield return new WaitForSeconds(0.3f);
    } */


    // accidently made something that bounces off of frog. Kidna cool.
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

    private void Die()
    {
        if (myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("enemy")))
        {
            print("has died");
           
            myAnimator.SetTrigger("hurt");
            GetComponent<Rigidbody2D>().velocity = deathKick;
        }
    }


    private void FlipSprite()
    {
        // if player is moving

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);

            //flip axis
        }
    }
    
}
 