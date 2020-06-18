using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roland : MonoBehaviour
{
    public bool isPlayerClimbing { private get; set; }

    public bool canPlayerMoveUp { private get; set; }

    public bool isPlayerDead { get; set; }

    public float ladderPositionX { private get; set; }

    public float playerEntranceTime;

    private Rigidbody2D Rigidbody2D;

    private Animator animator;

    private Collider2D playerCollider2D;

    private Dictionary<int, Collider2D> floorLaddersColliders;

    private Transform playerStartRaycastPosition;

    private MyGameController myGameController;

    private AlienEntity alienEntity;

    [SerializeField]
    private float playerSpeed;

    [SerializeField]
    private float playerClimbSpeed;

    private float horizontalPlayerMovement, verticalPlayerMovement;

    private int resetCounter;

    private bool isPlayerLookingRight;

    private bool isPlayerDigging;

    private bool canPlayerClimbUp;

    private bool playerDigSoundChanger;

    private bool canDigSoundPlay;

    private bool isPlayerStartEated;

    private float playerEatedStartTime;

    private const int ENEMY_LAYER = 9;

    private const int ANIMATION_LAYER = 0;

    private const String TAG_LADDER_FLOOR = "LadderFloor";

    private const String PLAYER_EATED_RIGHT_ANIMATION = "Roland_EatenRight";

    private const String PLAYER_EATED_LEFT_ANIMATION = "Roland_EatenLeft";

    // Use this for initialization
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider2D = GetComponent<Collider2D>();
        floorLaddersColliders = new Dictionary<int, Collider2D>();
        playerStartRaycastPosition = GameObject.FindGameObjectWithTag("Raycast").transform;
        myGameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<MyGameController>();

        resetCounter = 0;
        playerEatedStartTime = 0;

        isPlayerDead = false;
        playerDigSoundChanger = true;
        canDigSoundPlay = true;
        isPlayerStartEated = true;

        ResetAllPlayerStates();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStartRaycastPosition == null)
        {
            playerStartRaycastPosition = GameObject.FindGameObjectWithTag("Raycast").transform;
        }

        horizontalPlayerMovement = Input.GetAxisRaw("Horizontal");
        verticalPlayerMovement = Input.GetAxisRaw("Vertical");

        //Give control to player after starter animation
        if (Time.timeSinceLevelLoad >= playerEntranceTime && Time.timeScale != 0)
        {
            HandleDigInput();
            Actions(horizontalPlayerMovement, verticalPlayerMovement);
        }

        if(alienEntity == null)
        {
            return;
        }

        if (isPlayerDead && PlayerOnGround() && alienEntity.CheckOnGround())
        {
            //Save the time after first time eated player by the enemy to adjust player eated animation later
            if (isPlayerStartEated)
            {
                playerEatedStartTime = Time.realtimeSinceStartup;
                isPlayerStartEated = false;
                PlayDeadSound();
            }

            if(isPlayerLookingRight)
            {
                animator.PlayInFixedTime(PLAYER_EATED_RIGHT_ANIMATION, ANIMATION_LAYER, Time.realtimeSinceStartup - playerEatedStartTime);
            }
            else
            {
                animator.PlayInFixedTime(PLAYER_EATED_LEFT_ANIMATION, ANIMATION_LAYER, Time.realtimeSinceStartup - playerEatedStartTime);
            }
        }
    }

    // FixedUpdate is called in fixed rate of time
    void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad >= playerEntranceTime)
        {
            HandlePlayerMovement(horizontalPlayerMovement, verticalPlayerMovement);
        }

        StartMoveRight();
    }

    // Move roland right some steps when the game starts
    private void StartMoveRight()
    {
        if (Time.timeSinceLevelLoad <= playerEntranceTime)
        {
            Rigidbody2D.velocity = new Vector2(1 * playerSpeed, Rigidbody2D.velocity.y);
            animator.SetFloat("speed", 1);
        }
    }

    private void HandleDigInput()
    {
        if (Input.GetButtonDown("Fire1") && !isPlayerClimbing)
        {
            isPlayerDigging = true;
        }
        else
        {
            isPlayerDigging = false;
        }
    }

    // Here we handle players actions like: dig, climb etc...
    private void Actions(float horizontal, float vertical)
    {
        HandleDig(horizontal);
        HandlePlayerClimb(horizontal, vertical);
    }

    private void HandleDig(float horizontal)
    {
        // check if key pressed with isDigging, if animation still play with anim and if player not moving with horizontal
        if (isPlayerDigging && !PlayerDiggingAnimation() && horizontal == 0)
        {
            animator.SetTrigger("dig");

            Vector2 startLoc2D = new Vector2(playerStartRaycastPosition.position.x, playerStartRaycastPosition.position.y);

            // Cast a raycast to check if we hit a floor and isnt an alien inside then change the floor state to get dmg
            RaycastHit2D hit = Physics2D.Raycast(startLoc2D, Vector2.down, 0.5f, LayerMask.GetMask("Floor"));

            if (hit)
            {
                if (hit.transform.gameObject.tag == ("Floor"))
                {
                    if (!hit.collider.gameObject.GetComponent<Floor>().isAlienInside)
                    {
                        hit.collider.gameObject.GetComponent<Floor>().Damage();

                        if(hit.collider.gameObject.GetComponent<Floor>().dmgState == 0 && hit.collider.gameObject.GetComponent<Floor>().canDigged)
                        {
                            canDigSoundPlay = false;
                        }
                        else
                        {
                            canDigSoundPlay = true;
                        }
                    }
                }
            }

            // Cast a raycast to check if we hit an alien inside the floor and then we kill it
            RaycastHit2D hitAlien = Physics2D.Raycast(startLoc2D, Vector2.down, 0.5f, LayerMask.GetMask("Enemy"));

            if (hitAlien)
            {
                if (hitAlien.transform.gameObject.tag == ("GreenAlien"))
                {
                    GreenAliens ga = hitAlien.transform.gameObject.GetComponent<GreenAliens>();

                    if (ga.isInFloor && ga.isDieable)
                    {
                        myGameController.greenAliens--;
                        ga.SelfDestroy(true);
                    }
                }

                // If ray hit a red alien and alien is in floor then run function 
                // FallFromFloor to handle count time red alien hited
                if (hitAlien.transform.gameObject.tag == ("RedAlien"))
                {
                    RedAliens ra = hitAlien.transform.gameObject.GetComponent<RedAliens>();

                    if (ra.isInFloor && ra.isDieable)
                    {
                        ra.FallFromFloor();
                    }
                }

                if (hitAlien.transform.gameObject.tag == ("BossAlien"))
                {
                    BossAliens ba = hitAlien.transform.gameObject.GetComponent<BossAliens>();

                    if (ba.isInFloor && ba.isDieable)
                    {
                        ba.FallFromFloor();
                    }
                }

            }

            isPlayerDigging = false;
        }
    }

    private void HandlePlayerClimb(float horizontal, float vertical)
    {
        CheckCanClimb();

        if (PlayerMoveVertical(horizontal, vertical) && canPlayerClimbUp && !PlayerDiggingAnimation())
        {
            isPlayerClimbing = true;
        }

        if (PlayerMoveHorizontal(horizontal, vertical) && canPlayerClimbUp)
        {
            isPlayerClimbing = false;
        }

        // Handle climbing animation speed if animation DIG is stoped
        if (isPlayerClimbing && !PlayerDiggingAnimation())
        {
            Rigidbody2D.position = new Vector2(ladderPositionX, Rigidbody2D.position.y);
            Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);

            animator.speed = Mathf.Abs(vertical);
        }
        else
        {
            animator.speed = 1;
        }

        // if player stop climbing set all players and ladders floor collisions to false
        foreach (Collider2D c in floorLaddersColliders.Values)
        {
            if (!isPlayerClimbing && c && Physics2D.GetIgnoreCollision(playerCollider2D, c))
            {
                Physics2D.IgnoreCollision(playerCollider2D, c, false);
            }
        }

        // We clean the dictionary after set all floor colliders to false
        if (!isPlayerClimbing)
        {
            floorLaddersColliders.Clear();
            resetCounter = 0;
        }

        // Set climping animation variables
        animator.SetBool("isClimbing", isPlayerClimbing);
        animator.SetFloat("climbSpeed", vertical);
    }

    private static bool PlayerMoveHorizontal(float horizontal, float vertical)
    {
        return horizontal != 0 && vertical == 0;
    }

    private static bool PlayerMoveVertical(float horizontal, float vertical)
    {
        return vertical != 0 && horizontal == 0;
    }

    private bool PlayerDiggingAnimation()
    {
        return GetAnimationInfo(0, "dig");
    }

    private void CheckCanClimb()
    {
        if ((CheckForObstacleUp() || CheckForObstacleDown()) && PlayerOnGround())
        {
            canPlayerClimbUp = true;
        }
        else
        {
            canPlayerClimbUp = false;
        }
    }

    private bool CheckForObstacleUp()
    {
        RaycastHit2D hitUp = Physics2D.Raycast(Rigidbody2D.position + Vector2.up, Vector2.up, .5f, LayerMask.GetMask("Floor"));

        if (hitUp)
        {
            if (hitUp.transform.gameObject.tag == "LadderFloor")
            {
                ladderPositionX = hitUp.transform.position.x;
                return true;
            }
        }

        return false;
    }

    private bool CheckForObstacleDown()
    {

        RaycastHit2D hitDown = Physics2D.Raycast(Rigidbody2D.position, Vector2.down, .5f, LayerMask.GetMask("Floor"));

        if (hitDown)
        {
            if (hitDown.transform.gameObject.tag == "LadderFloor")
            {
                ladderPositionX = hitDown.transform.position.x;
                return true;
            }
        }

        return false;
    }

    public bool PlayerOnGround()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(Rigidbody2D.position, Vector2.down, .3f, LayerMask.GetMask("Floor"));

        if (hitGround)
        {
            if (hitGround.transform.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                return true;
            }
        }

        return false;
    }

    private void HandlePlayerMovement(float horizontal, float vertical)
    {
        FlipPlayerDirection(horizontal);

        // Check if player isnt on climbing state and isnt on dig animation and then we can move horizontaly
        if (!isPlayerClimbing && !PlayerDiggingAnimation())
        {
            // Move the player with horizontal axes and multiply with player speed, we set y velocity with same as player velocity
            Rigidbody2D.velocity = new Vector2(horizontal * playerSpeed, Rigidbody2D.velocity.y);
        }

        // Move player up and down when isClimbing is true
        if (isPlayerClimbing && canPlayerMoveUp)
        {
            Rigidbody2D.gravityScale = 0;
            Rigidbody2D.velocity = new Vector2(Rigidbody2D.velocity.x, vertical * playerClimbSpeed);
        }
        else
        {
            Rigidbody2D.gravityScale = 1;
        }

        // Set horizontal to speed variable to choose walk animation to play
        animator.SetFloat("speed", horizontal);
    }

    private void FlipPlayerDirection(float horizontal)
    {
        // Check speed and looking direction and we reverse direction
        if (horizontal > 0 && !isPlayerLookingRight || horizontal < 0 && isPlayerLookingRight)
        {
            isPlayerLookingRight = !isPlayerLookingRight;
        }

        // Set the looking direction to choose what direction animation will play
        animator.SetBool("isLookingRight", isPlayerLookingRight);
    }

    // Helper function to check if animation is still play
    private bool GetAnimationInfo(int layer, string tag)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).IsTag(tag);
    }

    public void ResetAllPlayerStates()
    {
        isPlayerLookingRight = true;
        isPlayerDigging = false;
        isPlayerClimbing = false;
        canPlayerClimbUp = false;
        canPlayerMoveUp = true;

        horizontalPlayerMovement = 0;
        verticalPlayerMovement = 0;

        Rigidbody2D.velocity = Vector2.zero;
    }

    public void PlayDigSound()
    {
        if(canDigSoundPlay)
        {
            if (playerDigSoundChanger)
            {
                GlobalGameController.PlaySoundEffect("Dig1");
                playerDigSoundChanger = !playerDigSoundChanger;
            }
            else
            {
                GlobalGameController.PlaySoundEffect("Dig2");
                playerDigSoundChanger = !playerDigSoundChanger;
            }
        }
    }

    public void PlayDeadSound()
    {
        GlobalGameController.PlaySoundEffect("DeadMan");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.layer == ENEMY_LAYER)
        {
            alienEntity = other.gameObject.GetComponent<AlienEntity>();
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // tracking ladders floor collider to reset them later and set players and ladders floor ignore collisions to true
        if (other.gameObject.tag == TAG_LADDER_FLOOR && isPlayerClimbing)
        {
            floorLaddersColliders.Add(resetCounter, other.collider);

            Physics2D.IgnoreCollision(playerCollider2D, floorLaddersColliders[resetCounter], true);
            resetCounter++;
        }
    }
}
