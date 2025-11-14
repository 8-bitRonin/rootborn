using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] orbs;

    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //left click to attack
        if (Input.GetMouseButton(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            Attack();
        }
        cooldownTimer += Time.deltaTime;
    }

    //attack logic
    private void Attack()
    {
        cooldownTimer = 0;
        int orbIndex = FindOrbs();
        orbs[orbIndex].transform.position = firePoint.position;
        orbs[orbIndex].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }

    //object pooling for the orbs(0-9 projectiles)
    private int FindOrbs()
    {
        for (int i = 0; i < orbs.Length; i++)
        {
            if (!orbs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
