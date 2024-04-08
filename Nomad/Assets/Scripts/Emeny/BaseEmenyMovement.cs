using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEmenyMovement : MonoBehaviour
{
    [Header("Basic Emeny Movement")]
    [SerializeField] float rotationSpeed;

    Vector3 Velocity;
    Rigidbody rb;
    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /*void Update()
    {
        TestBasicMovement();
        if (rotate)
        {//transform.LookAt(rotationTarget, Vector3.up);
            Vector3 targetRotation = rotationTarget - transform.position;
            targetRotation.y = transform.position.y;
            Vector3 newRotation = Vector3.RotateTowards(transform.forward, targetRotation, rotationSpeed * 2 * Time.deltaTime, 0.0f);
            Quaternion deltaRotation = Quaternion.LookRotation(newRotation);
            rb.rotation = deltaRotation;
            //transform.rotation = deltaRotation;
        }
    }*/

    void FixedUpdate()
    {
        if (Velocity != Vector3.zero)
        {
            rb.AddForce(Velocity, ForceMode.Force);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void Move(Vector2 speed, Vector3 rotationTarget, bool rotate)
    {
        Velocity = Vector3.zero;
        if (speed.x != 0)
        {
            Velocity = transform.forward * speed.x * Time.deltaTime;
        }
        
        if (speed.y != 0)
        {
            Velocity += transform.right * speed.y * Time.deltaTime;
        }
        Debug.Log(speed + " " + Velocity);

        if (rotate)
        {//transform.LookAt(rotationTarget, Vector3.up);
            Vector3 targetRotation = rotationTarget - transform.position;
            targetRotation.y = transform.position.y;
            
            Vector3 newRotation = Vector3.RotateTowards(transform.forward, targetRotation, rotationSpeed * 2 * Time.deltaTime, 0.0f);
            Quaternion deltaRotation = Quaternion.LookRotation(newRotation);
            //rb.rotation = deltaRotation;
            transform.rotation = deltaRotation;
        }


    }



    bool LocationMatch(Vector3 check1, Vector3 check2)
    {
        if (check1.x == check2.x && check1.z == check2.z)
        {
            return true;
        }
        return false;
    }
}
