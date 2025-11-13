using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public Image healthBar;
    [SerializeField] private float startingHealth;
    [SerializeField] private float deathAnimationDuration;
    public float currentHealth { get; private set; }

    private EnemyAnimationController anim;
    private EnemyAI enemyAI;
    private Collider2D col;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<EnemyAnimationController>();
        enemyAI = GetComponent<EnemyAI>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnemyTakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        anim?.PlayHurt();

        // health bar fill amount
        // if (healthBar != null) healthBar.fillAmount = currentHealth / startingHealth;

        if (currentHealth <= 0)
        {
            EnemyDeath();
        }
    }
    public void PlayerTakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        anim?.PlayHurt();

        // health bar fill amount
        // if (healthBar != null) healthBar.fillAmount = currentHealth / startingHealth;

        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    void EnemyDeath()
    {
        anim?.PlayDeath();

        if (enemyAI != null) enemyAI.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null) col.enabled = false;

        Destroy(gameObject, deathAnimationDuration); 
    }
    void PlayerDeath()
    {
        anim?.PlayDeath();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null) col.enabled = false;

        Destroy(gameObject, deathAnimationDuration); 
    }
}
