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
    [SerializeField] public Vector2 maxCameraDistance = new Vector2(-3, -0.5f);
    [SerializeField] private float cameraMovementSpeed = 1;
    private float cameraDistance;
    private bool cameracolliding;
    float newDistance;
    void Start()
    {
        cameraDistance = maxCameraDistance.x;
    }
    void Update()
    {
        if (collision)
        {
            AlterCameraDistance(1);
        }
        else
        {
            if (Vector3.Distance(lastestCollision, transform.position) > orginalDistance - pauseRange)
            {
                
                if (!rayColliderCheck())
                {
                    lastestCollision = transform.forward * maxCameraDistance.x * 2;
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
    
        if (Physics.Raycast(ray, out hit, -maxCameraDistance.x, layerMask))
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
            
            //cameraDistance = Mathf.Lerp(maxCameraDistance.x, maxCameraDistance.y, newDistance);
            /*if (cameraDistance >= maxCameraDistance.y && distance == 1)
            {
                cameraDistance = Mathf.SmoothDamp(cameraDistance, maxCameraDistance.y, ref newDistance, cameraMovementSmoothness, cameraMovementSpeed);
            }
            else if (cameraDistance <= maxCameraDistance.x && distance == -1)
            {
                newDistance = -newDistance;
                cameraDistance = Mathf.SmoothDamp(cameraDistance, maxCameraDistance.x, ref newDistance, cameraMovementSmoothness, cameraMovementSpeed);
            }*/

            if (cameraDistance >= maxCameraDistance.y + 0.2f)
            {
                cameraDistance = maxCameraDistance.y;
            }
            else if (cameraDistance <= maxCameraDistance.x - 0.2f)
            {
                cameraDistance = maxCameraDistance.x;
            }
            else
            {
                cameraDistance = Mathf.SmoothStep(maxCameraDistance.x - 0.2f, maxCameraDistance.y + 0.2f, newDistance);
            }


            Vector3 cameraRange = transform.localPosition;
            cameraRange.z = cameraDistance;
            transform.localPosition = cameraRange;
        }
}
