using UnityEngine;

public class EnemyAttackEvents : MonoBehaviour
{
    private EnemyDamage damage;

    private void Awake()
    {
        damage = GetComponentInChildren<EnemyDamage>();
    }

    public void DealDamageAnimationEvent()
    {
        if (damage != null)
        {
            damage.DoDamage(); 
        }
    }
}
