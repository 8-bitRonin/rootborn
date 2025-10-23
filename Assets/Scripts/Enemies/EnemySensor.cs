using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemySensor : MonoBehaviour
{
    public EnemyConfig config;
    public Transform target; // usually player
    public bool HasLOS { get; private set; }
    public bool InAggro { get; private set; }
    public bool InAttackRange { get; private set; }

    Transform self;
    Vector2 Eye => (Vector2)self.position + Vector2.up * config.sightHeightOffset;

    void Awake() { self = transform; }

    void FixedUpdate()
    {
        if (!target || !config) return;

        float dist = Vector2.Distance(self.position, target.position);
        InAttackRange = dist <= config.attackRange;
        InAggro = dist <= config.aggroRange;

        // simple LOS ray
        var dir = ((Vector2)target.position - Eye).normalized;
        float rayLen = Mathf.Min(dist, config.deAggroRange);
        HasLOS = !Physics2D.Raycast(Eye, dir, rayLen, config.losBlockers);
    }

    public bool ShouldAggro() => InAggro && HasLOS;
    public bool ShouldDeAggro() =>
        Vector2.Distance(self.position, target.position) > config.deAggroRange || !HasLOS;
}
