using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    public float moveSpeed = 5f;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 playerInput = new Vector2(horizontalInput, 0);
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        rb.velocity = new Vector2(playerInput.x * moveSpeed, rb.velocity.y);
    }

    public bool IsWalking()
    {
        return Mathf.Abs(rb.velocity.x) > 0.1f;
    }
    public bool IsGrounded()
    {
        return Mathf.Abs(rb.velocity.y) < 0.1f;
    }

    public FacingDirection GetFacingDirection()
    {
        if (rb.velocity.x > 0)
        {
            return FacingDirection.right;
            
        }
        if (rb.velocity.x < 0)
        {
            return FacingDirection.left;
        }
        
        return FacingDirection.right;
    }
}

