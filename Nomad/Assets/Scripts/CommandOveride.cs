using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandOveride : MonoBehaviour
{
    // Start is called before the first frame update
    public string instructionText;
    public string keyText;
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.ActiveInstructionPanel(true, instructionText, keyText);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.ActiveInstructionPanel(false, instructionText, keyText);
        }
    }
}
