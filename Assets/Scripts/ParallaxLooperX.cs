using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLooperX : MonoBehaviour
{
    SpriteRenderer sr;
    float width;
    Transform cam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main ? Camera.main.transform : null;

        if (sr == null || sr.sprite == null)
        {
            Debug.LogError($"{name}: SpriteRenderer or sprite missing.");
            enabled = false;
            return;
        }

        width = sr.bounds.size.x;
        if (width <= 0f)
        {
            Debug.LogError($"{name}: Sprite width invalid ({width}).");
            enabled = false;
        }
    }


    void LateUpdate()
    {
        if (!cam) return;
        if (width <= 0f) return;

        float diff = cam.position.x - transform.position.x;
        if (Mathf.Abs(diff) >= width)
        {
            float offset = Mathf.Floor(diff / width) * width;
            transform.position += new Vector3(offset, 0f, 0f);
        }

    }
}
