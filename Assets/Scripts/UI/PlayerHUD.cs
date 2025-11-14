using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class PlayerHUD : MonoBehaviour
{
    public PlayerStats player;
    public TextMeshProUGUI levelText;   
    public TextMeshProUGUI debugger;   
    public Slider xpSlider;             

    private void Start()
    {
        if (player == null)
            player = PlayerStats.Instance;
    }

    private void Update()
    {
        if (player == null) return;

        levelText.text = "Lv " + player.level;
        xpSlider.value = (float)player.currentXP / player.xpToNextLevel;

        debugger.text = $"hlth: {player.maxHealth}, dmg:{player.damage}, spd:{player.moveSpeed}";


    }
}
