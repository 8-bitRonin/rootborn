// IDamageable.cs
public interface IDamageable
{
    bool IsAlive { get; }
    void ApplyDamage(float amount, UnityEngine.Vector2 knockback);
}
