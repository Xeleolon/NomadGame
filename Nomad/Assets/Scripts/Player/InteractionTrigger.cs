using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionTrigger : MonoBehaviour
{
    #region Inputs
    private PlayerInputActions playerControls;
    private InputAction interactWith;
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    void OnEnable()
    {
        interactWith = playerControls.Player.Interact;
        interactWith.Enable();
        interactWith.performed += Interact;
    }

    void OnDisable()
    {

        interactWith.Disable();
    }

    #endregion
    

    
    private GameObject lastObjectRef;
    private InteractBase interactBase;

    [Header("UI")]
    [SerializeField] TMP_Text functionName;
    [SerializeField] GameObject interactPopup;
    void Start()
    {
        if (interactBase == null && interactPopup.activeSelf)
        {
            interactPopup.SetActive(false);
        }
    }
    
    void Interact(InputAction.CallbackContext context)
    {
        if (interactBase != null)
        {
            interactBase.Interact();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        InteractBase newInteractBase;

        newInteractBase = other.gameObject.GetComponent<InteractBase>();
        if (newInteractBase == null)
        {
            return;
        }
        Debug.Log(" test working");
        lastObjectRef = other.gameObject;
        interactBase = newInteractBase;

        //change display label
        string display = interactBase.displayInstructions;

        if (display == "")
        {
            display = other.gameObject.name;
        }
        
        if (functionName != null)
        {
            functionName.SetText(display);
        }

            

        RequirementCheck();

        
    }

    public void RequirementCheck()
    {
        if (interactBase != null && interactPopup != null)
        {
            bool check = interactBase.Requirements();

            if (check && !interactPopup.activeSelf)
            {
                interactPopup.SetActive(true);

                string display = interactBase.displayInstructions;

                if (display != "")
                {
                    if (functionName != null)
                    {
                        functionName.SetText(display);
                    }
                }

            }
            else if (!check && interactPopup.activeSelf)
            {
                interactPopup.SetActive(false);
            }

            
        }
        else if (interactBase == null && interactPopup != null && interactPopup.activeSelf)
        {
            interactPopup.SetActive(false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != lastObjectRef)
        {
            return;
        }
        Debug.Log("Exiting " + other.gameObject);
        interactBase = null;

        if (interactPopup != null && interactPopup.activeSelf)
        {
            interactPopup.SetActive(false);
        }
        

    }
}
