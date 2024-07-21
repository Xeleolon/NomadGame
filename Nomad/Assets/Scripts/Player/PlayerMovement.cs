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
        public Animator mainAnimator;
    }
    
    private Transform cameraCenter {get {return references.cameraCenter;} set {references.cameraCenter = value;}}
    private Transform mainCamera {get {return references.mainCamera;} set {references.mainCamera = value;}}
    private Transform playerBody {get {return references.playerBody;} set {references.playerBody = value;}}
    private Animator mainAnimator {get {return references.mainAnimator;} set {references.mainAnimator = value;}}
    private Rigidbody rb;
    private Vector3 newBodyTarget;

    public enum MovementType {walking, freefalling, slope, climbing, climbingMovement, swinging}
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
    [SerializeField] private Vector2 climbSpeed = new Vector2(1, 3);
    [SerializeField] private float holdClimbSpeed = 0.2f;
    [SerializeField] private Vector2 holdClimbLength = new Vector2(2, 4);
    //Swinging var
    //[SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] float swingMultiplier = 1;
    [SerializeField] private float extendRopeSpeed = 3;
    [SerializeField] Transform ropeTool;
    private Vector3 swingPoint;
    private SpringJoint joint;
    //Swinging refs

    [SerializeField] LineRenderer lr;

    public enum CameraSets {standard, forceFollow, onlyHorizontal, reset, detach}

    [System.Serializable]
    public class CameraControls
    {
        public CameraSets cameraSets = CameraSets.standard;
        [Tooltip("a mutiples the camera Input")] [Range(0.5f,5)]
        public float cameraSensitivity = 1.5f;
        [Tooltip("Speed of rotation")]
        public float cameraAcceleration = 100;
        [Tooltip("how long to check update around lag")]
        public float cameraInputLagPeriod = 0.01f;
        public bool forceCameraConection;
        public Vector2 cameraMaxVerticalAngleFromHorizon = new Vector2(-10, 10);
        public float standardCameraClamp = 10;
        public float cameraOffset = 10;
        public GameObject blackScreen;
        public string blackScreenAnimationName;
    }

    public CameraControls cameraControls;
    //camera Input
    private float cameraSensitivity {get {return cameraControls.cameraSensitivity;} set {cameraControls.cameraSensitivity = value;}}
    
    private float cameraAcceleration {get {return cameraControls.cameraAcceleration;} set {cameraControls.cameraAcceleration = value;}}
    private float cameraInputLagPeriod {get {return cameraControls.cameraInputLagPeriod;} set {cameraControls.cameraInputLagPeriod = value;}}
    private Vector2 cameraMaxVerticalAngleFromHorizon {get {return cameraControls.cameraMaxVerticalAngleFromHorizon;} set {cameraControls.cameraMaxVerticalAngleFromHorizon = value;}}
    private bool forceCameraConection {get {return cameraControls.forceCameraConection;} set {cameraControls.forceCameraConection = value;}}

    private float cameraOffset {get {return cameraControls.cameraOffset;} set {cameraControls.cameraOffset = value;}}
    private CameraSets cameraSets {get {return cameraControls.cameraSets;} set {cameraControls.cameraSets = value;}}
    private float standardCameraClamp {get {return cameraControls.standardCameraClamp;} set {cameraControls.standardCameraClamp = value;}}
    private GameObject blackScreen {get {return cameraControls.blackScreen;} set {cameraControls.blackScreen = value;}}
    private string blackScreenAnimationName {get {return cameraControls.blackScreenAnimationName;} set {cameraControls.blackScreenAnimationName = value;}}

    
    public References references;

    #endregion
    #region Start & Update Calls
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
            //CameraUpdate();
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
    #endregion

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
                ClimbingMovement(inputVariables);

            break;
            
            case MovementType.climbingMovement:
                 Climbing();
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

    #region Climbing Movement
    public class ClimbAnchorData
    {
        public Transform transform;
        public string animationName;
    }
    //left
    ClimbAnchorData leftClimbAnchor1;
    ClimbAnchorData leftClimbAnchor2;
    ClimbAnchorData leftClimbAnchor3;
    //right
    ClimbAnchorData rightClimbAnchor1;
    ClimbAnchorData rightClimbAnchor2;
    ClimbAnchorData rightClimbAnchor3;
    //up
    ClimbAnchorData upClimbAnchor1;
    ClimbAnchorData upClimbAnchor2;
    ClimbAnchorData upClimbAnchor3;
    //down
    ClimbAnchorData downClimbAnchor1;
    ClimbAnchorData downClimbAnchor2;
    ClimbAnchorData downClimbAnchor3;
    //left Up
    ClimbAnchorData leftUpClimbAnchor1;
    ClimbAnchorData leftUpClimbAnchor2;
    ClimbAnchorData leftUpClimbAnchor3; //1.2
    ClimbAnchorData leftUpClimbAnchor4; //1.8
    //right Up
    ClimbAnchorData rightUpClimbAnchor1;
    ClimbAnchorData rightUpClimbAnchor2;
    ClimbAnchorData rightUpClimbAnchor3; //1.2
    ClimbAnchorData rightUpClimbAnchor4; //1.8
    //left Down
    ClimbAnchorData leftDownClimbAnchor1;
    ClimbAnchorData leftDownClimbAnchor2;
    ClimbAnchorData leftDownClimbAnchor3; //1.2
    ClimbAnchorData leftDownClimbAnchor4; //1.8
    //right Down
    ClimbAnchorData rightDownClimbAnchor1;
    ClimbAnchorData rightDownClimbAnchor2;
    ClimbAnchorData rightDownClimbAnchor3; //1.2
    ClimbAnchorData rightDownClimbAnchor4; //1.8

    private Vector3 lastClimbPoint;
    private Vector3 climbPoint;
    private float currentClimbSpeed;

    Vector2 climbHoldVaribles;
    void ClimbingMovement(Vector2 inputVaribles)
    {

        if (inputVaribles.x > 0)
        {
            if (climbHoldVaribles.x < 0)
            {
                climbHoldVaribles.x = 0;
            }
            else
            {
                climbHoldVaribles.x += inputVaribles.x * holdClimbSpeed; 
            }
        }
        else if (inputVaribles.x < 0)
        {
            if (climbHoldVaribles.x > 0)
            {
                climbHoldVaribles.x = 0;
            }
            else
            {
                climbHoldVaribles.x += inputVaribles.x * holdClimbSpeed; 
            }
        }

        if (inputVaribles.y > 0)
        {
            if (climbHoldVaribles.y < 0)
            {
                climbHoldVaribles.y = 0;
            }
            else
            {
                climbHoldVaribles.y += inputVaribles.y * holdClimbSpeed; 
            }
        }
        else if (inputVaribles.y < 0)
        {
            if (climbHoldVaribles.y > 0)
            {
                climbHoldVaribles.y = 0;
            }
            else
            {
                climbHoldVaribles.y += inputVaribles.y * holdClimbSpeed; 
            }
        }


        if (climbHoldVaribles.x > 0 && climbHoldVaribles.y == 0) //right
        {
            StraightClimb(climbHoldVaribles.x, rightClimbAnchor1, rightClimbAnchor2, rightClimbAnchor3, rightUpClimbAnchor4, rightDownClimbAnchor3);
        }
        else if (climbHoldVaribles.x > 0 && climbHoldVaribles.y == 0) //Left
        {
            StraightClimb(-climbHoldVaribles.x, leftClimbAnchor1, leftClimbAnchor2, leftClimbAnchor3, leftDownClimbAnchor4, leftUpClimbAnchor3);
        }
        else if (climbHoldVaribles.x == 0 && climbHoldVaribles.y > 0) //up
        {
            StraightClimb(climbHoldVaribles.y, upClimbAnchor1, upClimbAnchor2, upClimbAnchor3, leftUpClimbAnchor4, rightUpClimbAnchor3);
        }
        else if (climbHoldVaribles.x == 0 && climbHoldVaribles.y < 0) //down
        {
            StraightClimb(-climbHoldVaribles.y, downClimbAnchor1, downClimbAnchor2, downClimbAnchor3, rightDownClimbAnchor4, leftDownClimbAnchor3);
        }
        else if (climbHoldVaribles.x > 0 && climbHoldVaribles.y > 0) //up right
        {
            CornerClimb(rightUpClimbAnchor1, rightUpClimbAnchor2, rightUpClimbAnchor3, rightUpClimbAnchor4);
        }
        else if (climbHoldVaribles.x < 0 && climbHoldVaribles.y > 0) //up left
        {
            CornerClimb(leftUpClimbAnchor1, leftUpClimbAnchor2, leftUpClimbAnchor3, leftUpClimbAnchor4);
        }
        else if (climbHoldVaribles.x > 0 && climbHoldVaribles.y < 0) //down right
        {
            CornerClimb(rightDownClimbAnchor1, rightDownClimbAnchor2, rightDownClimbAnchor3, rightDownClimbAnchor4);
        }
        else if (climbHoldVaribles.x < 0 && climbHoldVaribles.y < 0) //down left
        {
            CornerClimb(leftDownClimbAnchor1, leftDownClimbAnchor2, leftDownClimbAnchor3, leftDownClimbAnchor4);
        }
        

    }

    private void StraightClimb(float climbHolding, ClimbAnchorData anchor1, ClimbAnchorData anchor2, ClimbAnchorData anchor3, ClimbAnchorData anchorDown, ClimbAnchorData anchorUp)
    {
        if (anchor1.transform != null && anchor2.transform != null) // full speed move towards direction with no hold
        {
            if (climbHolding > holdClimbLength.x)
            {
                //then activate climb
            }
            return;
        }
        else //move in direction holding button before moving
        {
            if (anchor3.transform != null) //move here
            {
                if (climbHolding > holdClimbLength.y)
                {
                    //then activate climb
                }
                return;
            }
            else if (anchor2.transform != null) //if no 3 here first
            {
                if (climbHolding > holdClimbLength.y)
                {
                    //then activate climb
                }
                return;
            }
            else if (anchorDown != null || anchorUp != null) //if no two then try one of these at random
            {
                if (climbHolding > holdClimbLength.y)
                {
                    //then activate climb
                }
                return;
            }
            else if (anchor1.transform != null) //if none of the other try this one last
            {
                if (climbHolding > holdClimbLength.y)
                {
                    //then activate climb
                }
                return;
            }
        }
    }

    private void CornerClimb(ClimbAnchorData anchor1, ClimbAnchorData anchor2, ClimbAnchorData anchorDown, ClimbAnchorData anchorUp)
    {

    }

    private void Climbing()
    {
        if (transform.position != climbPoint)
        {
            transform.position = Vector3.MoveTowards(climbPoint, transform.position, currentClimbSpeed * Time.deltaTime);
        }
        else
        {
            curMovmenent = MovementType.climbing;
        }
    }



    //old climbing
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
                    cameraSets = CameraSets.detach;
                    Debug.Log("newBodyTarget");
                //}
            break;
        }
        curMovmenent = newMovement;
    }
    #endregion
    #region Swinging

    public void StartSwing(Vector3 swingAnchor)
    {
        curMovmenent = MovementType.swinging;
        
        swingPoint = swingAnchor;
        //Debug.Log(swingPoint);

        if (joint != null)
        {
            Destroy(joint);
        }

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
    private Camera secondCamera;
    private bool turnCameraOn;
    private bool changeCamera;
    public void ChangeCamera()
    {
        changeCamera = true;
    }

    public void AddCamera(Camera newCamera)
    {
        if (newCamera != secondCamera)
        {
            if (!blackScreen.activeSelf)
            {
                blackScreen.SetActive(true);
            }
            turnCameraOn = true;

            secondCamera = newCamera;

            blackScreen.SetActive(true);
            
            mainAnimator.Play(blackScreenAnimationName);
            

        }
    }

    public void RemoveCamera(Camera newCamera)
    {
        if (newCamera == secondCamera)
        {
            if (!blackScreen.activeSelf)
            {
                blackScreen.SetActive(true);
            }
            turnCameraOn = false;

            
            
            mainAnimator.Play(blackScreenAnimationName);
            

        }
    }

    public void ChangeCameraType(CameraSets cameraType)
    {
        if (cameraType == CameraSets.reset)
        {
            cameraType = CameraSets.standard;
        }
        cameraSets = cameraType;
    }




    void CameraMovementCallication()
    {
        if (changeCamera)
        {
            if (turnCameraOn && secondCamera != null)
            {
                secondCamera.enabled = true;
                cameraSets = CameraSets.onlyHorizontal;
            }
            else if (!turnCameraOn && secondCamera != null)
            {
                secondCamera.enabled = false;
                secondCamera = null;
                cameraSets = CameraSets.forceFollow;
            }
            changeCamera = false;
        }

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

            if (cameraSets != CameraSets.detach)
            {//playerBodyRotation
            Vector3 newBodyRotation;

            if (playerBody != null && cameraSets != CameraSets.standard || (rb.velocity.x != 0 || rb.velocity.y != 0))
                {
                    //playerBody.LookAt(cameraCenter.forward);
                    newBodyTarget = cameraCenter.forward;
                }

                //Debug.Log(playerBody.forward.y + " " + playerBody.position.y);

                newBodyTarget.y = 0;


                newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
                playerBody.rotation = Quaternion.LookRotation(newBodyRotation);
            }
    

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
            if (cameraSets == CameraSets.onlyHorizontal)
            {
                return Mathf.Clamp(angle, standardCameraClamp, standardCameraClamp);
            }

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
