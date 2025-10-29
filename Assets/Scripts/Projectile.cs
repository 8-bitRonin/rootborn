using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private float speed;
    private bool hit;
    private float direction;
    private float lifetime;

    private BoxCollider2D boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Verifies if projectile hits and its speed + direction
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);

        //projectile despawning
        lifetime += Time.deltaTime;
        if (lifetime > 5f) gameObject.SetActive(false);
    }

    //checks if the projectile touches anything so it can disappear
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        Deactivate();
    }

    //changing direction of the sprite of the projectile when the player changes direction
    public void SetDirection(float _direction)
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        float localScaleX = transform.localScale.x;
        if(Mathf.Sign(localScaleX) != _direction){
            localScaleX = -localScaleX;
        }

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
