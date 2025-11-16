using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    private bool hit;
    private float direction;
    private float lifetime;

    private BoxCollider2D boxCollider;
    private Animator anim;

    [SerializeField] private int baseDamage = 10; // istersen inspector’dan

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (hit) return;

        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > 5f) gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hit) return;

        hit = true;
        boxCollider.enabled = false;
        if (anim != null)
            anim.SetTrigger("explode");

        Health enemyHealth = collision.GetComponent<Health>();
        if (enemyHealth == null)
            enemyHealth = collision.GetComponentInParent<Health>();

        if (enemyHealth != null)
        {
            int dmg = PlayerStats.Instance != null ? PlayerStats.Instance.damage : baseDamage;
            enemyHealth.EnemyTakeDamage(dmg);
        }
    }

    public void SetDirection(float _direction)
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
            localScaleX = -localScaleX;

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }
}
