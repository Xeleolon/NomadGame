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
    public static InteractionTrigger instance;
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
    [SerializeField] bool activeDebug;
    
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
        InteractCheck(other.gameObject);        
    }

    public void InteractCheck(GameObject other)
    {
        InteractBase newInteractBase;

        newInteractBase = other.GetComponent<InteractBase>();
        if (newInteractBase == null)
        {
            DebugActivation(new string(other.name + " no interactbase decected"));
            return;
        }
        lastObjectRef = gameObject;
        interactBase = newInteractBase;

        //change display label
        string display = interactBase.displayInstructions;

        if (display == "")
        {
            display = gameObject.name;
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
        InteractExiting(other.gameObject);

    }

    public void InteractExiting(GameObject other)
    {
        if (gameObject != lastObjectRef)
        {
            return;
        }
        DebugActivation(new string("Exiting " + gameObject));
        interactBase = null;

        if (interactPopup != null && interactPopup.activeSelf)
        {
            interactPopup.SetActive(false);
        }
    }

    void DebugActivation(string debugText)
    {
        if (activeDebug)
        { Debug.Log(debugText); }
    }
}
