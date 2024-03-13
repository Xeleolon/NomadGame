using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractBase : MonoBehaviour
{
    [SerializeField] float distance = 3f;
    ToolsInventory tools;

    public virtual void Interact()
    {
        Debug.Log("Interact with " + gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
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
            if (tools == null)
            {
                tools = other.GetComponent<ToolsInventory>();
            }
            tools.InteractCheck(false);
        }
    }
    
}
