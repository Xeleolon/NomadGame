using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public PlayerMovement player;
    bool collision;
    Vector3 lastestCollision;
    int collisionCount;
    public float pauseRange = 3;
    public string[] avoidingTags;
    void FixedUpdate()
    {
        if (collision)
        {
            player.AlterCameraDistance(1);
        }
        else
        {
            if (Vector3.Distance(lastestCollision, transform.position) > pauseRange)
            {
                
                if (!rayColliderCheck())
                {
                    lastestCollision = transform.forward * player.maxCameraDistance.x * 2;
                }
                player.AlterCameraDistance(-1);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            lastestCollision = other.ClosestPointOnBounds(transform.position);
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
    bool rayColliderCheck()
    {
        Ray ray;
        ray = new Ray(transform.position, transform.TransformDirection(Vector3.back));
    
        RaycastHit hit;
    
        int layerMask = 1 << 10;
        layerMask = ~layerMask;
    
        if (Physics.Raycast(ray, out hit, -player.maxCameraDistance.x, layerMask))
        {
            lastestCollision = hit.point;
            return true;
        }
        
        return false;
    }

    float collisionPointToFloat(Vector3 collisionPoint)
    {
        return Vector3.Distance(collisionPoint, transform.position);
    }


    
}
