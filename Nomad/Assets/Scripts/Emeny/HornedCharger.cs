using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornedCharger : MonoBehaviour
{
    //possible could be the base script for Emeny movement, if built right
    [SerializeField] float speedCharge = 6;
    [SerializeField] float speedNetral = 3;
    [SerializeField] float rotationSpeed;

    public bool testing;
    public Transform TestDesitation;
    [SerializeField] Vector3 Velocity;
    [SerializeField] Vector3 rotationTarget;
    [SerializeField] bool rotate;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        TestBasicMovement();
        if (rotate)
        {transform.LookAt(rotationTarget, Vector3.up);}
    }

    void FixedUpdate()
    {
        rb.AddForce(Velocity, ForceMode.Force);
    }

    void TestBasicMovement()
    {
        if (testing && !LocationMatch(transform.position, TestDesitation.position))
        {
            Velocity = transform.forward * speedNetral * Time.deltaTime;
            rotationTarget = TestDesitation.position;
            rotate = true;
        }
        else
        {
            rotate = false;
            Velocity = Vector3.zero;
            rotationTarget = transform.forward;
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
