using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public PlayerMovement player;
    bool collision;
    int collisionCount;
    public string[] avoidingTags;
    void Update()
    {
        if (collision)
        {
            player.AlterCameraDistance(1);
        }
        else
        {
            player.AlterCameraDistance(-1);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            if (collisionCount == 0)
            {
                collision = true;
            }
            collisionCount += 1;
        }
        
    }
    
    void OnTriggerExit(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            collisionCount -= 1;
            if (collisionCount <= 0)
            {
                collisionCount = 0;
                collision = false;
            }
        }
    }

    bool CheckCollisionType(GameObject other)
    {
        for (int i = 0; i < avoidingTags.Length; i++)
        {
            if (other.tag == avoidingTags[i])
            {
                return false;
            }
        }
        
        return true;
        
    }
}
