using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float parallaxFactor = 0.3f; // 0=stuck to world, 1=locked to camera
    public bool affectY = false;                         // turn on if you have vertical scrolling
    Transform cam;
    Vector3 prevCamPos;

    void Awake()
    {
        cam = Camera.main ? Camera.main.transform : null;
        if (!cam) Debug.LogError("ParallaxLayer: No Main Camera found.");
        prevCamPos = cam ? cam.position : Vector3.zero;
    }

    void LateUpdate()
    {
        if (!cam) return;
        Vector3 delta = cam.position - prevCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor,
                                          affectY ? delta.y * parallaxFactor : 0f,
                                          0f);
        prevCamPos = cam.position;
    }
}
