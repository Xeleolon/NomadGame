using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolsInventory : MonoBehaviour
{
    // Start is called before the first frame update
    #region Inputs
    private PlayerInputActions playerControls;
    private InputAction strike;
    private InputAction flipTool;
    private InputAction swapTool;
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    void OnEnable()
    {
        strike = playerControls.Player.Fire;
        strike.Enable();
        strike.performed += FireTool;

        flipTool = playerControls.Player.FlipTool;
        flipTool.Enable();
        flipTool.performed += FlipTool;

        swapTool = playerControls.Player.SwapTool;
        swapTool.Enable();
        swapTool.performed += SwapTool;


    }

    void OnDisable()
    {
        strike.Disable();
        flipTool.Disable();
        swapTool.Disable();
    }
    #endregion

    [Header("Inventory")]
    public WeaponItem weapon;
    private int curTool;
    [SerializeField] GameObject toolObject;
    [SerializeField] GameObject torchPrefab;
    [Tooltip("0 = no torch, 3 = drenched torch, 4 = lit torch.")]
    [Range(0, 4)]
    [SerializeField] int torchState = 0; //0 = no torch, 2 = drenched torch, 4 = lit torch.

    [Header("Weapon")]
    [SerializeField] GameObject weaponObject;
    float coolDown;
    bool canHarm = true;
    public bool gamePaused;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (coolDown > 0)
        {
            coolDown -= 1 * Time.deltaTime;
        }
    }
    #region inputControl
    void FireTool(InputAction.CallbackContext context)
    {
        switch (curTool)
        {
            case 0: 
            Actack();
            break;

            case 1:
            DropTorch();
            
            break;
        }
    }

    void SwapTool(InputAction.CallbackContext context)
    {
        switch (curTool)
        {
            case 0: //change to tool mode
            if (torchState > 0)
            {
                curTool = 1;
                if (weaponObject != null && weaponObject.activeSelf) //hide weapon
                {
                    weaponObject.SetActive(false);
                }

                if (toolObject != null && !toolObject.activeSelf)
                {
                    toolObject.SetActive(true);
                }
            }
            break;

            case 1: //change to weapon mode
            curTool = 0;
            if (weaponObject != null && !weaponObject.activeSelf) //hide tool
            {
                weaponObject.SetActive(true);
            }

            if (toolObject != null && toolObject.activeSelf)
            {
                toolObject.SetActive(false);
            }
            break;
        }
    }

    void FlipTool(InputAction.CallbackContext context)
    {
        if (curTool == 0)
        {
            canHarm = !canHarm;
            if (canHarm)
            {
                Debug.Log("Weapon Set to Actack/Pointy end");
            }
            else
            {
                Debug.Log("Weapon Set to nock/studd end");
            }
            //flip tool render around as well
        }
    }

    void DropTorch()
    {
        //spawn Torch and drop it to the ground,
        if (torchState != 0)
        {
            torchState = 0;
            if (toolObject != null && toolObject.activeSelf)
            {
                toolObject.SetActive(false);
            }
            if (torchPrefab != null)
            {
                Instantiate(torchPrefab, toolObject.transform.position, Quaternion.Euler(Random.Range(-1.0f, 1.0f), 0 , Random.Range(-1.0f, 1.0f)));
            }
        }

    }


    #endregion

    #region Actacks

    void Actack()
    {
        if (coolDown <= 0 && !gamePaused && weapon != null)
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
