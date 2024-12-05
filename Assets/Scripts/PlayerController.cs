using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Tracks which way the player is facing
    public enum FacingDirection
    {
        left, right
    }

    private Rigidbody2D rb;

    // Movement variables
    public float moveSpeed = 5f;
    public float apexHeight = 2f; // Max height the player can jump
    public float apexTime = 0.5f; // Time to reach peak height
    public float terminalSpeed = -5f; // Fastest downward speed

    // Time buffer for jumping after leaving a platform
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter = 0f;

    private FacingDirection lastFacingDirection = FacingDirection.right; // Keeps track of the last facing direction

    // For detecting if the player is on the ground
    public LayerMask groundLayer;
    public float groundDistance = 0.7f; // Distance to check for ground
    public Vector2 directionGround = Vector2.down;

    // Dash Variables
    public float dashSpeed = 15f; // How fast the dash is
    public float dashDuration = 0.2f; // How long the dash lasts
    public float dashCooldown = 1f; // Time before you can dash again
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    // Climbing variables
    public LayerMask wallLayer;
    public float wallDistance = 0.6f; // Distance to check for walls
    private bool isClimbing = false;

    // Magnet variables
    public Transform magnetObject; // Target for magnet effect
    public float magnetPullStrength = 40f; // How strong the pull is
    public float maxPullSpeed = 10f; // Max speed of the pull
    private bool isMagnetPulling = false;

    // For the visual effect of the magnet
    public ParticleSystem magnetParticles;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Make sure particles are off at the start
        magnetParticles.Stop();
    }

    void Update()
    {
        // Keep track of dash cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Start dash if conditions are met
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash());
        }

        // Normal movement if not dashing or climbing
        if (!isDashing && !isClimbing)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector2 playerInput = new Vector2(horizontalInput, 0);
            MovementUpdate(playerInput);
        }

        // Refresh jump buffer if grounded
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Handle wall climbing
        HandleClimbing();

        // Handle magnet functionality
        HandleMagneticPull();
    }

    // Handles basic movement and jumping
    public void MovementUpdate(Vector2 playerInput)
    {
        // Prevent moving horizontally while climbing
        if (isClimbing)
        {
            playerInput.x = 0;
        }

        // Stop movement into walls when not on the ground
        if (!IsGrounded() && Mathf.Abs(playerInput.x) > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(playerInput.x, 0), wallDistance, wallLayer);
            if (hit.collider != null)
            {
                playerInput.x = 0;
            }
        }

        // Apply movement
        rb.velocity = new Vector2(playerInput.x * moveSpeed, rb.velocity.y);

        // Jump logic
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || coyoteTimeCounter > 0)
            {
                float jumpVelocity = (2 * apexHeight) / apexTime; // How fast the jump launches the player
                rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                coyoteTimeCounter = 0;
            }
        }

        // Limit fall speed
        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }
    }

    // Checks if the player is moving
    public bool IsWalking()
    {
        return Mathf.Abs(rb.velocity.x) > 0.1f;
    }

    // Check if the player is touching the ground
    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, directionGround, groundDistance, groundLayer);
    }

    // Placeholder for death detection
    public bool IsDead()
    {
        return false;
    }

    // Dashing functionality
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

    // For checking if the player is climbing
    public bool IsClimbing()
    {
        return isClimbing;
    }

    // Handles wall climbing logic
    private void HandleClimbing()
    {
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.W))
        {
            // Sets a footLevel so character doesn't get stuck at top of block
            Vector2 footLevel = new Vector2(transform.position.x, transform.position.y - 0.6f);
            RaycastHit2D leftHit = Physics2D.Raycast(footLevel, Vector2.left, wallDistance, wallLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(footLevel, Vector2.right, wallDistance, wallLayer);

            if (leftHit.collider != null || rightHit.collider != null)
            {
                isClimbing = true;
                rb.gravityScale = 0f; // Turn off gravity
                rb.velocity = new Vector2(rb.velocity.x, moveSpeed); // Move upward
            }
            else
            {
                isClimbing = false;
                rb.gravityScale = 1f; // Turn gravity back on
            }
        }
        else
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }
    }

    // Manages the magnetic pull mechanic
    private void HandleMagneticPull()
    {
        if (Input.GetKey(KeyCode.E))
        {
            // Activate magnet pull if not already active
            if (!isMagnetPulling && magnetObject != null)
            {
                isMagnetPulling = true;
                rb.gravityScale = 0f; // Turn off gravity
                magnetParticles.Play();
            }

            // Apply pulling force towards the magnet
            if (isMagnetPulling && magnetObject != null)
            {
                Vector2 directionToMagnet = (magnetObject.position - transform.position).normalized;
                float distanceToMagnet = Vector2.Distance(transform.position, magnetObject.position);

                float pullForce = magnetPullStrength / Mathf.Pow(distanceToMagnet, 2); // Adjust force based on distance
                pullForce = Mathf.Clamp(pullForce, 0, maxPullSpeed);

                rb.AddForce(directionToMagnet * pullForce, ForceMode2D.Force);
            }
        }
        else
        {
            // Deactivate magnet pull
            if (isMagnetPulling)
            {
                isMagnetPulling = false;
                rb.gravityScale = 1f; // Turn gravity back on
                magnetParticles.Stop();
            }
        }
    }

    // Determines which way the player is facing
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
}
