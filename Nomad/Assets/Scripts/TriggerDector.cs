using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            ToolsInventory tools = other.GetComponent<ToolsInventory>();
            tools.DrenchTorch(true);
        }
    }
}
