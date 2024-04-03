using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Inputs/Awake,OnEnable,OnDisable
    private PlayerInputActions playerControls;
    void Awake()
    {
        playerControls = new PlayerInputActions(); 
    }

    private InputAction moveInputs;
    private InputAction jumpInput;
    private InputAction lookInput;
    
    void OnEnable()
    {
        moveInputs = playerControls.Player.Move;
        moveInputs.Enable();

        jumpInput = playerControls.Player.Jump;
        jumpInput.Enable();

        lookInput = playerControls.Player.Look;
        lookInput.Enable();
    }

    void OnDisable()
    {
        moveInputs.Disable();
        jumpInput.Disable();
    }

    #endregion

    //refernces
    [SerializeField] private Transform cameraCenter;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform playerBody;
    private Rigidbody rb;
    private Vector3 newBodyTarget;

    [Header("movement")]
    [SerializeField] float speed = 3;
    [SerializeField] float airMultiplier = 1;
    [SerializeField] float bodyRotateSpeed = 60;
    [SerializeField] float maxSlopeAngle = 45;
    [SerializeField] float groundDrag = 10f;
    [SerializeField] LayerMask whatIsGround;

    [Header("Camera Controls")]
    //camera Input
    [Tooltip("a mutiples the camera Input")]
    [Range(0.5f,5)]
    [SerializeField] private float cameraSensitivity = 1;
    [Tooltip("Speed of rotation")]
    //[Range(1000,10000)]
    [SerializeField] private float cameraAcceleration = 1000;
    [SerializeField] private float cameraInputLagPeriod = 0.01f; //how long to check update around lag
    [SerializeField] private Vector2 cameraMaxVerticalAngleFromHorizon;
    [SerializeField] private float cameraOffset = 0;
    [SerializeField] public Vector2 maxCameraDistance = new Vector2(-3, -0.5f);
    [SerializeField] private float cameraMovementSpeed = 1;

    private float cameraDistance;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        newBodyTarget = cameraCenter.forward;
        cameraDistance = maxCameraDistance.x;
    }
    void Update()
    {
        GroundCheck();
        CameraMovementCallication();
        moveVelocity();
    }

    void FixedUpdate()
    {
        addMovementForce();
    }

    #region Movement
    Vector3 moveDirection;
    float lastPlayerHeight;
    RaycastHit slopeHit;
    bool grounded;
    void moveVelocity()
    {
        rb.useGravity = !OnSlope();

        Vector2 inputVariables = moveInputs.ReadValue<Vector2>();

        //get player body to look in direction of movement
        if (playerBody != null && (rb.velocity.x != 0 || rb.velocity.y != 0))
        {
            //playerBody.LookAt(cameraCenter.forward);
            newBodyTarget = cameraCenter.forward;
            newBodyTarget.y = playerBody.forward.y;
        }

        Vector3 newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
        playerBody.rotation = Quaternion.LookRotation(newBodyRotation);

        moveDirection = cameraCenter.forward * inputVariables.y + cameraCenter.right * inputVariables.x/2;
        moveDirection.y = 0;
        /*if (grounded)
        {
            moveDirection.y = transform.forward.y;
        }*/

        //rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
    }

    void addMovementForce()
    {

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * speed * 10f, ForceMode.Force);

            if(rb.velocity.y > 0)
            {
                if (transform.position.y < lastPlayerHeight)
                {
                    rb.AddForce(Vector3.down * 60f, ForceMode.Force);
                }
                else
                {
                    rb.AddForce(Vector3.down * 20f, ForceMode.Force);
                }
            }
            //rb.velocity = rb.velocity.normalized * speed;
        }
        else
        {
            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * speed * airMultiplier * 10f, ForceMode.Force);
            }

            
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }



        Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2 * 0.5f + 2.0f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, 2 * 0.5f + 0.2f, whatIsGround);

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0.2f;
        }
    }


    #endregion



    #region CameraControles
    
    //camera private collection
    private Vector2 cameraLastInputEvent; //a container holding the last input before lag occured
    private float cameraInputLagClock; //timer to help deal with lag and allow smooth camera motion
    private Vector2 cameraVelocity;
    private Vector2 cameraRotation; //house the camera rotation acrosss functions
    private bool cameracolliding;
    float newDistance;




    void CameraMovementCallication()
    {
        Vector2 cameraSpeed = GetMouseInput() * new Vector2(cameraSensitivity, cameraSensitivity);

        // Calculate new rotation and store it for future changes
        cameraVelocity = new Vector2(
            Mathf.MoveTowards(cameraVelocity.x, cameraSpeed.x, cameraAcceleration * Time.deltaTime),
            Mathf.MoveTowards(cameraVelocity.x, cameraSpeed.x, cameraAcceleration * Time.deltaTime));
        
        cameraRotation += cameraVelocity;// * Time.deltaTime;

        cameraRotation.y = ClampCameraVerticalAngle(cameraRotation.y);

        //transform.localEulerAngles = new Vector3(0, cameraRotation.x, 0);
        //cameraCenter.localEulerAngles = new Vector3(0, cameraRotation.x, 0);

        cameraCenter.localEulerAngles = new Vector3(cameraRotation.y, cameraRotation.x, 0);
        
    

        //mainCamera.RotateAround(cameraCenter.position, Vector3.up, cameraVelocity.x);

        //need to add a dead zone for up and down maybe?
        
        
        //take mouseinput inverts y and make sure it comes through without lag
        Vector2 GetMouseInput()
        {
            cameraInputLagClock += Time.deltaTime;

            //collect mouse input
            Vector2 mouseInput = lookInput.ReadValue<Vector2>();

            mouseInput.y = -mouseInput.y/2; //invert y

            cameraRotation += cameraVelocity * Time.deltaTime;

            if ((Mathf.Approximately(0, mouseInput.x) && Mathf.Approximately(0, mouseInput.y)) == false || cameraInputLagClock >= cameraInputLagPeriod)
            {
                cameraLastInputEvent = mouseInput;
                cameraInputLagClock = 0;
            }

            return cameraLastInputEvent;
        }

        float ClampCameraVerticalAngle(float angle)
        {
            return Mathf.Clamp(angle, -cameraMaxVerticalAngleFromHorizon.x, cameraMaxVerticalAngleFromHorizon.y);
        }

        /*void ChangeDistance()
        {
            cameraRange.z = cameraDistance;
        }*/
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


            Vector3 cameraRange = mainCamera.localPosition;
            cameraRange.z = cameraDistance;
            mainCamera.localPosition = cameraRange;
        }
            //cameracolliding = true;
    #endregion

    
}
