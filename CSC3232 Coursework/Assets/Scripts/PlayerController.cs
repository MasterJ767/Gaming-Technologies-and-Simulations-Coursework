using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls logic and physics of 3D, in-level player
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    public CharacterController controller;
    public Transform groundCheck;

    [Header("HUD Settings")] 
    public Canvas hud;
    public float textSize = 64;
    public float largeTextSize = 96;
    public Color textColor;
    public Color lightTextColor;
    public Font font;
    
    [Header("Menu Settings")]
    public OverworldMenu menuController;
    public GameObject deathScreen;

    [Header("Ground Check Attributes")] 
    public float groundDistance = 0.04f;
    public LayerMask groundMask;
    
    [Header("Player Movement Attributes")]
    public float walkSpeed = 6f;
    public float walkAccelerationTime = 0.5f;
    public float sprintSpeed = 12.5f;
    public float sprintAccelerationTime = 3.75f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    [NonSerialized]
    public float speed = 0;
    [NonSerialized]
    public Vector3 velocity;
    [NonSerialized] 
    public Vector3 slipVelocity;
    [NonSerialized]
    public bool isGrounded = false;
    [NonSerialized]
    public bool onIce = false;

    [Header("Player Health Attributes")] 
    public Slider slider;
    public TextMeshProUGUI healthText;
    public Image healthFill;
    public Gradient healthColour;
    public float baseMaxHealth;
    public GameObject damageScreen;
    private float currentHealth;
    private float impactTime;

    [Header("Player Attack Attributes")] 
    public LayerMask enemyMask;
    public GameObject magicAttack;
    public Sprite magicAttackImage;
    public float baseMagicDamage;
    public float baseMagicRange;
    public float baseMagicFireRate;
    public float baseMagicProjectileSpeed;
    public GameObject swordAttack;
    public Animator swordAnimator;
    public Sprite swordAttackImage;
    public float baseSwordDamge;
    public float baseSwordAttackSpeed;
    public GameObject primaryAttackSlot;
    private AttackMode attackMode;
    private bool canAttack = true;

    [Header("Vine Ability Attributes")] 
    public GameObject vineAbilitySlot;
    public Sprite vineAbilityImage;
    public GameObject vine;
    public GameObject vineAim;
    public Sprite allowVineImage;
    public Sprite disallowVineImage;
    public float baseVineCooldown = 8f;
    public float baseVineMaxExtent = 30f;
    private bool drawVine = false;
    private bool canVine = true;
    private bool allowVine;
    private Vector3 vineDestination;
    private Vector3 vineInitial;
    private Vector3 vineDirection;
    private Vector3 vineForce;
    private float vineStart;

    [Header("Levitate Ability Attributes")]
    public GameObject levitateAbilitySlot;
    public Sprite levitateAbilityImage;
    public float baseLevitateCooldown = 8f;
    public float baselevitateRange = 15f;
    private bool canElevate = true;
    private bool allowLevitate;
    private List<EnemyController> levitated;
    
    [Header("Layer Exclusion")]
    public LayerMask playerLayer;
    public LayerMask damageLayer;

    [Header("Attribute Multipliers")]
    public float healthMultiplierAmount = 10f;
    public float magicRangeMultiplierAmount = 1.2f;
    public float magicRateMultiplierAmount = 0.08f;
    public float magicSpeedMultiplierAmount = 0.9f;
    public float magicDamageMultiplierAmount = 0.6f;
    public float swordSpeedMultiplierAmount = 0.07f;
    public float swordDamageMultiplierAmount = 1.8f;
    public float vineExtentMultiplierAmount = 3f;
    public float vineCooldownMultiplierAmount = 0.6f;
    public float levitateRangeMultiplierAmount = 1.5f;
    public float levitateCooldownMultiplierAmount = 0.5f;

    [NonSerialized]
    public int healthMultiplier = 0;
    [NonSerialized]
    public int magicRangeMultiplier = 0;
    [NonSerialized]
    public int magicRateMultiplier = 0;
    [NonSerialized]
    public int magicSpeedMultiplier = 0;
    [NonSerialized]
    public int magicDamageMultiplier = 0;
    [NonSerialized]
    public int swordSpeedMultiplier = 0;
    [NonSerialized]
    public int swordDamageMultiplier = 0;
    [NonSerialized]
    public int vineExtentMultiplier = 0;
    [NonSerialized]
    public int vineCooldownMultiplier = 0;
    [NonSerialized]
    public int levitateRangeMultiplier = 0;
    [NonSerialized]
    public int levitateCooldownMultiplier = 0;
    private GameManager gm;
    
    private bool recordFall = false;
    private float maxY;
    
    private bool isDead = false;
    private bool _inUi = false;

    public bool InUI
    {
        get => _inUi;
        set
        {
            _inUi = value;
            transform.Find("Eyes").GetComponent<CameraMovement>().inUI = value;
            gm.Pause(value);
            if (value)
            {
                hud.GetComponent<CanvasGroup>().alpha = 0;
            }
            else
            {
                hud.GetComponent<CanvasGroup>().alpha = 1;
            }
        }
    }

    private void Awake()
    {
        // Set the counter used to display the taken damage vignette
        impactTime = -1.5f;
        
        // Set initial velocity
        velocity = new Vector3(0, 0, 0);
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    
    private void Start()
    {
        // Initialise a list of enemy controllers containing all enemies who were highlighted by the levitate ability last frame
        levitated = new List<EnemyController>();
        
        // Apply text settings to HUD elements
        ApplyTextSettings(healthText, true, false);
        ApplyTextSettings(vineAbilitySlot.transform.Find("CooldownText").gameObject.GetComponent<TextMeshProUGUI>(), true, true);
        ApplyTextSettings(levitateAbilitySlot.transform.Find("CooldownText").gameObject.GetComponent<TextMeshProUGUI>(), true, true);
        
        // Initialise the player's health bar
        SetSliderMaxHealth();
        
        // Set the default attack mode to Magic mode
        attackMode = AttackMode.Magic;
        
        // Initialise the player's ability slots
        primaryAttackSlot.transform.Find("Background").GetComponent<Image>().sprite = magicAttackImage;
        vineAbilitySlot.transform.Find("Background").GetComponent<Image>().sprite = vineAbilityImage;
        levitateAbilitySlot.transform.Find("Background").GetComponent<Image>().sprite = levitateAbilityImage;
    }

    private void Update()
    {
        if (!_inUi)
        {
            // Check to see if player is alive
            DeathCheck();

            if (!isDead)
            {
                // Check to see if grounded
                GroundedCheck();

                // Check to see if the player wants to open menus
                MenuCheck();

                // Move player according to inputs 
                MovePlayer();

                // Run vine ability updates and permissions
                if (drawVine)
                {
                    UpdateVine();
                }

                CheckVineAttachment();

                // Run levitate ability permissions
                CheckLevitate();

                // Use abilities according to inputs 
                ExecuteAbilities();

                // Check if player took damage
                CheckDamage();
            }
        }
    }

    /// <summary>
    /// Checks if the player has died
    /// </summary>
    private void DeathCheck()
    {
        if (currentHealth <= 0)
        {
            isDead = true;
            Cursor.lockState = CursorLockMode.Confined;
            deathScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Updates the isGrounded bool to determine if the player is touching the ground
    /// </summary>
    private void GroundedCheck()
    {
        isGrounded = Physics.CheckCapsule(groundCheck.position, groundCheck.position - new Vector3(0, groundDistance, 0), 0.4f, groundMask) || Physics.CheckCapsule(groundCheck.position, groundCheck.position - new Vector3(0, groundDistance, 0), 0.4f, enemyMask);
    }
    
    /// <summary>
    /// Moves player taking into account player input and resistive forces. Also is responsible for fall damage calculations
    /// </summary>
    private void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f) * CalculateSpeed((x != 0 || z != 0));
        
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity if not grounded, record information for fall damage calculations
        if (!isGrounded)
        {
            if (velocity.y < 0.001f && !recordFall)
            {
                recordFall = true;
                maxY = transform.position.y;
            }
            
            // Slowed Falling speed by holding jump button on descent
            if (velocity.y < 0.001f && Input.GetButton("Jump"))
            {
                velocity.y += 0.125f * gravity * Time.deltaTime;
                recordFall = false;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }
        
        // Apply fall damage once on the ground
        if (isGrounded && recordFall)
        {
            recordFall = false;
            float difference = maxY - transform.position.y;
            float fallDamage = (difference - (jumpHeight * 1.5f)) / 2;
            if (fallDamage > 0)
            {
                TakeDamage(fallDamage);
            }
        }
        
        float resistance = speed / 200f;

        // Apply air resistance in the x direction after use of vine ability or when on ice
        if (velocity.x < resistance && velocity.x > -resistance)
        {
            velocity.x = 0;
        }
        else if (velocity.x > 0)
        {
            velocity.x -= resistance;
        }
        else if (velocity.x < 0)
        {
            velocity.x += resistance;
        }

        // Apply air resistance in the z direction after use of vine ability
        if (velocity.z < resistance && velocity.z > -resistance)
        {
            velocity.z = 0;
        }
        else if (velocity.z > 0)
        {
            velocity.z -= resistance;
        }
        else if (velocity.z < 0)
        {
            velocity.z += resistance;
        }

        // Apply slipperiness if on ice
        if (onIce)
        {
            Vector3 slipForce = new Vector3(move.x, 0, move.z);
            slipVelocity += slipForce * 0.015f;
        }
        else
        {
            slipVelocity = Vector3.zero;
        }
        
        // Move player after taking into account player inputs and resistive forces
        controller.Move((velocity + slipVelocity + move) * Time.deltaTime);
    }
    
    /// <summary>
    /// Opens the menu on button press
    /// </summary>
    private void MenuCheck()
    {
        if (Input.GetButtonDown("Menu"))
        {
            InUI = true;
            menuController.MenuOn(this);
        }
    }
    
    /// <summary>
    /// Determine player's current speed
    /// </summary>
    /// <param name="isMoving">A bool stating if player input for movement was recieved this fram</param>
    /// <returns>A float containing the player's current speed</returns>
    private float CalculateSpeed(bool isMoving)
    {
        if (!isMoving)
        {
            speed = 0;
            return speed;
        }
        
        // Set acceleration to 0 in case the player is already at their maximum speed
        float acceleration = 0;
        
        // Calculate Acceleration from walk to run speed
        if (isGrounded && Input.GetButton("Run") && speed < sprintSpeed && speed >= walkSpeed)
        {
            acceleration = (sprintSpeed - walkSpeed) / sprintAccelerationTime;
        }
        // Limit player speed to the maximum sprint speed
        else if (isGrounded && Input.GetButton("Run") && speed >= sprintSpeed)
        {
            speed = sprintSpeed;
        }
        // Calculate Acceleration from static to walk speed
        else if (speed < walkSpeed)
        {
            acceleration = walkSpeed / walkAccelerationTime;
        }
        // Calculate acceleration if the player is moving at walk speed or if they are decelerating from run to walk speed
        else
        {
            acceleration = -((sprintSpeed - walkSpeed) / sprintAccelerationTime);
        }
        
        speed += (acceleration * Time.deltaTime);

        return speed;
    }
    
    /// <summary>
    /// Execute player's special abilities when the player presses the appropriate keys
    /// </summary>
    private void ExecuteAbilities()
    {
        // Switch between attack modes (Magic and Sword modes)
        if (Input.GetButtonDown("Switch"))
        {
            if (attackMode == AttackMode.Magic)
            {
                swordAttack.gameObject.SetActive(true);
                attackMode = AttackMode.Sword;
                primaryAttackSlot.transform.Find("Background").GetComponent<Image>().sprite = swordAttackImage;
            }
            else
            {
                swordAttack.gameObject.SetActive(false);
                attackMode = AttackMode.Magic;
                primaryAttackSlot.transform.Find("Background").GetComponent<Image>().sprite = magicAttackImage;
            }
        }
        // Attack with primary fire
        else if (Input.GetButton("Fire1") && canAttack)
        {
            // Release a projectile in the camera forward direction in magic mode
            if (attackMode == AttackMode.Magic)
            {
                Transform cameraTransform = transform.Find("Eyes"); 
                GameObject projectile = Instantiate(magicAttack, cameraTransform.position + new Vector3(0, -0.2f, 0), cameraTransform.rotation);
                projectile.GetComponent<Projectile>().Initialise(this);
                StartCoroutine(MagicCooldown());
            }
            // Play the sword swipe animation in sword mode
            else
            {
                swordAttack.GetComponent<AudioSource>().Play();
                swordAnimator.SetTrigger("Attack");
                StartCoroutine(SwordCooldown());
            }
        }
        // Launch vine ability
        else if (Input.GetButtonDown("Ability1") && canVine)
        {
            Transform cameraTransform = transform.Find("Eyes");
            
            // Check the player can use the ability currently
            RaycastHit hit = CheckVineAttachment();
            
            if (allowVine)
            {
                /*
                  If the raycast for the vine intersects with the environment or an enemy create a cuboid between the player
                  and the destination to simulate the vine
                */
                if (hit.collider.CompareTag("Environment") || hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Ice"))
                {
                    // Get Collision information
                    drawVine = true;
                    vineDirection = cameraTransform.forward.normalized;
                    vineForce = vineDirection * hit.distance;
                    vineDestination = transform.position + vineForce;
                    vineInitial = transform.position;
                    vineStart = Time.time;
                    
                    // Render the vine
                    MeshFilter mf = vine.transform.GetComponent<MeshFilter>();

                    Vector3 right = Vector3.right * 0.0625f;
                    Vector3 up = Vector3.up * 0.0625f;
                    
                    Vector3[] vertices = new[]
                    {
                        vineInitial + vineDirection - right, vineInitial + vineDestination - up, vineInitial + vineDestination + right, vineInitial + vineDirection + up, 
                        vineDestination - right, vineDestination - up, vineDestination + right, vineDestination + up
                    };
                    int[] indices = new[] { 0, 3, 1, 1, 3, 2, 5, 6, 4, 4, 6, 7, 3, 7, 2, 2, 7, 6, 1, 5, 0, 0, 5, 4, 4, 7, 0, 0, 7, 3, 1, 2, 5, 5, 2, 6 };
                    Mesh mesh = new Mesh();
                    mesh.SetVertices(vertices);
                    mesh.SetTriangles(indices, 0);
                    mesh.RecalculateNormals();

                    mf.mesh = mesh;
                    
                    velocity += vineForce * 0.7f;
                    
                    // Set the vine object to active
                    vine.gameObject.SetActive(true);

                    StartCoroutine(VineCooldown());
                }
            }
        }
        // Execute elevate ability
        else if (Input.GetButtonDown("Ability2") && canElevate)
        {
            // Check the player can use the ability currently
            RaycastHit hit = CheckLevitate();
            
            if (allowLevitate)
            {
                /*
                  Get the rigid body of the enemy selected and pass the necessary information so that the enemy can be launched
                  into the air
                */
                if (hit.collider.gameObject.layer == 8)
                {
                    if (!hit.collider.CompareTag("Bot"))
                    {
                        EnemyController ec;
                        if (hit.collider.CompareTag("Head"))
                        {
                            ec = hit.collider.gameObject.transform.parent.parent.parent.parent.parent.parent.parent
                                .parent.gameObject.GetComponent<EnemyController>();
                        }
                        else
                        {
                            ec = hit.collider.gameObject.GetComponent<EnemyController>();
                        }

                        ec.updraftNextFrame = true;
                        StartCoroutine(LevitateCooldown());
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Assesses if the player's vine ability can be executed each frame, taking into account what they player is looking at, how far away
    /// the thing they are looking at is. It then modifies the player's crosshair to indicate if the player can or cannot use the ability.
    /// </summary>
    /// <returns>RaycastHit object which is used when the ability is executed</returns>
    private RaycastHit CheckVineAttachment()
    {
        RaycastHit hit;
        
        // If the ability is on cooldown, skip check
        if (canVine)
        {
            Transform cameraTransform = transform.Find("Eyes");
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, GetVineMaxExtent(), ~(playerLayer | damageLayer)))
            {
                if (hit.collider.CompareTag("Psychic Box"))
                {
                    vineAim.transform.GetComponent<Image>().sprite = disallowVineImage;
                    allowVine = false;
                }
                else if (hit.collider.CompareTag("Environment") || hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Ice"))
                {
                    vineAim.transform.GetComponent<Image>().sprite = allowVineImage;
                    allowVine = true;
                }
                else
                {
                    vineAim.transform.GetComponent<Image>().sprite = disallowVineImage;
                    allowVine = false;
                }
            }
            else
            {
                vineAim.transform.GetComponent<Image>().sprite = disallowVineImage;
                allowVine = false;
            }
        }
        else
        {
            Physics.Raycast(transform.position, transform.forward, out hit, 0.001f);
            vineAim.transform.GetComponent<Image>().sprite = disallowVineImage;
            allowVine = false;
        }

        return hit;
    }
    
    /// <summary>
    /// Updates the position of the vertices near the player which are used to construct the near end of the vine cuboid, then re-renders
    /// the object. This method also turns off the vine if either the distance or temporal termination requirements are met for deactivating the vine.
    /// </summary>
    private void UpdateVine()
    {
        // Deactivate the vine object if the player has travelled more than half of the distance towards the vine destination
        if (Vector3.Distance(vineInitial, transform.position) >= Vector3.Distance(vineInitial, vineDestination) * 0.5)
        {
            vine.gameObject.SetActive(false);
            drawVine = false;
        }
        // Deactivate the vine object if more than 0.6 seconds have passed since the ability's activation
        else if (Time.time >= vineStart + 0.6f)
        {
            vine.gameObject.SetActive(false);
            drawVine = false;
        }
        // Update and re-render the vine
        else
        {
            MeshFilter mf = vine.transform.GetComponent<MeshFilter>();

            Vector3 right = Vector3.right * 0.0625f;
            Vector3 up = Vector3.up * 0.0625f;
                    
            Vector3[] vertices = new[]
            {
                transform.position + vineDirection - right, transform.position + vineDirection - up, transform.position + vineDirection + right, transform.position + vineDirection + up, 
                vineDestination - right, vineDestination - up, vineDestination + right, vineDestination + up
            };
            
            mf.mesh.SetVertices(vertices);
            mf.mesh.RecalculateNormals();
        }
    }
    
    /// <summary>
    /// Assesses if the player's elevate ability can be executed each frame, taking into account what they player is looking at, how far away
    /// the thing they are looking at is. It then activates and deactivates a particle highlight on enemies to indicate if the player can or cannot use the ability.
    /// </summary>
    /// <returns>RaycastHit object which is used when the ability is executed</returns>
    private RaycastHit CheckLevitate()
    {
        Transform cameraTransform = transform.Find("Eyes");
        RaycastHit hit;
        
        // If the ability is on cooldown, skip check
        if (canElevate)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, GetLevitateRange(),
                ~(playerLayer | damageLayer)))
            {
                if (hit.collider.gameObject.layer == 8)
                {
                    if (!hit.collider.CompareTag("Bot"))
                    {
                        EnemyController ec;
                        if (hit.collider.CompareTag("Head"))
                        {
                            ec = hit.collider.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent
                                .gameObject.GetComponent<EnemyController>();
                        }
                        else
                        {
                            ec = hit.collider.gameObject.GetComponent<EnemyController>();
                        }

                        ec.SetHighlight(true);
                        levitated.Add(ec);
                        allowLevitate = true;
                    }
                }
                else
                {
                    if (levitated.Count > 0)
                    {
                        foreach (EnemyController ec in levitated)
                        {
                            ec.SetHighlight(false);
                        }
                    }

                    allowLevitate = false;
                }
            }
            else
            {
                if (levitated.Count > 0)
                {
                    foreach (EnemyController ec in levitated)
                    {
                        ec.SetHighlight(false);
                    }
                }

                allowLevitate = false;
            }
        }
        else
        {
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 0.001f);
            if (levitated.Count > 0)
            {
                foreach (EnemyController ec in levitated)
                {
                    ec.SetHighlight(false);
                }
            }

            allowLevitate = false;
        }

        return hit;
    }
    
    /// <summary>
    /// Run each frame to determine if the taken damage vignette needs to be displayed over the HUD or not
    /// </summary>
    private void CheckDamage()
    {
        if (impactTime + 1.5f >= Time.time)
        {
            damageScreen.gameObject.SetActive(true);
        }
        else
        {
            damageScreen.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Initialise player health / fully heal player
    /// </summary>
    public void SetSliderMaxHealth()
    {
        slider.maxValue = GetMaxHealth();
        currentHealth = GetMaxHealth();
        SetSliderHealth();
    }
    
    /// <summary>
    /// Update player's health display
    /// </summary>
    public void SetSliderHealth()
    {
        slider.value = currentHealth;
        healthText.text = Mathf.Ceil(currentHealth) + "    /    " + GetMaxHealth();
        healthFill.color = healthColour.Evaluate(slider.normalizedValue);
    }
    
    /// <summary>
    /// Applies text settings to a given TextMeshPro object
    /// </summary>
    /// <param name="textMesh">The TextMeshPro object which needs the settings to be applied to it</param>
    /// <param name="light">A bool which determines the color of text. Text is light when true, dark when false</param>
    /// <param name="large">A bool which determines the size of the Text. Text is larger when true, smaller when false</param>
    private void ApplyTextSettings(TextMeshProUGUI textMesh, bool light, bool large)
    {
        textMesh.font = TMP_FontAsset.CreateFontAsset(font);
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.SmallCaps;
        
        if (large)
        {
            textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
            textMesh.fontSize = largeTextSize;
        }
        else
        {
            textMesh.verticalAlignment = VerticalAlignmentOptions.Bottom;
            textMesh.fontSize = textSize;
        }
        
        if (light)
        {
            textMesh.color = lightTextColor;
        }
        else
        {
            textMesh.color = textColor;
        }
    }
    
    /// <summary>
    /// Applies daamage to player
    /// </summary>
    /// <param name="value">The damage the player has taken</param>
    public void TakeDamage(float value)
    {
        impactTime = Time.time;
        currentHealth -= value;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
        SetSliderHealth();
    }
    
    /// <summary>
    /// Applies healing to player
    /// </summary>
    /// <param name="value">The health the player has recovered</param>
    public void Heal(float value)
    {
        currentHealth += value;
        if (currentHealth >= GetMaxHealth())
        {
            currentHealth = GetMaxHealth();
        }
        SetSliderHealth();
    }

    public float GetMaxHealth()
    {
        return baseMaxHealth + (healthMultiplierAmount * (gm.healthMultiplier + healthMultiplier));
    }

    public float GetMagicRange()
    {
        return baseMagicRange + (magicRangeMultiplierAmount * (gm.magicRangeMultiplier + magicRangeMultiplier));
    }

    public float GetMagicFireRate()
    {
        float result = baseMagicFireRate + (magicRateMultiplierAmount * (gm.magicRateMultiplier + magicRateMultiplier));
        
        // Fire rate of magic cannot be increased above this point
        if (result > 4f)
        {
            return 4f;
        }
        
        return result;
    }

    public float GetMagicProjectileSpeed()
    {
        return baseMagicProjectileSpeed + (magicSpeedMultiplierAmount * (gm.magicSpeedMultiplier + magicSpeedMultiplier));
    }

    public float GetMagicDamage()
    {
        return baseMagicDamage + (magicDamageMultiplierAmount * (gm.magicDamageMultiplier + magicDamageMultiplier));
    }

    public float GetSwordAttackSpeed()
    {
        float result =  baseSwordAttackSpeed + (swordSpeedMultiplierAmount * (gm.swordSpeedMultiplier + swordSpeedMultiplier));
        
        // Fire rate of sword cannot be increased above this point
        if (result > 4f)
        {
            return 4f;
        }
        
        return result;
    }

    public float GetSwordDamage()
    {
        return baseSwordDamge + (swordDamageMultiplierAmount * (gm.swordDamageMultiplier + swordDamageMultiplier));
    }

    public float GetVineMaxExtent()
    {
        return baseVineMaxExtent + (vineExtentMultiplierAmount * (gm.vineExtentMultiplier + vineExtentMultiplier));
    }

    public float GetVineCooldownDuration()
    {
        float result = baseVineCooldown - (vineCooldownMultiplierAmount * (gm.vineCooldownMultiplier + vineCooldownMultiplier));
        
        // Cooldown of this ability cannot be reduced below this point
        if (result < 1f)
        {
            return 1f;
        }

        return result;
    }

    public float GetLevitateRange()
    {
        return baselevitateRange + (levitateRangeMultiplierAmount * (gm.levitateRangeMultiplier + levitateRangeMultiplier));
    }

    public float GetLevitateCooldownDuration()
    {
        float result = baseLevitateCooldown - (levitateCooldownMultiplierAmount * (gm.levitateCooldownMultiplier + levitateCooldownMultiplier));
        
        // Cooldown of this ability cannot be reduced below this point
        if (result < 2f)
        {
            return 2f;
        }

        return result;
    }
    
    /// <summary>
    /// Applies a short cooldown between magic blasts
    /// </summary>
    /// <returns></returns>
    IEnumerator MagicCooldown()
    {
        canAttack = false;

        primaryAttackSlot.transform.Find("Cooldown").gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1 / GetMagicFireRate());
        
        primaryAttackSlot.transform.Find("Cooldown").gameObject.SetActive(false);

        canAttack = true;
    }
    
    /// <summary>
    /// Applies a short cooldown between sword swipes
    /// </summary>
    /// <returns></returns>
    IEnumerator SwordCooldown()
    {
        canAttack = false;
        
        primaryAttackSlot.transform.Find("Cooldown").gameObject.SetActive(true);

        yield return new WaitForSeconds(1 / GetSwordAttackSpeed());
        
        primaryAttackSlot.transform.Find("Cooldown").gameObject.SetActive(false);

        canAttack = true;
    }
    
    /// <summary>
    /// Applies a cooldown between uses of the vine ability
    /// </summary>
    /// <returns></returns>
    IEnumerator VineCooldown()
    {
        canVine = false;
        
        vineAbilitySlot.transform.Find("Cooldown").gameObject.SetActive(true);
        GameObject cooldownText = vineAbilitySlot.transform.Find("CooldownText").gameObject;
        cooldownText.SetActive(true);
        TextMeshProUGUI cooldownTextNumber = cooldownText.GetComponent<TextMeshProUGUI>();
        
        string text;
        float difference = GetVineCooldownDuration() - Mathf.FloorToInt(GetVineCooldownDuration());

        for (int i = Mathf.FloorToInt(GetVineCooldownDuration()); i > 0; i--)
        {
            text = "" + i;
            cooldownTextNumber.text = text;
            yield return new WaitForSeconds(1);
        }

        text = "" + 0;
        cooldownTextNumber.text = text;
        yield return new WaitForSeconds(difference);
        
        cooldownText.SetActive(false);
        vineAbilitySlot.transform.Find("Cooldown").gameObject.SetActive(false);

        canVine = true;
    }
    
    /// <summary>
    /// Applies a cooldown between uses of the levitate ability
    /// </summary>
    /// <returns></returns>
    IEnumerator LevitateCooldown()
    {
        canElevate = false;
        
        levitateAbilitySlot.transform.Find("Cooldown").gameObject.SetActive(true);
        GameObject cooldownText = levitateAbilitySlot.transform.Find("CooldownText").gameObject;
        cooldownText.SetActive(true);
        TextMeshProUGUI cooldownTextNumber = cooldownText.GetComponent<TextMeshProUGUI>();

        string text;
        float difference = GetLevitateCooldownDuration() - Mathf.FloorToInt(GetLevitateCooldownDuration());
        
        for (int i = Mathf.FloorToInt(GetLevitateCooldownDuration()); i > 0; i--)
        {
            text = "" + i;
            cooldownTextNumber.text = text;
            yield return new WaitForSeconds(1);
        }
        
        text = "" + 0;
        cooldownTextNumber.text = text;
        yield return new WaitForSeconds(difference);
        
        cooldownText.SetActive(false);
        levitateAbilitySlot.transform.Find("Cooldown").gameObject.SetActive(false);

        canElevate = true;
    }
}

public enum AttackMode
{
    None,
    Magic,
    Sword
}
