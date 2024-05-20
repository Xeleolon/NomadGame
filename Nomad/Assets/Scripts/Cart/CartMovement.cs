using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartMovement : MonoBehaviour
{
    [Header ("Path")]
    public Vector3[] path;

    public int pathTarget = -1;
    [SerializeField] bool followPath;
    [SerializeField] bool loop;

    [Header ("Movement")]
    [SerializeField] float speed = 6;
    [SerializeField] Vector2 minMaxSpeed = new Vector2(3, 9);
    [SerializeField] float rotationSpeed = 1;
    public float offShootRange = 0.1f;
    [SerializeField] CartTrack LineA;

    void Update()
    {
        if (followPath)
        {
            if (path.Length > 0)
            {
                if (pathTarget < 0)
                {
                    transform.position = path[0];
                    pathTarget += 1;
                }
                else if (pathTarget >= path.Length)
                {
                    if (loop)
                    {
                        pathTarget = 0;
                    }
                    else 
                    {
                        followPath = false;
                        pathTarget = -1;
                    }
                }
                else
                {
                    PathLoop();
                }
            }
            else
            {
                UpdateCartTrack();
            }
        }
    }

    void PathLoop()
    {
        if (Vector3.Distance(transform.position, path[pathTarget]) > offShootRange)
        {
            float curSpeed = speed;
            //Transform rotation = transform.rotation.localEulerAngles;
            if (transform.eulerAngles.x > 0.5f)
            {
                curSpeed = minMaxSpeed.x;
            }
            else if (transform.eulerAngles.x < 0.5f)
            {
                curSpeed = minMaxSpeed.y;
            }

            Debug.Log(transform.eulerAngles.x + " roation & curSpeed " + curSpeed);

            transform.position = Vector3.MoveTowards(transform.position, path[pathTarget], curSpeed * Time.deltaTime);
            //transform.LookAt(path[pathTarget], Vector3.up);

            //Rotation

            Vector3 targetRotation = path[pathTarget] - transform.position;
            
            Vector3 newRotation = Vector3.RotateTowards(transform.forward, targetRotation, rotationSpeed * 2 * Time.deltaTime, 0.0f);
            Quaternion deltaRotation = Quaternion.LookRotation(newRotation);
            transform.rotation = deltaRotation;
        }
        else
        {
            pathTarget += 1;
        }
    }

    void UpdateCartTrack()
    {
        if (LineA == null && LineA.line.Length == 0)
        {
            followPath = false;
            return;
        }

        path = LineA.line;

    }

}
