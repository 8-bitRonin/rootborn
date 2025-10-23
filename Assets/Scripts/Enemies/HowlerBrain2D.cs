using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMotor2D))]
public class HowlerBrain2D : MonoBehaviour, IDamageable
{
    enum State { Idle, Patrol, Chase, Lunge, Stagger, Dead }

    [Header("Refs")]
    [SerializeField] Transform target;                // auto-found by tag if null
    [SerializeField] Animator anim;                   // child animator
    [SerializeField] SpriteRenderer sprite;           // optional tint
    [SerializeField] MeleeLunge lunge;                // same object
    [SerializeField] EnemyMotor2D motor;              // same object
    [SerializeField] bool artFacesRight = true;

    [Header("Patrol Waypoints (optional)")]
    [SerializeField] Transform waypointA;
    [SerializeField] Transform waypointB;

    // Patrol internals
    Vector2 patrolStart;
    Vector2 patrolTarget;
    bool patrolForward = true;
    float patrolWaitTimer;

    // State/health
    State state = State.Idle;
    float hp;
    float nextAttackTime;

    EnemyConfig C => motor ? motor.config : null;     // shorthand
    public bool IsAlive => state != State.Dead;

    void Awake()
    {
        if (!motor) motor = GetComponent<EnemyMotor2D>();
        if (!lunge) lunge = GetComponent<MeleeLunge>();
        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        hp = (C ? C.maxHP : 30f);

        // Default patrol = spawn ± distance
        patrolStart = transform.position;
        patrolTarget = patrolStart + Vector2.right * (C ? C.patrolDistance : 4f);
    }

    void Update()
    {
        if (!IsAlive || !C) return;

        float dist = target ? Vector2.Distance(transform.position, target.position) : Mathf.Infinity;
        bool hasLOS = target && HasLineOfSight();

        // Decide which point to face this frame (used by FaceByState below)
        Vector2 faceTarget = transform.position;

        switch (state)
        {
            case State.Idle:
                anim.SetFloat("Speed", 0f);

                if (C.patrols) state = State.Patrol;
                if (target && dist <= C.aggroRange && hasLOS) state = State.Chase;

                faceTarget = (Vector2)transform.position + new Vector2(Mathf.Sign(GetVelX()), 0f);
                break;

            case State.Patrol:
                {
                    Vector2 a = waypointA ? (Vector2)waypointA.position : patrolStart;
                    Vector2 b = waypointB ? (Vector2)waypointB.position : patrolTarget;
                    Vector2 targetPos = patrolForward ? b : a;

                    // Move along patrol path
                    if (motor.TryJumpToward(targetPos))
                    {
                        {
                            anim.SetTrigger("Jump");
                        }
                        ;
                    }
                    ;
                    motor.MoveTowards(targetPos, C.patrolSpeed, Time.deltaTime);
                    anim.SetFloat("Speed", Mathf.Abs(GetVelX()));

                    // Arrival logic (robust radius + clean wait)
                    const float arriveRadius = 0.35f;
                    if (Vector2.SqrMagnitude((Vector2)transform.position - targetPos) <= arriveRadius * arriveRadius)
                    {
                        motor.StopX(Time.deltaTime);
                        anim.SetFloat("Speed", 0f);

                        patrolWaitTimer += Time.deltaTime;
                        if (patrolWaitTimer >= C.patrolWait)
                        {
                            patrolForward = !patrolForward;
                            patrolWaitTimer = 0f;
                        }
                    }
                    else
                    {
                        patrolWaitTimer = 0f;
                    }

                    // Player spotted?
                    if (target && dist <= C.aggroRange && hasLOS)
                        state = State.Chase;

                    faceTarget = targetPos;
                    break;
                }

            case State.Chase:
                {
                    float speed = dist > 2.2f ? C.runSpeed : C.walkSpeed;
                    motor.MoveTowards(target.position, speed, Time.deltaTime);
                    if (motor.TryJumpToward(target.position))
                    {
                        anim.SetTrigger("Jump");
                    };
                    anim.SetFloat("Speed", Mathf.Abs(GetVelX()));

                    // Attack gate
                    if (Time.time >= nextAttackTime && dist <= C.attackRange && hasLOS && !lunge.IsLunging)
                        StartCoroutine(DoLunge());

                    // Lose target → return to patrol/idle
                    if ((dist > C.deAggroRange || !hasLOS))
                        state = C.patrols ? State.Patrol : State.Idle;

                    faceTarget = target.position;
                    break;
                }

            case State.Lunge:
            case State.Stagger:
            case State.Dead:
                // handled elsewhere
                faceTarget = target ? (Vector2)target.position : (Vector2)transform.position;
                break;
        }

        FaceByState(faceTarget);
    }

