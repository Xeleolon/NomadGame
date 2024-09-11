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
    Transform player;


    [SerializeField] private float alterMinCoolDown = 0.2f;

    private float alterClock;

    private enum CameraDirection {forward, backwards, changingForward, changingBackwards, neutral}
    CameraDirection cameraDirection = CameraDirection.neutral;
    private float rateToPlace;
    void Start()
    {
        startPlace = transform.localPosition.z;
        cameraCurSeat = cameraRange.x;
        
        player = GameObject.FindWithTag("Player").transform;
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
                cameraCurSeat = LeftCollisionRayCast();
                //Debug.Log("reset to origin " + cameraCurSeat);
            }
            else if (cameraDirection == CameraDirection.forward)
            {
                cameraDirection = CameraDirection.changingBackwards;
                alterClock = 0;
            }
        }
        MoveCameraClear();

    }

    float LeftCollisionRayCast()
    {
        RaycastHit hit;
        float range = cameraRange.x + transform.position.x;
        int layerMask = 1 << 14;
        layerMask = ~layerMask;
        if (Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.forward), out hit, range, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(-Vector3.forward) * hit.distance, Color.yellow);
            return -hit.distance + offSet;
        }

        return cameraRange.x;
    }

    void MoveCameraClear()
    {
        if (cameraDirection == CameraDirection.changingForward || cameraDirection == CameraDirection.changingBackwards)
        {
            //Debug.Log("camera changing Direction " + cameraDirection + " " + cameraCurSeat);
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

    //int checker;

    void SetTargetDistance(Vector3 newDistance)
    {
        //checker += 1;

        //newDistance = transform.TransformDirection(newDistance);
        float newCameraSeat = -Vector3.Distance(newDistance, player.position) - offSet;

        //Debug.Log(checker + " zero checkPoint checking status of distatnce " + newDistance);

        //newDistance = new Vector3(0, 0, newDistance.z);
        //float newCameraSeat = newDistance.z - offSet;

        //delay input changes that occur in reverse direction\\
        //Debug.Log(checker + " First checkPoint checking status of distatnce" + newCameraSeat + " seat without offSet " + (newCameraSeat + offSet) + " " +  " + distance  " + newDistance);


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
            //Debug.Log(newCameraSeat + " " + cameraCurSeat + " " + cameraDirection + " collision dectection " + collision );
            rateToPlace = 0;
            startPlace = transform.localPosition.z;
            cameraCurSeat = newCameraSeat;
        }
        //Debug.Log(checker + " Second checkPoint checking status of distatnce" + newCameraSeat);

        void CameraDirectionChecker()
        {
            startPlace = transform.localPosition.z;
            if (newCameraSeat > startPlace)
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
            else if (newCameraSeat < startPlace) //check if camera is going to move backwards 
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
