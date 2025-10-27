using UnityEngine.InputSystem;
using UnityEngine; 

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    private BoxCollider2D boxCollider;
    private Vector3 originalScale;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    private float wallJumpCooldown;
    private float horizontalInput;

    //Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalScale = transform.localScale;

    }
    //Update is called once per frame
    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        //Flipping sprites
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        //Wall Jump Logic
        if(wallJumpCooldown > 0.2f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

            if (onWall() && !isGrounded())
            {
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.gravityScale = 7;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
            }
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }


    }   

    //Jump Method
    private void Jump()
    {
        if (isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
        else if(onWall() && !isGrounded()) 
        {
            if(horizontalInput == 0)
            {
                rb.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                rb.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);
            }

            wallJumpCooldown = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    //Checking if the player is grounded by using Raycast(a "laser" that detects ground)
    private bool isGrounded()
    {
        RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return rayCastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return rayCastHit.collider != null;
    }
}
 