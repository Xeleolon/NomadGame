using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Inputs/Awake,OnEnable,OnDisable
    private PlayerInputActions playerControls;
    public static PlayerMovement instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("more than one player life!");
        }
        instance = this;
        playerControls = new PlayerInputActions(); 
    }

    private InputAction moveInputs;
    private InputAction jumpInput;
    private InputAction lookInput;
    private InputAction climbRopeInput;
    
    void OnEnable()
    {
        moveInputs = playerControls.Player.Move;
        moveInputs.Enable();

        jumpInput = playerControls.Player.Jump;
        jumpInput.Enable();

        lookInput = playerControls.Player.Look;
        lookInput.Enable();

        climbRopeInput = playerControls.Player.ClimbRope;
        climbRopeInput.Enable();
    }

    void OnDisable()
    {
        moveInputs.Disable();
        jumpInput.Disable();
        climbRopeInput.Disable();

    }

    #endregion

    //refernces
    #region Variables
    [System.Serializable]
    public class References
    {
        public Transform cameraCenter;
        public Transform mainCamera;
        public Transform playerBody;
    }
    
    private Transform cameraCenter {get {return references.cameraCenter;} set {references.cameraCenter = value;}}
    private Transform mainCamera {get {return references.mainCamera;} set {references.mainCamera = value;}}
    private Transform playerBody {get {return references.playerBody;} set {references.playerBody = value;}}
    private Rigidbody rb;
    private Vector3 newBodyTarget;

    public enum MovementType {walking, freefalling, slope, climbing, swinging}
    [SerializeField] MovementType curMovmenent = MovementType.walking;
    [Header("Movement")]
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
    //Swinging var
    //[SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] float swingMultiplier = 1;
    [SerializeField] private float extendRopeSpeed = 3;
    [SerializeField] Transform ropeTool;
    private Vector3 swingPoint;
    private SpringJoint joint;
    //Swinging refs

    [SerializeField] LineRenderer lr;

    [System.Serializable]
    public class CameraControls
    {
        [Tooltip("a mutiples the camera Input")] [Range(0.5f,5)]
        public float cameraSensitivity = 1.5f;
        [Tooltip("Speed of rotation")]
        public float cameraAcceleration = 100;
        [Tooltip("how long to check update around lag")]
        public float cameraInputLagPeriod = 0.01f;
        public bool forceCameraConection;
        public Vector2 cameraMaxVerticalAngleFromHorizon = new Vector2(-10, 10);
        public float cameraOffset = 10;
    }

    public CameraControls cameraControls;
    //camera Input
    private float cameraSensitivity {get {return cameraControls.cameraSensitivity;} set {cameraControls.cameraSensitivity = value;}}
    
    private float cameraAcceleration {get {return cameraControls.cameraAcceleration;} set {cameraControls.cameraAcceleration = value;}}
    private float cameraInputLagPeriod {get {return cameraControls.cameraInputLagPeriod;} set {cameraControls.cameraInputLagPeriod = value;}}
    private Vector2 cameraMaxVerticalAngleFromHorizon {get {return cameraControls.cameraMaxVerticalAngleFromHorizon;} set {cameraControls.cameraMaxVerticalAngleFromHorizon = value;}}
    private bool forceCameraConection {get {return cameraControls.forceCameraConection;} set {cameraControls.forceCameraConection = value;}}

    private float cameraOffset {get {return cameraControls.cameraOffset;} set {cameraControls.cameraOffset = value;}}

    public References references;

    #endregion
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

    void LateUpdate()
    {
        DrawRope();
    }

    #region Movement
    Vector3 moveDirection;
    float lastPlayerHeight;
    RaycastHit slopeHit;
    RaycastHit climbHit;
    bool grounded;
    Vector2 lastInputs;
    void moveVelocity()
    {
        CheckMovmenentState(false);

        Vector2 inputVariables = moveInputs.ReadValue<Vector2>();
        Vector3 newBodyRotation;
        float ropeVariables = climbRopeInput.ReadValue<float>();

        switch (curMovmenent)
        {
            case MovementType.climbing:
                bool sideHit = false;
                newBodyTarget.y = playerBody.forward.y;

                newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
                playerBody.rotation = Quaternion.LookRotation(newBodyRotation);

                if (!climbingRayCast())
                {
                    if (lastInputs.x < 0 && inputVariables.x <= 0)
                    {
                        inputVariables.x = 0;
                        sideHit = true;
                    }
                    else if (lastInputs.x > 0 && inputVariables.x >= 0)
                    {
                        inputVariables.x = 0;
                        sideHit = true;
                    }
                    Debug.Log("attemping to freaze variables" + lastInputs.x);
                }
                
                if (!sideHit)
                {
                    lastInputs.x = inputVariables.x;
                }

                lastInputs.y = inputVariables.y;

                moveDirection = cameraCenter.up * inputVariables.y + playerBody.right * inputVariables.x/2;

            break;

            case MovementType.swinging:
                
                //movement must follow camera when swinging
                newBodyTarget = cameraCenter.forward;
                newBodyTarget.y = playerBody.forward.y;


                //movement
                if (ropeVariables == 0)
                {
                    newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
                    playerBody.rotation = Quaternion.LookRotation(newBodyRotation);
            
                    moveDirection = cameraCenter.forward * inputVariables.y + cameraCenter.right * inputVariables.x/2;
                    moveDirection.y = 0;
                }
                else
                {
                    moveDirection = Vector3.zero;
                    float extendDistance = Vector3.Distance(transform.position, swingPoint);
                    if (ropeVariables < 0)
                    {
                        Debug.Log("Extending Cable");
                        extendDistance += extendRopeSpeed * Time.deltaTime;
                    }
                    else
                    {
                        Debug.Log("shorting Cable");
                        extendDistance -= extendRopeSpeed * Time.deltaTime;
                    }

                    joint.maxDistance = extendDistance * 0.8f;
                    joint.minDistance = extendDistance * 0.25f;
                }
            break;

            default:
                //get player body to look in direction of movement
                if (playerBody != null && forceCameraConection || (rb.velocity.x != 0 || rb.velocity.y != 0))
                {
                    //playerBody.LookAt(cameraCenter.forward);
                    newBodyTarget = cameraCenter.forward;
                    newBodyTarget.y = playerBody.forward.y;
                }

                newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
                playerBody.rotation = Quaternion.LookRotation(newBodyRotation);
        
                moveDirection = cameraCenter.forward * inputVariables.y + cameraCenter.right * inputVariables.x/2;
                moveDirection.y = 0;
            break;
        }
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
                NormalizeAllMovement();

            break;

            case MovementType.freefalling: //FreeFalling
                rb.AddForce(moveDirection.normalized * speed * airMultiplier * 10f, ForceMode.Force);
                NormalizeAllMovement();
            break;

            case MovementType.climbing: // climbing
                Debug.Log("Set to climbing");
                rb.AddForce(moveDirection.normalized * climbSpeed * 10f, ForceMode.Force);
                NormalizeAllMovement();
            break;


            case MovementType.swinging: // swining
                //Debug.Log("Set to swinging");
                rb.AddForce(moveDirection.normalized * speed * swingMultiplier * 10f, ForceMode.Force);
                NormalizeAllMovement();
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
        if (!forceOut)
        {
            if (curMovmenent == MovementType.climbing)
            {
                rb.useGravity = false;
                return;
            }
            else if (curMovmenent == MovementType.swinging)
            {
                rb.useGravity = true;
                return;
            }
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
            if (readyToJump && (curMovmenent == MovementType.swinging || grounded))
            {
                //Debug.Log("Jump");
                //exitingSlope = true;
                if (curMovmenent == MovementType.swinging)
                {
                    StopSwing();
                }
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
    public bool CurMovmenentMatch(MovementType newMovement)
    {
        if (curMovmenent == newMovement)
        {
            return true;
        }
        
        return false;
    }
    public void ChangeMovement(MovementType newMovement, Vector3 hopPosition)
    {

        switch (newMovement)
        {
            case MovementType.climbing:
                //if (playerBody != null)
                //{
                    newBodyTarget = cameraCenter.TransformDirection(hopPosition);
                    lastInputs = Vector2.zero;
                    Debug.Log("newBodyTarget");
                //}
            break;

            case MovementType.swinging:

            break;
        }
        curMovmenent = newMovement;
    }

    private bool climbingRayCast()
    {
        if (Physics.Raycast(transform.position, playerBody.forward, out climbHit, 1 * 0.5f + 2.0f))
        {
            if (climbHit.collider.gameObject.GetComponent<Climb>() != null)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Swinging

    public void StartSwing(Vector3 swingAnchor)
    {
        curMovmenent = MovementType.swinging;
        
        swingPoint = swingAnchor;
        Debug.Log(swingPoint);
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;
        joint.anchor = Vector3.zero;

        float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

        // the distance grapple will try to keep from grapple point.
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        //customize values
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        //visualisation

        lr.positionCount = 2;
    }

    void StopSwing()
    {
        curMovmenent = MovementType.freefalling;
        lr.positionCount = 0;
        Destroy(joint);
    }
    private Vector3 currentGrapplePosition;

    void DrawRope()
    {
        //Debug.Log("testing");
        if (curMovmenent != MovementType.swinging)
        {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, ropeTool.position);
        lr.SetPosition(1, swingPoint);
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
            Mathf.MoveTowards(cameraVelocity.y, cameraSpeed.y, cameraAcceleration * Time.deltaTime));
        
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
