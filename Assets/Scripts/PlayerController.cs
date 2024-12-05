using System.Collections;
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
    public float terminalSpeed = -5f;

    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter = 0f;

    private FacingDirection lastFacingDirection = FacingDirection.right;

    public LayerMask groundLayer;
    public float groundDistance = 0.7f;
    public Vector2 directionGround = Vector2.down;

    public LayerMask wallLayer;
    public float wallDistance = 0.6f;
    private bool isClimbing = false;


    public Transform magnetObject;
    public float magneticPullStrength = 10f;
    public float maxPullSpeed = 10f;
    private bool isMagneticPulling = false;

    public ParticleSystem magneticParticles;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        magneticParticles.Stop();
    }

    void Update()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash());
        }

        if (!isDashing && !isClimbing)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector2 playerInput = new Vector2(horizontalInput, 0);
            MovementUpdate(playerInput);
        }

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        HandleClimbing();

        HandleMagneticPull();
    }

    public void MovementUpdate(Vector2 playerInput)
    {
        if (isClimbing)
        {
            playerInput.x = 0;
        }

        if (!IsGrounded() && Mathf.Abs(playerInput.x) > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(playerInput.x, 0), wallDistance, wallLayer);
            if (hit.collider != null)
            {
                playerInput.x = 0;
            }
        }

        rb.velocity = new Vector2(playerInput.x * moveSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || coyoteTimeCounter > 0)
            {
                float jumpVelocity = (2 * apexHeight) / apexTime;
                rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                coyoteTimeCounter = 0;
            }
        }

        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }
    }

    private void HandleClimbing()
    {
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.W))
        {
            Vector2 footLevel = new Vector2(transform.position.x,transform.position.y - 0.6f);
            RaycastHit2D leftHit = Physics2D.Raycast(footLevel, Vector2.left, wallDistance, wallLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(footLevel, Vector2.right, wallDistance, wallLayer);

            if (leftHit.collider != null || rightHit.collider != null)
            {
                isClimbing = true;
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(rb.velocity.x, moveSpeed);
            }
            else
            {
                isClimbing = false;
                rb.gravityScale = 1f;
            }
        }
        else
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }
    }


    private IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection = (GetFacingDirection() == FacingDirection.right) ? Vector2.right : Vector2.left;
        Vector2 originalVelocity = rb.velocity;
        rb.velocity = new Vector2(dashDirection.x * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = new Vector2(originalVelocity.x, rb.velocity.y);
        isDashing = false;
    }

    public bool IsWalking()
    {
        return Mathf.Abs(rb.velocity.x) > 0.1f;
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, directionGround, groundDistance, groundLayer);
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }

    public bool IsDead()
    {
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        if (rb.velocity.x > 0)
        {
            lastFacingDirection = FacingDirection.right;
            return FacingDirection.right;
        }
        if (rb.velocity.x < 0)
        {
            lastFacingDirection = FacingDirection.left;
            return FacingDirection.left;
        }
        return lastFacingDirection;
    }

    private void HandleMagneticPull()
    {
        if (Input.GetKey(KeyCode.E))
        {
            if (!isMagneticPulling && magnetObject != null)
            {
                isMagneticPulling = true;
                rb.gravityScale = 0f;
                magneticParticles.Play();
            }

            if (isMagneticPulling && magnetObject != null)
            {
                Vector2 directionToMagnet = (magnetObject.position - transform.position).normalized;
                float distanceToMagnet = Vector2.Distance(transform.position, magnetObject.position);

                float pullForce = magneticPullStrength / Mathf.Pow(distanceToMagnet, 2);
                pullForce = Mathf.Clamp(pullForce, 0, maxPullSpeed);

                rb.AddForce(directionToMagnet * pullForce, ForceMode2D.Force);
            }
        }
        else
        {
            if (isMagneticPulling)
            {
                isMagneticPulling = false;
                rb.gravityScale = 1f;
                magneticParticles.Stop();
            }
        }
    }
}
