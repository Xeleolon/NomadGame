using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeCamera : MonoBehaviour
{
    [SerializeField] private Camera camera;
    private PlayerMovement playerMovement;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (playerMovement == null)
            {
                playerMovement = PlayerMovement.instance;
            }
            playerMovement.AddCamera(camera);
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
            playerMovement.RemoveCamera(camera);
        }
    }
}
