using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwingMovementTest : MonoBehaviour
{
    #region Awake & Inputs
    private PlayerInputActions playerControls;
    private InputAction move;
    
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

    }

    void OnDisable()
    {
        move.Disable();
    }
    #endregion
    Rigidbody rb;

    public bool setCenterOfMass;
    bool curCenterOfmass;
    public float swingHeight;

    public bool freaze;
    public float speed;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (setCenterOfMass)
        {
            rb.centerOfMass = new Vector3(transform.position.x, transform.position.y + swingHeight, transform.position.z);
        }
        
        curCenterOfmass = setCenterOfMass;
    }

    void Update()
    {
        if (setCenterOfMass != curCenterOfmass)
        {
            if (setCenterOfMass)
            {
                rb.centerOfMass = new Vector3(transform.position.x, transform.position.y + swingHeight, transform.position.z);
            }
            else
            {
                rb.ResetCenterOfMass();
            }

            curCenterOfmass = setCenterOfMass;
        }

        if (freaze)
        {
            rb.velocity = Vector3.zero;
            rb.AddRelativeTorque(Vector3.zero, ForceMode.Force);
        }

        Vector2 inputVariables = move.ReadValue<Vector2>();
        if (inputVariables != Vector2.zero)
        {
            rb.AddForce(transform.right * speed * Time.deltaTime, ForceMode.Force);
            //rb.AddRelativeTorque(transform.right * speed * Time.deltaTime, ForceMode.Force);
        }
    }
}
