using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLife : MonoBehaviour
{
    [System.Serializable]
public class ToolInfo
{
    public bool locked;
    [SerializeField] GameObject heldRefernce;
    public string interactAnimation;
    public string secondAnimation;
    public string idealAnimation;

    public void ActivateObject(bool activate)
    {
        if (heldRefernce != null)
        {
            if (activate && !heldRefernce.activeSelf)
            {
                heldRefernce.SetActive(true);
            }
            else if (!activate && heldRefernce.activeSelf)
            {
                heldRefernce.SetActive(false);
            }
        }

    }
}

    #region Awake & Inputs
    public static PlayerLife instance;
    private PlayerInputActions playerControls;
    private InputAction strike;
    private InputAction inputToolA;
    private InputAction inputToolB;
    private InputAction inputToolC;
    private InputAction interactWith;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("more than one player life!");
        }
        instance = this;
        playerControls = new PlayerInputActions();
    }

    void OnEnable()
    {
        strike = playerControls.Player.Fire;
        strike.Enable();
        strike.performed += FireTool;

        inputToolA = playerControls.Player.ToolA;
        inputToolA.Enable();
        inputToolA.performed += ToolASelect;

        inputToolB = playerControls.Player.ToolB;
        inputToolB.Enable();
        inputToolB.performed += ToolBSelect;

        inputToolC = playerControls.Player.ToolC;
        inputToolC.Enable();
        inputToolC.performed += ToolCSelect;

        interactWith = playerControls.Player.Interact;
        interactWith.Enable();
        interactWith.performed += Interact;
    }

    void OnDisable()
    {
        strike.Disable();
        inputToolA.Disable();
        inputToolB.Disable();
        inputToolC.Disable();
        interactWith.Disable();
    }
    #endregion


    [Header("Health")]
    [SerializeField] bool freazeDecays;
    [SerializeField] float health;
    float curHealth;
    [SerializeField] float hunger;
    float curHunger;
    [SerializeField] float hungerDecay;
    [Header("Inventory")]
    [SerializeField] int coins = 0;

    [SerializeField] int arrows = 10;
    [SerializeField] int maxArrows = 20;


    [Header("ToolSelection")]
    [SerializeField] Transform playerBody;
    [SerializeField] int toolA = 0;
    [SerializeField] int toolB = 0;

    [SerializeField] int toolC = 0;
    [SerializeField] int curTool = 0;
    [SerializeField] Animator toolsAnimator;

    [Header("Weapons")]
    //spear
    [SerializeField] WeaponItem spear;
    [SerializeField] ToolInfo spearInfo;
    bool canHarm = true;

    //Bow
    [SerializeField] WeaponItem bow;
    [SerializeField] ToolInfo bowInfo;
    
    [Header("Tools")]
    //torch
    [SerializeField] ToolInfo torchInfo;
    [SerializeField] int torchState = 0; //0 = no torch, 2 = lit torch, 3 = drenched torch.
    [SerializeField] GameObject torchLight;
    [SerializeField] GameObject torchPrefab;

    [Header("UI")]
    [SerializeField] GameObject healthyUiPrefab;
    [SerializeField] Transform healthParent;
    [SerializeField] Animation toolanimation;
    private Vector3 spawnPoint;


    
    [Header("Paused")]
    public bool gamePaused;


    void Start()
    {
        spawnPoint = transform.position;
        curHealth = health;
        curHunger = hunger;
        UpdateHealthUI();
        ToolChange();
    }

    void Update()
    {

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

        if (curHunger > 0)
        {
            curHunger -= hungerDecay * Time.deltaTime;
        }
        else
        {
            //AlterHealth(-(0.2f * Time.deltaTime));
        }
    }

    public void AlterHealth(float temp)
    {
        curHealth += temp;
        //Debug.Log("Player recieved damage");

        if (curHealth <= 0)
        {
            Debug.Log("Player Died");
            Respawn();
        }
        else if (curHealth > health)
        {
            curHealth = health;
        }
        UpdateHealthUI();

    }
    void Respawn()
    {
        curHealth = health;
        transform.position = spawnPoint;
    }

    public void RestRestore(Vector3 newPosition)
    {
        spawnPoint = newPosition;
        curHealth = health;
        UpdateHealthUI();
    }
    #endregion
    
    #region InputInteract/Fire
    private InteractBase interactBase;

    void Interact(InputAction.CallbackContext context) //need to swap from pure raycast to collision
    {
        if (interactBase != null)
        {
            interactBase.Interact();
        }
    }

    void FireTool(InputAction.CallbackContext context)
    {
        //complete spefic function for the tool.
        switch (curTool)
        {
            case 0: //No Tool
            //Debug.Log("Firing at with no tool 0");
            break;

            case 1: //Spear
            SpearActack();

            break;

            case 2: //Torch

            break;

            case 3: //Bow

            break;

            case 4: //Rope

            break;
        }
    }

    void Reset()
    {
        if (!canHarm)
        {
            canHarm = true;
        }
    }

    #endregion
    #region ToolSelection

    void ToolASelect(InputAction.CallbackContext context)
    {
        if (toolA == 0)
        {
            Debug.Log("No tool in selection A");
            curTool = toolC;
            ToolChange();
            return;
        }
        else if (curTool != toolA)
        {
            Debug.Log("set tool to A");
            curTool = toolA;
            ToolChange();
        }
    }

    void ToolBSelect(InputAction.CallbackContext context)
    {
        if (toolB == 0)
        {
            Debug.Log("No tool in selection B");
            curTool = toolC;
            ToolChange();
            return;
        }
        else if (curTool != toolB)
        {
            Debug.Log("set tool to B");
            curTool = toolB;
            ToolChange();
        }
    }

    void ToolCSelect(InputAction.CallbackContext context)
    {
        if (toolC == 0)
        {
            Debug.Log("No tool in selection C");
            curTool = toolC;
            ToolChange();
            return;
        }
        else if (curTool != toolC)
        {
            Debug.Log("set tool to C");
            curTool = toolC;
            ToolChange();
        }
    }

    void ToolChange()
    {
        switch (curTool)
        {
            case 0:
            //no tool
            spearInfo.ActivateObject(false);
            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);

            break;

            case 1:
            //spear
            spearInfo.ActivateObject(true);

            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case 2:
            //torch
            torchInfo.ActivateObject(true);

            spearInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case 3:
            //bow
            bowInfo.ActivateObject(true);

            spearInfo.ActivateObject(false);
            torchInfo.ActivateObject(false);
            break;

            case 4:
            //rope
            
            break;

        }
    }
    #endregion

    #region Spear
    void SpearActack()
    {
        if (canHarm)
        {
            Debug.Log("spear Actacked");
            if (spearInfo.interactAnimation != null)
            {
                toolsAnimator.Play(spearInfo.interactAnimation);
            }
            Ray ray;
            ray = new Ray(gameObject.transform.position, playerBody.forward);
    
            RaycastHit hit;
    
            int layerMask = 1 << 10;
            layerMask = ~layerMask;
    
            if (Physics.Raycast(ray, out hit, spear.range, layerMask))
            {
                EmenyHealth emeny = hit.collider.gameObject.GetComponent<EmenyHealth>();
                if (emeny != null)
                {
                    emeny.Damage(spear.damage);
                    Debug.Log("Hit " + hit.collider.gameObject.name);
                }
            }
            Invoke(nameof(Reset), spear.actackSpeed);
        }
    }
    #endregion

    #region Torch

    #endregion


    #region UI

    void UpdateHealthUI()
    {
        int numChildren = healthParent.childCount;
        int intHealth = -(Mathf.FloorToInt(-curHealth));
        //Debug.Log(numChildren + " " + intHealth + " " + curHealth);
        if (numChildren != intHealth)
        {
            if (numChildren > intHealth)
            {
                Debug.Log("removing ui element");
                //int target = numChildren - intHealth
                for (int i = numChildren - intHealth; i > 0; i--)
                {
                    //remove extra element
                    Destroy(healthParent.GetChild(0).gameObject);
                }
            }
            else
            {
                for (int i = intHealth- numChildren; i > 0; i--)
                {
                    //remove extra element
                    Instantiate(healthyUiPrefab, healthParent);
                    Debug.Log("add health element");
                }
            }
        }


    }


    #endregion
    
    


}
