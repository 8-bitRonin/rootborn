// MeleeLunge.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeleeLunge : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] float windupTime = 0.28f;
    [SerializeField] float lungeDuration = 0.22f;
    [SerializeField] float recoverTime = 0.25f;
    [SerializeField] float cooldown = 0.8f;

    [Header("Motion")]
    [SerializeField] float lungeSpeed = 18f;
    [SerializeField] float stopControlMultiplier = 0.1f; // reduce steering while lunging

    [Header("Ranges")]
    [SerializeField] float minRange = 1.2f;   // too close? back off a touch before lunging (handled by brain)
    [SerializeField] float maxRange = 5.0f;   // won’t lunge beyond this

    [Header("Damage")]
    [SerializeField] float damage = 15f;
    [SerializeField] LungeHitbox hitbox;

    [Header("Anim (optional)")]
    [SerializeField] Animator anim;
    [SerializeField] string windupTrigger = "Windup";
    [SerializeField] string lungeTrigger = "Lunge";
    [SerializeField] string recoverTrigger = "Recover";

    public bool IsCoolingDown => Time.time < _nextUsableTime;
    public bool IsLunging { get; private set; }

    Rigidbody2D rb;
    float _nextUsableTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    public bool InRange(float dist) => dist >= minRange && dist <= maxRange && !IsCoolingDown && !IsLunging;

    public IEnumerator DoLunge(Vector2 dir)
    {
        IsLunging = true;

        // WINDUP
        anim?.SetTrigger(windupTrigger);
        hitbox?.Disarm();
        yield return new WaitForSeconds(windupTime);

        // LUNGE
        anim?.SetTrigger(lungeTrigger);
        var storedDrag = rb.linearDamping;
        rb.linearDamping *= stopControlMultiplier;
        hitbox?.Arm(damage);

        float t = 0f;
        while (t < lungeDuration)
        {
            rb.linearVelocity = new Vector2(dir.x * lungeSpeed, rb.linearVelocity.y); // preserve gravity
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // RECOVER
        hitbox?.Disarm();
        rb.linearDamping = storedDrag;
        anim?.SetTrigger(recoverTrigger);
        yield return new WaitForSeconds(recoverTime);

        _nextUsableTime = Time.time + cooldown;
        IsLunging = false;
    }
}
