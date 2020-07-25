using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class BetterJump : MonoBehaviour
{
    [SerializeField] float lowJump = .5f;

    Rigidbody2D myRigidBody;
    // Start is called before the first frame update
    void Start()
    {
        myRigidBody.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myRigidBody.velocity.y > 0 && !CrossPlatformInputManager.GetButton("Jump"))
        {
            myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * 5f * (lowJump - 1) * Time.deltaTime;
        }
    }
}
