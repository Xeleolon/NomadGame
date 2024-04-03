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
    
    private float rateToPlace;
    void Start()
    {
        startPlace = transform.localPosition.z;
        cameraCurSeat = cameraRange.x;
    }
    
    void Update()
    {
        MoveCameraClear();

    }

    void MoveCameraClear()
    {
        Vector3 cameraCurrent = transform.localPosition;

        if (cameraCurSeat != cameraCurrent.z)
        {
            rateToPlace += speed * Time.deltaTime;
            rateToPlace = Mathf.Clamp(rateToPlace, 0, 1);

            float newPlacement = Mathf.SmoothStep(startPlace, cameraCurSeat, rateToPlace);
            cameraCurrent.z = newPlacement;
            transform.localPosition = cameraCurrent;
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if (CheckCollisionType(other.gameObject))
        {
            SetTargetDistance(other.ClosestPointOnBounds(transform.position));
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
        newDistance = transform.InverseTransformDirection(newDistance);
        float newCameraSeat = newDistance.z - offSet;

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
            rateToPlace = 0;
            startPlace = transform.localPosition.z;
            cameraCurSeat = newCameraSeat;;
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
