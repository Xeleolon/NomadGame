using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private string[] avoidingTags;
    [SerializeField] private float offSet = 0.2f;
    [SerializeField] private Vector2 cameraRange = new Vector2(-3, 0.5f);

    private float cameraCurSeat; //float for the target of the camera position;
    [SerializeField] private float speed = 2;
    private float startPlace;
    bool collision;
    int collisionCount;
    public bool bowCameraSetting;


    [SerializeField] private float alterMinCoolDown = 0.2f;

    private float alterClock;

    private enum CameraDirection {forward, backwards, changingForward, changingBackwards, neutral}
    CameraDirection cameraDirection = CameraDirection.neutral;
    private float rateToPlace;
    void Start()
    {
        startPlace = transform.localPosition.z;
        cameraCurSeat = cameraRange.x;
    }

    void Update()
    {
        if (!collision && cameraCurSeat != cameraRange.x) //set place to distance rest
        {
            //Debug.Log("active");
            if (cameraDirection == CameraDirection.backwards || cameraDirection == CameraDirection.changingForward || cameraDirection == CameraDirection.neutral)
            {
                cameraDirection = CameraDirection.backwards;
                rateToPlace = 0;
                startPlace = transform.localPosition.z;
                cameraCurSeat = cameraRange.x;
            }
            else if (cameraDirection == CameraDirection.forward)
            {
                cameraDirection = CameraDirection.changingBackwards;
                alterClock = 0;
            }
        }
        MoveCameraClear();

    }

    void MoveCameraClear()
    {
        if (cameraDirection == CameraDirection.changingForward || cameraDirection == CameraDirection.changingBackwards)
        {
            Debug.Log("camera changing Direction " + cameraDirection + " " + alterClock);
            if (alterClock >= alterMinCoolDown)
            {
                alterClock = 0;
                if (cameraDirection == CameraDirection.changingForward)
                {
                    cameraDirection = CameraDirection.forward;
                    return;
                }
                else if (cameraDirection == CameraDirection.changingBackwards)
                {
                    cameraDirection = CameraDirection.backwards;
                    return;
                }
            }
            else
            {
                alterClock += 1 * Time.deltaTime;
            }
        }
        else
        {
            Vector3 cameraCurrent = transform.localPosition;
            float cameraDistance = cameraCurSeat;
            if (bowCameraSetting)
            {
                cameraDistance = cameraRange.y;
            }

            if (cameraDistance != cameraCurrent.z)
            {
                rateToPlace += speed * Time.deltaTime;
                rateToPlace = Mathf.Clamp(rateToPlace, 0, 1);

                //float newPlacement = Mathf.SmoothStep(startPlace, cameraDistance, rateToPlace);
                float newPlacement = Mathf.Lerp(startPlace, cameraDistance, rateToPlace);
                newPlacement = Mathf.Clamp(newPlacement, cameraRange.x, cameraRange.y);
                cameraCurrent.z = newPlacement;
                transform.localPosition = cameraCurrent;
            }
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            SetTargetDistance(other.ClosestPointOnBounds(transform.position));
            if (collisionCount == 0)
            {
                collision = true;
            }
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
    void OnTriggerExit(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            SetTargetDistance(other.ClosestPointOnBounds(transform.position));
            collisionCount -= 1;
            if (collisionCount <= 0)
            {
                collisionCount = 0;
                collision = false;
            }
        }
    }

    void SetTargetDistance(Vector3 newDistance)
    {
        newDistance = new Vector3(0, 0, newDistance.z);
        //newDistance = transform.TransformDirection(newDistance);
        float newCameraSeat = newDistance.z - offSet;


        //delay input changes that occur in reverse direction

            if (newCameraSeat >= cameraRange.y)
            {
                newCameraSeat = cameraRange.y;
            }
            else if (newCameraSeat <= cameraRange.x)
            {
                newCameraSeat = cameraRange.x;
            }

            if (newCameraSeat != cameraCurSeat) //check if the location is not already in use
            {
                CameraDirectionChecker();
                Debug.Log(newCameraSeat + " " + cameraCurSeat + " " + cameraDirection + " collision dectection " + collision );
                rateToPlace = 0;
                startPlace = transform.localPosition.z;
                cameraCurSeat = newCameraSeat;
            } 

        void CameraDirectionChecker()
        {
            if (newCameraSeat < cameraCurSeat) //check if camera is going to move forward
            {

                switch (cameraDirection)
                {
                    case CameraDirection.neutral:
                        cameraDirection = CameraDirection.forward;
                        break;

                    case CameraDirection.backwards:
                        alterClock = 0;
                        cameraDirection = CameraDirection.changingForward;
                        break;

                    case CameraDirection.changingForward:
                        cameraDirection = CameraDirection.forward;
                        break;
                }
            }
            else if (newCameraSeat > cameraCurSeat) //check if camera is going to move backwards 
            {
                switch (cameraDirection)
                {
                    case CameraDirection.neutral:
                        cameraDirection = CameraDirection.forward;
                        break;

                    case CameraDirection.forward:
                        alterClock = 0;
                        cameraDirection = CameraDirection.changingBackwards;
                        break;

                    case CameraDirection.changingBackwards:
                        cameraDirection = CameraDirection.forward;
                        break;
                }
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
