using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour
{

    private GameObject objectToSetActive;
    bool activate = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (activate && !objectToSetActive.activeSelf)
            {
                objectToSetActive.SetActive(true);
                return;
            }
            else if (!activate && objectToSetActive.activeSelf)
            {
                objectToSetActive.SetActive(false);
                return;
            }
            
        }
    }
}
