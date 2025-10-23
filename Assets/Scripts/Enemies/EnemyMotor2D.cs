using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyMotor2D : MonoBehaviour
{
    [Header("Config & Visuals")]
    public EnemyConfig config;
    public Transform gfxRoot;

    [Header("Navigation")]
    public LayerMask obstacleMask;   // walls/ledges
    public bool autoFlip = false;    // leave false; Brain handles facing

    Rigidbody2D rb;
    Collider2D col;

    Vector2 desiredVel;
    Vector2 steerDir;

    // Grounding & jump
    bool grounded;
    float lastGroundedTime;
    float lastJumpTime;
    bool jumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        UpdateGrounded();
    }

    // --- Movement ------------------------------------------------------------

    public void MoveTowards(Vector2 worldTarget, float speed, float dt)
    {
        if (!config) return;

        Vector2 pos = rb.position;
        Vector2 dir = worldTarget - pos;
        if (dir.sqrMagnitude < 0.0001f) { desiredVel = Vector2.zero; return; }

        dir.Normalize();
        dir = AvoidObstacles(dir);

        // smooth steering
        steerDir = Vector2.Lerp(steerDir, dir, config.turnSharpness * dt).normalized;

#if UNITY_6000_0_OR_NEWER
        Vector2 newVel = Vector2.MoveTowards(rb.linearVelocity, steerDir * speed, config.acceleration * dt);
        newVel.y = rb.linearVelocity.y; // preserve gravity / jump
        rb.linearVelocity = newVel;
#else
        Vector2 newVel = Vector2.MoveTowards(rb.velocity, steerDir * speed, config.acceleration * dt);
        newVel.y = rb.velocity.y;
        rb.velocity = newVel;
#endif

        if (autoFlip && gfxRoot)
        {
            bool facingRight = steerDir.x >= 0f;
            var s = gfxRoot.localScale;
            s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
            gfxRoot.localScale = s;
        }
    }

    public void StopX(float dt)
    {
#if UNITY_6000_0_OR_NEWER
        var v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, 0f, (config ? config.acceleration : 20f) * dt);
        rb.linearVelocity = v;
#else
        var v = rb.velocity;
        v.x = Mathf.MoveTowards(v.x, 0f, (config ? config.acceleration : 20f) * dt);
        rb.velocity = v;
