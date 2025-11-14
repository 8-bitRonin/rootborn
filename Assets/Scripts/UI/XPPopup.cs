using UnityEngine;
using TMPro;

public class XPPopup : MonoBehaviour
{
    public TextMeshPro text;  
    public float floatSpeed = 1f;
    public float lifetime = 1f;

    public void Setup(int xpAmount)
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();

        text.text = "+" + xpAmount + " XP";
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
            Destroy(gameObject);
    }
}
