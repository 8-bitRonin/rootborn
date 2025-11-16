using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    public int xpAmount = 25;
    public Health health;
    public GameObject xpPopupPrefab;

    private bool isDead = false;
    public EnemySpawner spawner;   // Spawner referance

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // XP popup
        if (xpPopupPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1f;
            GameObject popupObj = Instantiate(xpPopupPrefab, spawnPos, Quaternion.identity);
            popupObj.GetComponent<XPPopup>()?.Setup(xpAmount);
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddXP(xpAmount);
        }

        if (spawner != null)
        {
            spawner.OnEnemyDied();
        }

        // death anim/destroy
        if (health != null)
            health.EnemyDeath();
        else
            Destroy(gameObject);
    }

}
