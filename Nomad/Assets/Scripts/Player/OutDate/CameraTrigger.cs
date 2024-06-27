using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    bool collision;
    Vector3 lastestCollision;
    float orginalDistance;
    int collisionCount;
    public float pauseRange = 3;
    public string[] avoidingTags;
    [Header("Camera Movement")]
    [SerializeField] public Vector2 cameraRange = new Vector2(-3, -0.5f);
    [SerializeField] private float cameraMovementSpeed = 1;
    [SerializeField] private float offSet = 0.2f;
    [SerializeField] private float borderSpot;
    [SerializeField] private bool Test;
    public bool bowCameraPosition;
    private float cameraDistance;
    private bool cameracolliding;
    float newDistance;
    void Start()
    {
        cameraDistance = cameraRange.x;
        borderSpot = cameraDistance;
    }
    void Update()
    {
        if (bowCameraPosition || collision)
        {
            if (borderSpot < cameraDistance)
            {
                if (!Test)
                {
                    AlterCameraDistance(1);
                }
                Debug.Log("I Work Here");
            }
            else if (borderSpot > cameraDistance)
            {
                if (Test)
                {
                    AlterCameraDistance(1);
                }
                Debug.Log("I Not Working");
            }
        }
        else
        {
            if (Vector3.Distance(lastestCollision, transform.position) > orginalDistance - pauseRange)
            {
                
                if (!rayColliderCheck())
                {
                    lastestCollision = transform.forward * cameraRange.x * 2;
                }
                AlterCameraDistance(-1);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            lastestCollision = other.ClosestPointOnBounds(transform.position);
            orginalDistance = Vector3.Distance(lastestCollision, transform.position);
            if (collisionCount == 0)
            {
                collision = true;
            }
            SetTargetDistance(other.ClosestPointOnBounds(transform.position));
            //Debug.Log("This is what was given " + other.ClosestPointOnBounds(transform.position));
            
            collisionCount += 1;
        }
        
    }
    void OnTriggerStay(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            SetTargetDistance(other.ClosestPointOnBounds(transform.position));
        }
    }

    void SetTargetDistance(Vector3 newDistance)
    {
        newDistance = new Vector3(0, 0, newDistance.z);
        Debug.Log("This is what has been made " + newDistance);
        
        //newDistance = transform.TransformDirection(newDistance);
        Debug.Log("This is what has been made " + newDistance);
        float newBorderSpot = newDistance.z - offSet;
        Debug.Log("This is the pre border spot" + newBorderSpot);
        if (newBorderSpot >= cameraRange.y)
        {
            newBorderSpot = cameraRange.y;
        }
        else if (newBorderSpot <= cameraRange.x)
        {
            newBorderSpot = cameraRange.x;
        }
        Debug.Log("This is the Post border spot" + newBorderSpot + " " + borderSpot);

        if (borderSpot != newBorderSpot) //check if the location is not already in use
        {
            borderSpot = newBorderSpot;
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
    
        if (Physics.Raycast(ray, out hit, -cameraRange.x, layerMask))
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


    public void AlterCameraDistance(float distance)
        {
            newDistance += distance * Time.deltaTime * cameraMovementSpeed;


            newDistance = Mathf.Clamp(newDistance, 0, 1);
            
            

            if (cameraDistance >= cameraRange.y + 0.2f)
            {
                cameraDistance = cameraRange.y;
            }
            else if (cameraDistance <= cameraRange.x - 0.2f)
            {
                cameraDistance = cameraRange.x;
            }
            else
            {
                cameraDistance = Mathf.SmoothStep(cameraRange.x, cameraRange.y, newDistance);
            }


            Vector3 newPlacement = transform.localPosition;
            newPlacement.z = cameraDistance;
            transform.localPosition = newPlacement;
        }
}
