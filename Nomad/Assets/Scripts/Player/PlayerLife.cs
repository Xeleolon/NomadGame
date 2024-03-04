using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLife : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] bool freazeDecays;
    [SerializeField] float health;
    float curHealth;
    [SerializeField] float thirst;
    float curThrist;
    [SerializeField] float thirstDecay;
    [SerializeField] float hunger;
    float curHunger;
    [SerializeField] float hungerDecay;
    [Header("Inventory")]
    [SerializeField] float waterSkinSize = 10;
    [SerializeField] float waterSkinFill = 10;


    
    [Header("Paused")]
    public bool gamePaused;
    public static PlayerLife instance;
    [Header("Actack")]
    float coolDown;
    bool canHarm = true;
    public WeaponItem weapon;

    #region Inputs
    private PlayerInputActions playerControls;
    private InputAction strike;
    void Awake()
    {
        playerControls = new PlayerInputActions();
        if (instance != null)
        {
            Debug.LogWarning("more than one player life!");
        }
        instance = this;
    }
    void OnEnable()
    {
        strike = playerControls.Player.Hit;
        strike.Enable();
        strike.performed += Actack;
    }

    void OnDisable()
    {
        strike.Disable();
    }
    #endregion

    void Start()
    {
        curHealth = health;
        curHunger = hunger;
        curThrist = thirst;
    }

    void Update()
    {
        if (coolDown > 0)
        {
            coolDown -= 1 * Time.deltaTime;
        }

        if (!gamePaused)
        {
            if (!freazeDecays)
            {
                SurvivalDecays();
            }
        }

    }
    #region Survial

    void SurvivalDecays()
    {
        if (curThrist > 0)
        {
            curThrist -= thirstDecay * Time.deltaTime;
        }
        else
        {
            AlterHealth(0.2f * Time.deltaTime);
        }

        if (curHunger > 0)
        {
            curHunger -= hungerDecay * Time.deltaTime;
        }
        else
        {
            AlterHealth(0.2f * Time.deltaTime);
        }
    }

    public void AlterHealth(float temp)
    {
        curHealth += temp;

        if (curHealth <= 0)
        {
            Debug.Log("Player Died");
        }
        else if (curHealth > health)
        {
            curHealth = health;
        }

    }
    #endregion
    #region Actacks

    void Actack(InputAction.CallbackContext context)
    {
        if (coolDown <= 0 && !gamePaused)
        {
            switch (weapon.type)
            {
                case 1:
                SpearActack();
                break;

                case 2:

                break;
            }
            Debug.Log("actack");
        }
    }

    void SpearActack()
    {
        if (canHarm)
        {
            Ray ray;
            ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
    
            RaycastHit hit;
    
            int layerMask = 1 << 10;
            layerMask = ~layerMask;
    
            if (Physics.Raycast(ray, out hit, weapon.range, layerMask))
            {
                EmenyHealth emeny = hit.collider.gameObject.GetComponent<EmenyHealth>();
                if (emeny != null)
                {
                    emeny.Damage(weapon.damage);
                    Debug.Log("Hit " + hit.collider.gameObject.name);
                    coolDown = weapon.actackSpeed;
                }
            }
        }
    }
    #endregion


}
