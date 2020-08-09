using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    Rigidbody2D myRigidBody;
    BoxCollider2D myBoxCollider;
    PolygonCollider2D myPoly;



    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsFacingRight())
        {
            myRigidBody.velocity = new Vector2(-moveSpeed, 0f);

        } else
        {
            myRigidBody.velocity = new Vector2(moveSpeed, 0f);

        }
  
    }

    bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }


    // when collider exits collision with whatever. In this case, the ground.

    private void OnTriggerExit2D(Collider2D ting)
    {
        if (myBoxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) == false)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(-myRigidBody.velocity.x)), 1f);

        }

    }

}
