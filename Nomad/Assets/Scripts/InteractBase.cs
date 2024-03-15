using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractBase : MonoBehaviour
{
    [Header("Interact Base")]
    [SerializeField] float distance = 3f;
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            triggerEnterUsed = true;
            if (tools == null)
            {
                tools = other.GetComponent<ToolsInventory>();
            }
            tools.InteractCheck(true);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (!!triggerEnterUsed && other.gameObject.tag == "Player")
        {
            triggerEnterUsed = true;
            if (tools == null)
            {
                tools = other.GetComponent<ToolsInventory>();
            }
            tools.InteractCheck(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            triggerEnterUsed = false;
            if (tools == null)
            {
                tools = other.GetComponent<ToolsInventory>();
            }
            tools.InteractCheck(false);
        }
    }
    
}
