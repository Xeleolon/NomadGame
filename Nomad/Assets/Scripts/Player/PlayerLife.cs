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

    }

    void OnDisable()
    {
        strike.Disable();
        inputToolA.Disable();
        inputToolB.Disable();
        inputToolC.Disable();
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


    public enum ToolType {empty, spear, torch, bow, rope}

    [Header("ToolSelection")]

    [SerializeField] Transform playerBody;
    [SerializeField] ToolType toolA = ToolType.empty;
    [SerializeField] ToolType toolB = ToolType.empty;

    [SerializeField] ToolType toolC = ToolType.empty;
    [SerializeField] public ToolType curTool = ToolType.empty;
    [SerializeField] Animator toolsAnimator;
    private InteractionTrigger interactTrigger;

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
    [SerializeField] public ToolInfo torchInfo;
    [SerializeField] public int torchState = 0; //0 = no torch, 2 = lit torch, 3 = drenched torch.
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
        interactTrigger = playerBody.GetComponent<InteractionTrigger>();
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

    void FireTool(InputAction.CallbackContext context)
    {
        //complete spefic function for the tool.
        switch (curTool)
        {
            case ToolType.empty: //No Tool
            //Debug.Log("Firing at with no tool 0");
            break;

            case ToolType.spear: //Spear
            SpearActack();

            break;

            case ToolType.torch: //Torch

            break;

            case ToolType.bow: //Bow

            break;

            case ToolType.rope: //Rope 

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
        ToolSelection(toolA);
    }

    void ToolBSelect(InputAction.CallbackContext context)
    {
        ToolSelection(toolB);
    }

    void ToolCSelect(InputAction.CallbackContext context)
    {
        ToolSelection(toolC);
    }

    void ToolSelection(ToolType heldTool)
    {
        if (heldTool == ToolType.empty)
        {
            curTool = heldTool;
            ToolChange();
            return;
        }
        else if (curTool != heldTool)
        {
            curTool = heldTool;
            ToolChange();
        }
    }

    void ToolChange()
    {
        switch (curTool)
        {
            case ToolType.empty:
            //no tool
            spearInfo.ActivateObject(false);
            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);

            break;

            case ToolType.spear:
            //spear
            if (spear != null)
            { 
                spearInfo.ActivateObject(true); 
            }
            else
            {
                spearInfo.ActivateObject(false);
                Debug.Log("spear data missing");
                curTool = ToolType.empty;
            }

            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case ToolType.torch:
            //torch
            torchInfo.ActivateObject(true);

            spearInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case ToolType.bow:
            //bow
            if (bow != null)
            { 
                bowInfo.ActivateObject(true); 
            }
            else
            {
                bowInfo.ActivateObject(false);
                Debug.Log("bow data missing");
                curTool = ToolType.empty;
            }

            spearInfo.ActivateObject(false);
            torchInfo.ActivateObject(false);
            break;

            case ToolType.rope:
            //rope
            
            break;

        }

        if (interactTrigger != null)
        { 
            interactTrigger.RequirementCheck();
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
            ray = new Ray(transform.position, playerBody.forward);
    
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
    public void changeTorch(int tempTorchState)
    {
        if (torchState != 0 && curTool == ToolType.torch && torchState != tempTorchState)
        {
            switch (tempTorchState)
            {
                case 0:
                torchState = 0;
                torchInfo.ActivateObject(false);
                break;

                case 1:
                torchState = 1;
                Debug.Log("offcourse");
                if (torchLight != null && torchLight.activeSelf)
                {
                    torchLight.SetActive(false);
                }
                break;

                case 2:
                //Debug.Log("Made to stage 2");
                if (torchState != 3)
                {
                    //Debug.Log("Made to stage 3");
                    torchState = 2;
                    //turn light on
                    if (torchLight != null && !torchLight.activeSelf)
                    {
                        //Debug.Log("Made to stage 4");
                        torchLight.SetActive(true);
                    }
                }
                break;

                case 3:
                torchState = 3;
                if (torchLight != null && torchLight.activeSelf) // turn light off
                {
                    torchLight.SetActive(false);
                }
                break;
            }
        }
    }

    public bool AddTorch(int torchData)
    {
        if (torchState == 0)
        {
            torchState = 1;
            curTool = ToolType.torch;
            ToolChange();
            changeTorch(torchData);
            
            return true;
        }
        return false;
    }
    void DropTorch()
    {
        //spawn Torch and drop it to the ground,
        if (torchState != 0)
        {
            
            torchInfo.ActivateObject(false);
            if (torchPrefab != null)
            {
                GameObject tempObject = Instantiate(torchPrefab, playerBody.forward * 1.5f, Quaternion.Euler(Random.Range(-1.0f, 1.0f), 0 , Random.Range(-1.0f, 1.0f)));
                tempObject.GetComponent<PickUpTorch>().ChangeState(torchState);
            }

            torchState = 0;
        }

    }

    public void DrenchTorch(bool drench)
    {
        if (curTool == ToolType.torch)
        {
            if (drench)
            {
                torchState = 3;
                //torch become unlit
            }
            else if (torchState == 3)
            {
                torchState = 1;
            }
        }
    }
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
