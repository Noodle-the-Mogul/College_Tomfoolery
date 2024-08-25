using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Goatscript : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    public NavMeshAgent agent;
    public float fovAngle = 90f;
    public float cooldownDuration = 5f; // Cooldown duration in seconds
    public float waitDuration = 2f; // Wait duration in seconds after cooldown
    private bool isChasing = false;
    private bool initialized = false; // Flag to track if NPC has been initialized
    private Vector2 lastKnownPlayerPosition;
    private float cooldownTimer = 0f;
    private float waitTimer = 0f; // Timer for the wait duration
    private Vector2 lastPlayerPositionDuringCooldown;
    public int segments = 10; // Number of segments to draw the field of view
    private Vector3 startingPosition; // NPC's starting position
    public AudioClip detectionSoundEffect; // Add this variable to hold the detection sound effect
    private AudioSource audioSource; // Reference to the AudioSource component

    public string sceneToLoad; // Name of the scene to load
    public GameObject spriteToDetect; // Reference to the sprite to detect
    
    private bool hasTriggered = false;

    public Rigidbody2D rb;
    public Animator animator;
    private bool isMoving;
    private Vector2 previousPosition;
    private Vector2 moveInput;

    void Start()
    {
        startingPosition = transform.position; // Store the starting position
        audioSource = GetComponentInChildren<AudioSource>();
        initialized = true; // NPC is initialized
        previousPosition = rb.position;
    }

    private void Update()
    {
        if (!initialized) return; // Don't execute if NPC is not initialized

        agent.transform.rotation = Quaternion.identity;
        if (isChasing)
        {
            if (PlayerInFieldOfView())
            {
                lastKnownPlayerPosition = player.position;
                agent.SetDestination(player.position);
                lastPlayerPositionDuringCooldown = player.position; // Update last player position during cooldown
                cooldownTimer = cooldownDuration; // Reset cooldown timer while player is in sight
                waitTimer = 0f; // Reset wait timer
            }
            else
            {
                if (cooldownTimer <= 0f)
                {
                    if (waitTimer >= waitDuration)
                    {
                        lastKnownPlayerPosition = lastPlayerPositionDuringCooldown; // Update last known position to the last received position during cooldown
                        agent.SetDestination(startingPosition); // Set destination to starting position
                    }
                    else
                    {
                        waitTimer += Time.deltaTime;
                    }
                }
                else
                {
                    cooldownTimer -= Time.deltaTime;
                    agent.SetDestination(player.position); // Move towards player's actual position during cooldown
                    lastPlayerPositionDuringCooldown = player.position; // Update last player position during cooldown
                    waitTimer = 0f; // Reset wait timer
                }
            }
        }
        else if (PlayerInDetectionRadius() && PlayerInFieldOfView())
        {
            Debug.Log("Player detected!");
            isChasing = true;
            lastKnownPlayerPosition = player.position;
            if (detectionSoundEffect != null && audioSource != null)
            {
                audioSource.PlayOneShot(detectionSoundEffect); // Play audio every time NPC detects the player
            }
        }
        Vector2 movement = rb.position - previousPosition;

        moveInput=movement.normalized;

        
        
        UpdateAnimatorParameters();
        
            if (moveInput.y <0)
            {
                
                animator.SetInteger("Direction", 3);
            }
            else if (moveInput.y> 0)
            {
                
                animator.SetInteger("Direction", 2);
            }

            if (moveInput.x >0)
            {
                
                animator.SetInteger("Direction", 1);
            }
            else if (moveInput.x <0)
            {
                
                animator.SetInteger("Direction", 0);
            }
            previousPosition = rb.position;

        
        
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        // Check if the other collider belongs to the specified sprite
        if (other.gameObject == spriteToDetect && !hasTriggered)
        {   Debug.Log("collided");
            StartCoroutine(StartSceneLoadCoroutine());
            //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            hasTriggered = true; // Mark as triggered
        }
    }
    private IEnumerator StartSceneLoadCoroutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f); // Wait for 1 second in real time
        Time.timeScale = 0.3f; // Resume normal time scale
        SceneManager.LoadScene(sceneToLoad); // Load the specified scene // Load the specified scene
    }
    void UpdateAnimatorParameters()
    {
        isMoving = moveInput.magnitude > 0f;

        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
    }
    

    private bool PlayerInDetectionRadius()
    {
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    private bool PlayerInFieldOfView()
    {
        if (agent.velocity.magnitude == 0)
        {
            // If NPC is idle and has no velocity, calculate FOV direction based on sprite direction
            Vector2 directionToPlayer = player.position - transform.position;
            Vector2 npcDirection = -transform.right; // Get the NPC's right direction (assuming sprite faces right)
            float angleToPlayer = Vector2.Angle(npcDirection, directionToPlayer);

            if (angleToPlayer <= fovAngle * 0.5f) // Change the angle as needed
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, playerLayer);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // If NPC is moving, calculate FOV based on its velocity direction
            Vector2 directionToPlayer = player.position - (Vector3)transform.position;
            Vector2 npcDirection = agent.velocity.normalized; // Get the normalized direction of the NPC's velocity
            float angleToPlayer = Vector2.Angle(npcDirection, directionToPlayer);

            if (angleToPlayer <= fovAngle * 0.5f) // Change the angle as needed
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, playerLayer);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw the field of view
        DrawFieldOfView();
    }

    private void DrawFieldOfView()
    {
        if (agent.velocity.magnitude == 0)
        {
            // If NPC is idle and has no velocity, draw FOV based on sprite direction
            Vector2 npcDirection = -transform.right; // Get the NPC's right direction (assuming sprite faces right)
            float halfAngle = fovAngle * 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = -halfAngle + (i * fovAngle / segments);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * npcDirection;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * detectionRadius);
            }
        }
        else
        {
            // If NPC is moving, draw FOV based on its velocity direction
            Vector2 npcDirection = agent.velocity.normalized;
            float halfAngle = fovAngle * 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = -halfAngle + (i * fovAngle / segments);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * npcDirection;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * detectionRadius);
            }
        }
    }
}