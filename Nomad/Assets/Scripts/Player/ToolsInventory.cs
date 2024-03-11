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

    [Header("Weapon")]
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
            
            
            break;
        }
    }

    void SwapTool(InputAction.CallbackContext context)
    {
        switch (curTool)
        {
            case 0: //change to tool mode
            curTool = 1;
            //hide tool
            break;

            case 1: //change to weapon mode
            curTool = 0;
            //hide weapon
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


    #endregion

    #region Actacks

    void Actack()
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
