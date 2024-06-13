using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractBase : MonoBehaviour
{
    [Header("Interact Base")]
    public string displayInstructions;
    //[SerializeField] float distance = 3f;
    [Tooltip("0 for open&Close, 1 for Open, 2 for Close")]
    [SerializeField] public DoorControl door;
    [SerializeField] int doorOpeningState;
    bool triggerEnterUsed;
    ToolsInventory tools;

    public virtual void Interact()
    {
        Debug.Log("Interact with " + gameObject.name);
    }

    public virtual void OpenDoor()
    {
        if (door != null)
        {
            door.OpenDoor(doorOpeningState);
        }
    }

    public virtual bool Requirements()
    {
        return true;
    }

    /*
    [SerializeField] bool triggerInteract;
    void OnTriggerEnter(Collider other)
    {
        if (triggerInteract && other.gameObject.tag == "Player")
        {
            Debug.Log("Testing");
            InteractionTrigger.instance.InteractCheck(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (triggerInteract && other.gameObject.tag == "Player")
        {
            InteractionTrigger.instance.InteractExiting(other.gameObject);
        }
    }*/
    
}
