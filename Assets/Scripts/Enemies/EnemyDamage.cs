using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private EnemyAnimationController anim;
    private float lastAttackTime;

    public Transform currentTarget;

    private void Awake()
    {
        anim = GetComponentInParent<EnemyAnimationController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartAttack(other);
    }

    private void TryStartAttack(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        anim?.PlayAttack();

        currentTarget = other.transform;
    }
    public void DoDamage()
    {
        if (currentTarget == null) return;

        Health playerHealth = currentTarget.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.PlayerTakeDamage(damage);
        }
    }

}
