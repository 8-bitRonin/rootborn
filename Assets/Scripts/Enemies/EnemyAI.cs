using Pathfinding;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activationDistance = 50f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 200f;
    public float nextWaypointRequirement = 0.8f;
    public float jumpNodeHeightRequirement = 0.3f;
    public float jumpModifier = 0.3f;
    public float jumpCheckOffset = 0.1f;

    [Header("Custom Behaviour")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionalLookEnabled = true;

    private Path path;
    private int currentWaypoint = 0;
    bool isGrounded = false;
    Seeker seeker;
    Rigidbody2D rb;
    Health targetHealth;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        if (target != null)
        {
            targetHealth = target.GetComponent<Health>();
        }

        InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        if (target == null || targetHealth == null)
        {
            StopMovement();
            return;
        }
        if (targetHealth.currentHealth <= 0)
        {
            StopMovement();
            return;
        }
        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
        else
        {
            StopMovement();
        }
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdatePath()
    {
        if (target == null || targetHealth == null) return;
        if (targetHealth.currentHealth <= 0) return;

        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        if (path == null) return;

        // Reached end
        if (currentWaypoint >= path.vectorPath.Count) return;

        // Ground check
        isGrounded = Physics2D.Raycast(transform.position, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);

        // Movement direction
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        // Jump logic
        if (jumpEnabled && isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * speed * jumpModifier);
            }
        }

        // Apply force
        rb.AddForce(force);

        // Next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointRequirement)
        {
            currentWaypoint++;
        }

        // Flip sprite
        if (directionalLookEnabled)
        {
            if (rb.linearVelocity.x > 0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.linearVelocity.x < -0.05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.position) < activationDistance;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
