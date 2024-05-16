using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDector : MonoBehaviour
{
    public bool drench = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife tools = other.GetComponent<PlayerLife>();
            tools.DrenchTorch(drench);
        }
    }
}
