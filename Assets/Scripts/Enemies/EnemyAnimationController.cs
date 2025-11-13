using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Health health;
    private EnemyDamage attack;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        attack = GetComponent<EnemyDamage>();
    }

    void Update()
    {
        float speed = 0f;

        if (rb != null)
        {
            speed = rb.linearVelocity.magnitude; 
        }

        animator.SetFloat("Speed", speed);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger("Attack");
    }

    public void PlayHurt()
    {
        if (animator == null) return;
        animator.SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetBool("Dead", true);
    }
}
