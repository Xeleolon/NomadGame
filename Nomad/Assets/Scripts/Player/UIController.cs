using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour //focus on controlling UI and pla
{
    public MapData[] dungeonMap;
    public GameObject MapUI;


    #region Inputs
    private PlayerInputActions playerControls;
    private InputAction menu;
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    void OnEnable()
    {
        menu = playerControls.Player.Inventory;
        menu.Enable();
        menu.performed += OpenInventory;
    }

    void OnDisable()
    {
        menu.Disable();
    }
    #endregion

    void OpenInventory(InputAction.CallbackContext context)
    {
        if (MapUI != null)
        {
            if (!MapUI.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                MapUI.SetActive(true);
                
            }
            else if (MapUI.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                //close
                MapUI.SetActive(false);
                
            }
            
        }
    }
}