#endif
    }

    // --- Jumping -------------------------------------------------------------
    public bool TryJumpToward(Vector2 target)
    {
        if (!config || !config.canJump) return false;

        bool canCoyote = (Time.time - lastGroundedTime) <= config.coyoteTime;
        bool canCooldown = (Time.time - lastJumpTime) >= config.jumpCooldown;
        if (!canCooldown || jumping || !(grounded || canCoyote))
            return false;

        // Determine horizontal direction and distance to target
        Vector2 toTarget = target - rb.position;
        float horizontalDist = Mathf.Abs(toTarget.x);
        float verticalDiff = target.y - rb.position.y;
        float facingSign = Mathf.Sign(steerDir.x == 0f ? toTarget.x : steerDir.x);

        bool targetInFront = Mathf.Sign(toTarget.x) == facingSign || Mathf.Approximately(toTarget.x, 0f);

        // Evaluate conditions more permissively
        // Trigger a jump when approaching the edge or wall before we're directly under it
        bool targetHigher = targetInFront &&
                            verticalDiff > config.targetHeightJumpThreshold &&
                            horizontalDist < 2.5f;

        bool gapAhead = targetInFront && GapAhead();
        bool lowWall = targetInFront && LowWallAhead();
        bool ledgeAbove = targetInFront && HasLedgeAbove();   // NEW

        if (!(targetHigher || gapAhead || lowWall || ledgeAbove))
            return false;


        DoJump();
        return true;
    }


    void DoJump()
    {
#if UNITY_6000_0_OR_NEWER
        var v = rb.linearVelocity; v.y = config.jumpForce; rb.linearVelocity = v;
#else
    var v = rb.velocity; v.y = config.jumpForce; rb.velocity = v;
#endif
        lastJumpTime = Time.time;
        jumping = true;
        StartCoroutine(EndJumpFlag());
    }
    IEnumerator EndJumpFlag()
    {
        yield return new WaitForSeconds(0.25f); // short lockout
        jumping = false;
    }


    // --- Probes --------------------------------------------------------------

    void UpdateGrounded()
    {
        if (!config) { grounded = false; return; }

        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.02f);
        float radius = Mathf.Max(0.06f, b.extents.x * 0.6f);
        grounded = Physics2D.OverlapCircle(origin, radius, config.groundMask);

        if (grounded) lastGroundedTime = Time.time;
    }

    bool GapAhead()
    {
        // cast slightly ahead & down to see if ground continues
        float dir = Mathf.Sign(steerDir.x == 0f ? 1f : steerDir.x);
        Vector2 start = (Vector2)col.bounds.center + new Vector2(dir * config.jumpProbeDistance, 0.1f);
        float drop = col.bounds.extents.y + 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, drop, config.groundMask);
        return !hit; // no ground where we’re headed
    }

    bool LowWallAhead()
    {
        float dir = Mathf.Sign(steerDir.x == 0f ? 1f : steerDir.x);
        Vector2 basePos = (Vector2)col.bounds.center;
        float forward = col.bounds.extents.x + 0.1f;

        // probe at low knee height
        Vector2 pLow = basePos + new Vector2(dir * forward, config.stepProbeHeight);
        RaycastHit2D hitLow = Physics2D.Raycast(pLow, new Vector2(dir, 0f), 0.1f, obstacleMask);

        return hitLow;
    }
    bool HasLedgeAbove()
    {
        if (!config || !config.canJump) return false;

        // direction we’re moving/facing
        float dir = Mathf.Sign(steerDir.x == 0f ? 1f : steerDir.x);
        Vector2 start = (Vector2)col.bounds.center + new Vector2(dir * 0.2f, 0.2f);

        // Aim the ray diagonally upward
        Vector2 rayDir = new Vector2(dir, 1f).normalized;
        float rayLength = config.jumpProbeDistance * 6.5f; // longer reach for upper platforms

        RaycastHit2D hit = Physics2D.Raycast(start, rayDir, rayLength, config.groundMask);

        if (hit.collider)
        {
            // check that the ledge is within jumpable height
            float heightDiff = hit.point.y - rb.position.y;
            return heightDiff > 0.5f && heightDiff < config.jumpForce * 0.5f; // roughly half jump arc
        }

        return false;
    }

    // --- Steering avoidance --------------------------------------------------

    Vector2 AvoidObstacles(Vector2 dir)
    {
        Vector2 origin = rb.position + Vector2.up * 0.2f;
        const float probe = 0.8f;

        if (!Physics2D.Raycast(origin, dir, probe, obstacleMask)) return dir;

        // try slight angles up/down
        Vector2 a = (Quaternion.Euler(0, 0, 25f) * dir);
        Vector2 b = (Quaternion.Euler(0, 0, -25f) * dir);
        if (!Physics2D.Raycast(origin, a, probe, obstacleMask)) return a.normalized;
        if (!Physics2D.Raycast(origin, b, probe, obstacleMask)) return b.normalized;

        // slide along surface normal
        var hit = Physics2D.Raycast(origin, dir, probe, obstacleMask);
        if (hit)
        {
            float sign = Mathf.Sign(Vector2.SignedAngle(Vector2.right, dir));
            return Vector2.Perpendicular(hit.normal).normalized * sign;
        }
        return dir;
    }

    // --- Exposed helpers -----------------------------------------------------

    public float GetVelX()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity.x;
#else
        return rb.velocity.x;
#endif
    }

    public bool IsGrounded() => grounded;
    void OnDrawGizmosSelected()
    {
        if (!col || !config) return;
        float dir = Mathf.Sign(steerDir.x == 0f ? 1f : steerDir.x);
        Vector2 start = (Vector2)col.bounds.center + new Vector2(dir * 0.2f, 0.2f);
        Vector2 rayDir = new Vector2(dir, 1f).normalized;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(start, start + rayDir * (config.jumpProbeDistance * 2f));
    }


}
