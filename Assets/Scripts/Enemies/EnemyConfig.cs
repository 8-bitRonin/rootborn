using UnityEngine;

public enum EnemyArchetype { Howler, Scuttler, Behemoth }

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Identity")]
    public EnemyArchetype archetype;

    [Header("Stats")]
    public float maxHP = 30f;
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float acceleration = 20f;
    public float turnSharpness = 12f;
    public float gravity = 30f;

    [Header("Jumping")]
    public bool canJump = true;
    public float jumpForce = 12f;
    public float jumpCooldown = 0.5f;
    public float coyoteTime = 0.12f;              // grace window after leaving ground
    public float jumpProbeDistance = 0.6f;        // how far ahead to check for gap/ledge
    public float stepProbeHeight = 0.2f;          // low wall probe height
    public float targetHeightJumpThreshold = 0.6f;// if target/waypoint is this much higher, attempt jump
    public LayerMask groundMask;                  // set to Ground (can be same as obstacleMask)

    [Header("Patrol")]
    public float patrolSpeed = 1.5f;
    public float patrolWait = 2f;   // seconds to wait at each end
    public float patrolDistance = 4f; // total distance from start


    [Header("Sensing")]
    public float aggroRange = 8f;
    public float deAggroRange = 12f;
    public float attackRange = 1.3f;
    public float sightHeightOffset = 0.4f;
    public LayerMask losBlockers;   // set to Ground
    public LayerMask playerMask;    // set to Player

    [Header("Combat")]
    public float windupTime = 0.25f;
    public float attackCooldown = 0.8f;
    public float lungeForce = 8f;
    public int damage = 10;

    [Header("Behavior Flags")]
    public bool patrols = true;
    public bool rushAndRetreat = true; // Howler style
    public bool swarmer = false;       // Scuttler: faster aggro, lower hp
    public bool tank = false;          // Behemoth: slow, heavy
}
