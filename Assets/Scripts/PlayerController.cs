using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    private Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float apexHeight = 2f;
    public float apexTime = 0.5f;
    private bool isJumping = false;
    public float terminalSpeed = -10f;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 playerInput = new Vector2(horizontalInput, 0);
        MovementUpdate(playerInput);

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    public void MovementUpdate(Vector2 playerInput)
    {
        rb.velocity = new Vector2(playerInput.x * moveSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || coyoteTimeCounter > 0)
            {
                float jumpVelocity = (2 * apexHeight) / apexTime;
                rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                isJumping = true;
                coyoteTimeCounter = 0;
            }
        }

        if (isJumping && rb.velocity.y <= 0)
        {
            rb.gravityScale = 1;
            isJumping = false;
        }

        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }
    }

    public bool IsWalking()
    {
        return Mathf.Abs(rb.velocity.x) > 0.1f;
    }

    public bool IsGrounded()
    {
        return Mathf.Abs(rb.velocity.y) < 0.01f;
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
