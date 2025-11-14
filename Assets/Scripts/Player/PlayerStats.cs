using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; 

    [Header("Level & EXP")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Stats")]
    public int maxHealth = 100;
    public int damage = 10;
    public float moveSpeed = 5f;

    [Header("Level Up")]
    public int unspentLevelUps = 0; 
    public LevelUpUI levelUpUI;    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        unspentLevelUps++;

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.3f);

        if (levelUpUI != null)
        {
            levelUpUI.Show();
        }
    }

    public void IncreaseHealth()
    {
        maxHealth += 20;
        SpendLevelPoint();
    }

    public void IncreaseDamage()
    {
        damage += 3;
        SpendLevelPoint();
    }

    public void IncreaseMoveSpeed()
    {
        moveSpeed += 0.5f;
        SpendLevelPoint();
    }

    void SpendLevelPoint()
    {
        unspentLevelUps--;

        if (unspentLevelUps <= 0 && levelUpUI != null)
        {
            levelUpUI.Hide();
        }
    }
}