    IEnumerator DoLunge()
    {
        state = State.Lunge;

        // prep
        motor.StopX(Time.deltaTime);

        // pass config timings if present (keeps MeleeLunge reusable)
        if (C)
        {
            var t = lunge.GetType();
            t.GetField("windupTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
             ?.SetValue(lunge, C.windupTime);
            t.GetField("cooldown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
             ?.SetValue(lunge, C.attackCooldown);
        }

        float dir = Mathf.Sign(target.position.x - transform.position.x);
        yield return StartCoroutine(lunge.DoLunge(new Vector2(dir, 0f)));

        nextAttackTime = Time.time + (C ? C.attackCooldown : 0.8f);
        if (IsAlive) state = State.Chase;
    }

    //Facing logic: use velocity when moving, intent when slow
    void FaceByState(Vector2 moveTarget)
    {
        if (!sprite) return; // assign your SpriteRenderer in the Inspector

        // Decide which way we WANT to face
        float desiredSign;
        float vx = GetVelX();

        if (Mathf.Abs(vx) > 0.05f)        // prefer real motion when moving
            desiredSign = Mathf.Sign(vx);
        else                               // otherwise face the intent/target
            desiredSign = Mathf.Sign(moveTarget.x - transform.position.x);

        if (desiredSign == 0f) desiredSign = 1f;

        // Convert desired direction to flipX based on how the art is drawn
        // If the art naturally faces RIGHT:   face Right => flipX=false, Left => flipX=true
        // If the art naturally faces LEFT:    face Right => flipX=true,  Left => flipX=false
        bool wantRight = desiredSign > 0f;
        bool rightNeedsFlip = !artFacesRight;

        sprite.flipX = wantRight ? rightNeedsFlip : !rightNeedsFlip;
    }

    //Line of sight: multi-height sample so short targets register 
    bool HasLineOfSight()
    {
        Vector2 baseOrigin = transform.position;
        Vector2 playerCenter = target.position;
        Vector2 dest = playerCenter + Vector2.up * 0.5f;

        float[] heights = { 0.1f, C.sightHeightOffset, 1.2f };
        foreach (float h in heights)
        {
            Vector2 origin = baseOrigin + Vector2.up * h;
            Vector2 dir = (dest - origin).normalized;
            float dist = Vector2.Distance(origin, dest);

            if (!Physics2D.Raycast(origin, dir, dist, C.losBlockers))
            {
                var hitPlayer = Physics2D.Raycast(origin, dir, dist, C.playerMask);
                if (hitPlayer.collider) return true;
            }
        }
        return false;
    }

    float GetVelX()
    {
        var rb = GetComponent<Rigidbody2D>();
    #if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity.x;
    #else
            return rb.velocity.x;
    #endif
    }

    // === IDamageable ===
    public void ApplyDamage(float amount, Vector2 knockback)
    {
        if (!IsAlive) return;
        hp -= amount;

        var rb = GetComponent<Rigidbody2D>();
        rb.AddForce(knockback, ForceMode2D.Impulse);
        anim.SetTrigger("Hit");

        if (hp <= 0f)
        {
            state = State.Dead;
            anim.SetTrigger("Die");
            rb.simulated = false;
            Destroy(gameObject, 2.5f);
        }
        else
        {
            if (gameObject.activeInHierarchy) StartCoroutine(StaggerCo());
        }
    }

    IEnumerator StaggerCo()
    {
        state = State.Stagger;
        yield return new WaitForSeconds(0.2f);
        if (IsAlive) state = State.Chase;
    }

    // Debug helpers
    void OnDrawGizmosSelected()
    {
        if (!C) return;
        Gizmos.color = Color.cyan;
        Vector2 a = waypointA ? (Vector2)waypointA.position : (Application.isPlaying ? patrolStart : (Vector2)transform.position);
        Vector2 b = waypointB ? (Vector2)waypointB.position : (Application.isPlaying ? patrolTarget : (Vector2)transform.position + Vector2.right * (C ? C.patrolDistance : 4f));
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.DrawWireSphere(b, 0.15f);
    }
}
