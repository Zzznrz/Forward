using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Forgiving auto-runner controller for the reference prototype.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public event System.Action Died;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float steeringSpeed = 3f;
    [SerializeField] private float speedChange = 25f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;

    [Header("Death")]
    [SerializeField] private float deathY = -8f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 standingSize;
    private Vector2 standingOffset;
    private float normalGravityScale;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool grounded;
    private bool sliding;
    private bool dead;
    private bool completed;
    private bool menuPaused;
    private bool jumpCutApplied;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();
    private Transform visualRoot;
    private Vector3 visualRestScale;
    private Vector3 visualRestPosition;
    private Vector3 respawnPosition;

    // Cached components and the player's initial physics values.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        standingSize = boxCollider.size;
        standingOffset = boxCollider.offset;
        normalGravityScale = rb.gravityScale;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        respawnPosition = transform.position;
        ConfigureVisualRoot();

        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("PlayerNoFriction")
        {
            friction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine2D.Minimum,
            bounceCombine = PhysicsMaterialCombine2D.Minimum
        };
        boxCollider.sharedMaterial = noFriction;
    }

    private void Update()
    {
        if (dead || completed || menuPaused || Keyboard.current == null)
            return;

        if (transform.position.y < deathY)
        {
            Die();
            return;
        }

        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpCutApplied = false;
        }

        if (!Keyboard.current.spaceKey.isPressed && !Keyboard.current.upArrowKey.isPressed &&
            rb.linearVelocity.y > 0f && !jumpCutApplied)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            jumpCutApplied = true;
        }

        bool wantsSlide = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        if (wantsSlide && grounded && !sliding)
            StartSlide();
        else if (!wantsSlide && sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (dead || completed || menuPaused)
            return;

        rb.gravityScale = rb.linearVelocity.y < 0f
            ? normalGravityScale * fallGravityMultiplier
            : normalGravityScale;

        float steering = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                steering -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                steering += 1f;
        }

        // Default is a steady run to the right; steering helps correct a landing.
        // Auto-run is always the base speed. Steering is only available in air,
        // so A/D changes the landing point without stopping the run on the ground.
        float targetSpeed = speed;
        if (!grounded && Mathf.Abs(steering) > 0.01f)
            targetSpeed = Mathf.Max(0.5f, speed + steering * steeringSpeed);

        float horizontalSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            speedChange * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !sliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            grounded = false;
            jumpCutApplied = false;
            AudioManager.GetOrCreate().PlayJump();
        }
    }

    private void StartSlide()
    {
        sliding = true;
        AudioManager.GetOrCreate().PlaySlide();
        boxCollider.size = new Vector2(standingSize.x, standingSize.y * 0.5f);
        boxCollider.offset = new Vector2(standingOffset.x, standingOffset.y - standingSize.y * 0.25f);
        if (visualRoot != null)
        {
            visualRoot.localScale = new Vector3(visualRestScale.x, visualRestScale.y * 0.5f, visualRestScale.z);
            visualRoot.localPosition = visualRestPosition + Vector3.down * standingSize.y * 0.25f;
        }
    }

    private void StopSlide()
    {
        sliding = false;
        boxCollider.size = standingSize;
        boxCollider.offset = standingOffset;
        if (visualRoot != null)
        {
            visualRoot.localScale = visualRestScale;
            visualRoot.localPosition = visualRestPosition;
        }
    }

    private void ConfigureVisualRoot()
    {
        visualRoot = transform.Find("Visual");
        if (visualRoot == null)
        {
            GameObject visualObject = new GameObject("Visual");
            visualRoot = visualObject.transform;
            visualRoot.SetParent(transform, false);

            SpriteRenderer sourceRenderer = GetComponent<SpriteRenderer>();
            if (sourceRenderer != null)
            {
                SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
                visualRenderer.sprite = sourceRenderer.sprite;
                visualRenderer.color = sourceRenderer.color;
                visualRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                visualRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                visualRenderer.sortingOrder = sourceRenderer.sortingOrder;
                sourceRenderer.enabled = false;
            }

            // Face parts created by the scene bootstrap belong to the visual group.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != visualRoot)
                    child.SetParent(visualRoot, true);
            }
        }

        visualRestScale = visualRoot.localScale;
        visualRestPosition = visualRoot.localPosition;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard"))
        {
            Die();
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.65f)
            {
                groundColliders.Add(collision.collider);
                grounded = true;
                coyoteTimer = coyoteTime;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
        grounded = groundColliders.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckpointZone checkpoint = other.GetComponent<CheckpointZone>();
        if (checkpoint != null)
            respawnPosition = checkpoint.RespawnPosition;
        else if (other.GetComponent<FinishZone>() != null)
            CompleteLevel();
        else if (other.CompareTag("Hazard"))
            Die();
    }

    public void CompleteLevel()
    {
        if (dead || completed)
            return;

        completed = true;
        AudioManager.GetOrCreate().PlayFinish();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        if (sliding)
            StopSlide();
    }

    public void SetMenuPaused(bool paused)
    {
        menuPaused = paused;
        if (paused)
            rb.linearVelocity = Vector2.zero;
    }

    public void ResetForLevel(Vector3 startPosition)
    {
        CancelInvoke();
        transform.position = startPosition;
        respawnPosition = startPosition;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = normalGravityScale;
        grounded = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        groundColliders.Clear();
        dead = false;
        completed = false;
        if (sliding)
            StopSlide();
    }

    private void Die()
    {
        if (dead || completed)
            return;

        dead = true;
        AudioManager.GetOrCreate().PlayDeath();
        Died?.Invoke();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Invoke(nameof(Respawn), 0.2f);
    }

    private void Respawn()
    {
        transform.position = respawnPosition;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = normalGravityScale;
        grounded = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        groundColliders.Clear();
        dead = false;
        completed = false;
        if (sliding)
            StopSlide();
    }
}
