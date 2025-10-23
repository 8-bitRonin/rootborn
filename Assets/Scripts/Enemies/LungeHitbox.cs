// LungeHitbox.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LungeHitbox : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    [SerializeField] Vector2 knockback = new Vector2(8f, 2f);
    [SerializeField] LayerMask targetMask;

    HashSet<IDamageable> hitThisLunge = new();

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        gameObject.SetActive(false); // off by default
    }

    public void Arm(float newDamage) { damage = newDamage; hitThisLunge.Clear(); gameObject.SetActive(true); }
    public void Disarm() { gameObject.SetActive(false); hitThisLunge.Clear(); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;
        if (!other.TryGetComponent<IDamageable>(out var dmg) || !dmg.IsAlive) return;
        if (hitThisLunge.Contains(dmg)) return;

        // Directional knockback based on relative positions
        var dir = Mathf.Sign(other.transform.position.x - transform.position.x);
        dmg.ApplyDamage(damage, new Vector2(knockback.x * dir, knockback.y));
        hitThisLunge.Add(dmg);
    }
}
