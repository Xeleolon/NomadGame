 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public enum MovementType {walking, freefalling, slope, climbing, swinging}
    [SerializeField] MovementType curMovmenent = MovementType.walking;
    [Header("Movement")]
    [SerializeField] bool freazeMovement;
    [SerializeField] float speed = 3;
    [SerializeField] float airMultiplier = 1;
    [SerializeField] float airDevider = 0.5f;
    [SerializeField] float bodyRotateSpeed = 60;
    [SerializeField] float maxSlopeAngle = 45;

    //air controls
    [SerializeField] float groundDrag = 10f;
    [SerializeField] float airDrag = 0.2f;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private float jumpCooldown = 1;

    [SerializeField] private float airGlideLength = 1;
    [SerializeField] private float gravityGlide = 2;
    private float glideClock;
    private bool readyToJump;
    [SerializeField] LayerMask whatIsGround;


    [Header("climbing and swinging")]
    [SerializeField] private float climbSpeed = 3;
    [SerializeField] private float forceToWall = 3;
    [SerializeField] private float distanceCheck = 1;
    [SerializeField] private float maxClimbEnterCloc = 1;
    [SerializeField] private Vector2 forwardForce = new Vector2(0.2f, 0.8f);

    //Detection

    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private LayerMask whatIsWall;

    private RaycastHit frontWallHit;

    //Swinging var
    //[SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] float swingMultiplier = 1;
    [SerializeField] private float extendRopeSpeed = 3;
    [SerializeField] Transform ropeTool;
    private Vector3 swingPoint;
    private SpringJoint joint;
    //Swinging refs

    [SerializeField] LineRenderer lr;

    [Header("UI insturctions")]
    [SerializeField] string exitInstruction;
    [SerializeField] string exitPressKey;

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

        public Slider mouseSlider;
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

    private Slider mouseSlider {get {return cameraControls.mouseSlider;} set {cameraControls.mouseSlider = value;}}

    
    public References references;

    #endregion
    #region Start & Update Calls
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        newBodyTarget = cameraCenter.forward;
        ResetJump();

        mouseSlider.value = cameraSensitivity;
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
    public void FreazePlayer()
    {
        freazeMovement = !freazeMovement;

        if (freazeMovement)
        {
            rb.velocity = Vector3.zero;
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

    Vector3 climbingExitingVar;
    bool climbingUpBool;
    float climbVariable;
    bool enteringClimb;
    Transform climbingWall;
    private float climbEnterClock;

    void moveVelocity()
    {
        CheckMovmenentState(false);

        Vector2 inputVariables = moveInputs.ReadValue<Vector2>();
        Vector3 newBodyRotation;
        float ropeVariables = climbRopeInput.ReadValue<float>();


        switch (curMovmenent)
        {
            case MovementType.climbing:
                newBodyTarget = climbingWall.forward;
                newBodyTarget.y = playerBody.forward.y;
                newBodyRotation = Vector3.RotateTowards(playerBody.forward, newBodyTarget, bodyRotateSpeed * Time.deltaTime, 0);
                playerBody.rotation = Quaternion.LookRotation(newBodyRotation);

                //dectect wall to determine if exiting wall

                if (inputVariables.y < 0 && GroundCheck())
                {
                    ExitingClimbing();
                    //Debug.Log("testing ground exit");

                }

                float forwardInput = forwardForce.x;

                if (!enteringClimb)
                {

                    if (inputVariables.y > 0 && !climbingRayCast())
                    {
                        //Debug.Log("exiting up out of Climbing");
                        if (!climbingUpBool)
                        {
                            climbingExitingVar = transform.position;
                            climbingUpBool = true;
                        }

                        //forward variable
                        forwardInput = forwardForce.y;

                        if (Vector3.Distance(transform.position, climbingExitingVar) >= distanceCheck)
                        {
                            ExitingClimbing();
                            climbingUpBool = false;
                        }
                    }
                    else if (!climbingRayCast())
                    {
                        if (!climbingUpBool)
                        {
                            climbingExitingVar = transform.position;
                            climbingUpBool = true;
                        }


                        if (Vector3.Distance(transform.position, climbingExitingVar) >= distanceCheck)
                        {
                            ExitingClimbing();
                            climbingUpBool = false;
                        }
                    }
                    else if (climbingRayCast() && climbingUpBool)
                    {
                        climbingUpBool = false;
                    } 
                }
                else if (enteringClimb)
                {
                    if (climbingRayCast() || climbEnterClock > maxClimbEnterCloc)
                    {
                        enteringClimb = false;
                        climbEnterClock = 0;
                    }
                    else
                    {
                        climbEnterClock += 1 * Time.deltaTime;
                    }
                }


                climbVariable = inputVariables.y * climbSpeed * Time.deltaTime;
                moveDirection = /*playerBody.up * inputVariables.y +*/ climbingWall.right * inputVariables.x/2 + climbingWall.forward * forwardInput * Time.deltaTime;
                //Debug.Log("Climbing variables " + moveDirection);

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
                if (glideClock < airGlideLength)
                {
                    rb.AddForce(moveDirection.normalized * speed * airMultiplier * 10f, ForceMode.Force);
                    glideClock += 1 * Time.deltaTime;
                }
                else if (rb.mass != gravityGlide)
                {
                    rb.AddForce(moveDirection.normalized * speed * airDevider * 10f, ForceMode.Force);
                    rb.mass = gravityGlide;
                }

                //rb.AddForce(moveDirection.normalized * speed * airMultiplier * 10f, ForceMode.Force);
                NormalizeAllMovement();
            break;

            case MovementType.climbing: // climbing
                //Debug.Log("Set to climbing");
                //Debug.Log("Climbing variables " + moveDirection);
                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);

                Vector3 climbingUp = transform.position;
                climbingUp.y += climbVariable;
                transform.position = climbingUp;
                //NormalizeAllMovement(climbSpeed);
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

        if (!GroundCheck())
        {
            EnteringFreeFalling();
            return;
        }
        
        curMovmenent = MovementType.walking;
        return;
    }

    void EnteringFreeFalling()
    {
        if (curMovmenent == MovementType.freefalling)
        {
            return;
        }
        curMovmenent = MovementType.freefalling;
        glideClock = 0;

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
        RaycastHit groundHit;
        bool grounded; 
        bool testingGround = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out groundHit, 2 * 0.5f + 0.2f, whatIsGround);

        if (testingGround)
        {
            Vector3 raycastHitPosition = groundHit.point;
            float heightDistance = transform.position.y - raycastHitPosition.y;
            if (heightDistance > 0.8f && heightDistance < 1.2f)
            {
                grounded = true;
            }
            else
            {
                grounded = false;
            }
        }
        else
        {
            grounded = false;
        }

        if (grounded)
        {
            rb.drag = groundDrag;
            rb.mass = 1;
            return true;
        }
 
        rb.drag = airDrag;
        if (curMovmenent == MovementType.climbing)
        {
            rb.drag = groundDrag;
        }
        return false;
        
    }

    private void JumpFunction()
    {
        if (jumpInput.ReadValue<float>() > 0)
        {
            
            //Debug.Log("attemp Jump " + readyToJump + GroundCheck());
            if (readyToJump && (curMovmenent == MovementType.swinging || curMovmenent == MovementType.climbing || GroundCheck()))
            {
                //Debug.Log("Jump");
                //exitingSlope = true;
                if (curMovmenent == MovementType.swinging)
                {
                    StopSwing();
                }
                else if (curMovmenent == MovementType.climbing)
                {
                    ExitingClimbing();
                }
                else
                {
                    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                }
                readyToJump = false;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    
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

    public bool CurMovmenentMatch(MovementType newMovement)
    {
        if (curMovmenent == newMovement)
        {
            return true;
        }
        
        return false;
    }
    public void ChangeMovement(MovementType newMovement, Vector3 hopPosition, Transform wall)
    {

        switch (newMovement)
        {
            case MovementType.climbing:
                //if (playerBody != null)
                //{
                PlayerLife.instance.ActiveInstructionPanel(true, exitInstruction, exitPressKey);
                rb.mass = 1;
                newBodyTarget = cameraCenter.TransformDirection(hopPosition);
                    if (joint != null)
                    {
                        Destroy(joint);
                    }
                    
                    climbingWall = wall;
                    lastInputs = Vector2.zero;
                    cameraSets = CameraSets.detach;
                    Debug.Log("newBodyTarget");
                    enteringClimb = true;
                    climbEnterClock = 0;


                rb.AddForce(playerBody.forward * forceToWall, ForceMode.Impulse);
                //}
            break;

            case MovementType.swinging:

            break;
        }
        curMovmenent = newMovement;
    }

    private bool climbingRayCast()
    {
        /*if (Physics.Raycast(transform.position, playerBody.forward, out climbHit, 1 * 0.5f + 2.0f))
        {
            if (climbHit.collider.gameObject.GetComponent<Climb>() != null)
            {
                return true;
            }
        }
        return false;*/

        //Debug.Log("testing raycast Climbing");

        if (Physics.SphereCast(transform.position, sphereCastRadius, playerBody.forward, out frontWallHit, detectionLength, whatIsWall))
        {
            //Debug.Log("rayCast Decting Wall");
            return true;
        }
        //Debug.Log("rayCast not decting Wall");
        return false;


    }

    private void ExitingClimbing()
    {
        if (GroundCheck())
        {
            climbEnterClock = 0;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            curMovmenent = MovementType.walking;
            cameraSets = CameraSets.standard;
            return;
        }
        PlayerLife.instance.ActiveInstructionPanel(false, exitInstruction, exitPressKey);

        EnteringFreeFalling();
        cameraSets = CameraSets.forceFollow;
    }
    #endregion
    #region Swinging
    SwingAnchor swingScript;

    public void StartSwing(Vector3 swingAnchor, GameObject anchorObject)
    {
        PlayerLife.instance.ActiveInstructionPanel(true, exitInstruction, exitPressKey);
        curMovmenent = MovementType.swinging;
        rb.mass = 1;
        swingPoint = swingAnchor;
        //Debug.Log(swingPoint);

        if (joint != null)
        {
            Destroy(joint);
            
        }

        if (swingScript != null)
        {
            swingScript.HideRope(false);
            swingScript = null;
        }

        swingScript = anchorObject.GetComponent<SwingAnchor>();

        if (swingScript != null)
        {
            swingScript.HideRope(true);
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
    public void KillJoint()
    {
        if (joint != null)
        {
            Destroy(joint);
        }
    }

    void StopSwing()
    {
        PlayerLife.instance.ActiveInstructionPanel(false, exitInstruction, exitPressKey);
        if (swingScript != null)
        {
            swingScript.HideRope(false);
            swingScript = null;
        }
        EnteringFreeFalling();
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

    public void SetMouseSensitivity()
    {
        cameraSensitivity = mouseSlider.value;
    }

    
            //cameracolliding = true;
    #endregion

    
}
