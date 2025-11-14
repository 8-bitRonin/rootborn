using UnityEngine;

public enum StatType
{
    Health,
    Damage,
    MoveSpeed
}

public class LevelUpCard : MonoBehaviour
{
    public StatType statType;

    public void OnCardClicked()
    {
        if (PlayerStats.Instance == null) return;

        switch (statType)
        {
            case StatType.Health:
                PlayerStats.Instance.IncreaseHealth();
                break;
            case StatType.Damage:
                PlayerStats.Instance.IncreaseDamage();
                break;
            case StatType.MoveSpeed:
                PlayerStats.Instance.IncreaseMoveSpeed();
                break;
        }
    }
}
