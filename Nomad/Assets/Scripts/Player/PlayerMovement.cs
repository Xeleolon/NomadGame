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

    enum MovementType {walking, freefalling, slope, climbing, swinging}
    [Header("movement")]
    MovementType curMovmenent = MovementType.walking;
    [SerializeField] bool freazeMovement;
    [SerializeField] float speed = 3;
    [SerializeField] float airMultiplier = 1;
    [SerializeField] float bodyRotateSpeed = 60;
    [SerializeField] float maxSlopeAngle = 45;
    [SerializeField] float groundDrag = 10f;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private float jumpCooldown = 1;
    private bool readyToJump;
    [SerializeField] LayerMask whatIsGround;

    [Header("climbing and swinging")]
    [SerializeField] private float climbSpeed = 3;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        newBodyTarget = cameraCenter.forward;
        ResetJump();
    }
    void Update()
    {
        CameraMovementCallication();
        if (!freazeMovement)
        {
            moveVelocity();
            JumpFunction();
        }
        else
        {
            rb.useGravity = false;
        }
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
        CheckMovmenentState(false);

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
    }

    void addMovementForce()
    {

        switch (curMovmenent)
        {
            case MovementType.walking: //Walking

            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
            NormalizeAllMovement();
            break;

            case MovementType.slope: //Slope

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

            Vector3 GetSlopeMoveDirection()
            {
                return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            }

            break;

            case MovementType.freefalling: //FreeFalling

            rb.AddForce(moveDirection.normalized * speed * airMultiplier * 10f, ForceMode.Force);
            break;

            case MovementType.climbing: // climbing
            Debug.Log("Set to climbing");
            break;


            case MovementType.swinging: // swining
            Debug.Log("Set to swinging");
            break;
        }


        void NormalizeAllMovement()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    
            if (flatVel.magnitude > speed)
            {
                Vector3 limitedVel = flatVel.normalized * speed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    void CheckMovmenentState(bool forceOut)
    {
        if (!forceOut && (curMovmenent == MovementType.swinging || curMovmenent == MovementType.climbing))
        {
            return;
        }

        if (OnSlope())
        {
            if (curMovmenent != MovementType.slope)
            {
                rb.useGravity = false;
                curMovmenent =  MovementType.slope;
            }
            return;
        }

        rb.useGravity = true;

        if (GroundCheck())
        {
            curMovmenent = MovementType.freefalling;
            return;
        }
        
        curMovmenent = MovementType.walking;
        return;
    }

    #endregion

    #region Slope And Jump Movement;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2 * 0.5f + 2.0f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    private bool GroundCheck()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, 2 * 0.5f + 0.2f, whatIsGround);

        if (grounded)
        {
            rb.drag = groundDrag;
            return true;
        }

        rb.drag = 0.2f;
        return false;
        
    }

    private void JumpFunction()
    {
        if (jumpInput.ReadValue<float>() > 0)
        {
            //Debug.Log("attemp Jump " + readyToJump + grounded);
            if (readyToJump && grounded)
            {
                //Debug.Log("Jump");
                //exitingSlope = true;
                readyToJump = false;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    
                Invoke(nameof(ResetJump), jumpCooldown);
            }

        }

    }

    private void ResetJump()
    {
        readyToJump = true;
        //exitingSlope = false;
    }
    #endregion 
    #region Ladder Movement
    public void ChangeMovement(MovementType newMovement, Vector3 hopPosition)
    {

        switch (newMovement)
        {
            case MovementType.climbing:

            break;

            case MovementType.swinging

            break;
        }
        curMovmenent = newMovement;
    }
    private void Climbing()
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
    }
    #endregion



    #region CameraControles
    
    //camera private collection
    private Vector2 cameraLastInputEvent; //a container holding the last input before lag occured
    private float cameraInputLagClock; //timer to help deal with lag and allow smooth camera motion
    private Vector2 cameraVelocity;
    private Vector2 cameraRotation; //house the camera rotation acrosss functions




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

    
            //cameracolliding = true;
    #endregion

    
}
