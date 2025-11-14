using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public CanvasGroup cg;

    private void Awake()
    {
        // Panel sahne açılır açılmaz kapalı başlasın
        Hide();
    }

    public void Show()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Time.timeScale = 0f; // istersen oyunu durdur
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        Time.timeScale = 1f;
    }
}
