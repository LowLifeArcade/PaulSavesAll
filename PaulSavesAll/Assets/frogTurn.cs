using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frogTurn : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;

    Rigidbody2D myRigidBody;
    BoxCollider2D myBoxCollider;
    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        void Update()
        {
            if (IsFacingRight())
            {
                myRigidBody.velocity = new Vector2(-moveSpeed, 0f);

            }
            else
            {
                myRigidBody.velocity = new Vector2(moveSpeed, 0f);

            }

        }

    }

    bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }

    void OnTriggerExit2D(Collider2D myBoxCollider)
    {
        if (myBoxCollider.IsTouchingLayers(LayerMask.GetMask("ground")) == false)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(-myRigidBody.velocity.x)), 1f);

        }

    }
}
