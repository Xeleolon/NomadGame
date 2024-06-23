using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeCamera : MonoBehaviour
{
    public Camera alternativeCamera;
    private PlayerMovement playerMovement;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (playerMovement == null)
            {
                playerMovement = PlayerMovement.instance;
            }
            playerMovement.AddCamera(alternativeCamera);
        }


    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (playerMovement == null)
            {
                playerMovement = PlayerMovement.instance;
            }
            playerMovement.RemoveCamera(alternativeCamera);
        }
    }
}
