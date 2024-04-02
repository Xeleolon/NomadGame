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

    private InputAction moveInput;
    private InputAction jumpInput;
    private InputAction lookInput;
    
    void OnEnable()
    {
        moveInput = playerControls.Player.Move;
        moveInput.Enable();

        jumpInput = playerControls.Player.Jump;
        jumpInput.Enable();

        lookInput = playerControls.Player.Look;
        lookInput.Enable();
    }

    void OnDisable()
    {
        moveInput.Disable();
        jumpInput.Disable();
    }

    #endregion


    //camera Input
    [Tooltip("a mutiples the camera Input")]
    //[Range(1,30)]
    [SerializeField] private float cameraSensitivity = 1;
    [Tooltip("Speed of rotation")]
    //[Range(1000,10000)]
    [SerializeField] private float cameraAcceleration = 1000;
    [SerializeField] private float cameraInputLagPeriod = 0.01f; //how long to check update around lag
    [SerializeField] private float cameraMaxVerticalAngleFromHorizon = 90;
    [SerializeField] private Transform cameraTransform;
    void Update()
    {
        CameraMovementCallication();
    }

    void FixedUpdate()
    {

    }
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

        cameraTransform.localEulerAngles = new Vector3(cameraRotation.y, cameraRotation.x, 0);
        
        
        //take mouseinput inverts y and make sure it comes through without lag
        Vector2 GetMouseInput()
        {
            cameraInputLagClock += Time.deltaTime;

            //collect mouse input
            Vector2 mouseInput = lookInput.ReadValue<Vector2>();

            mouseInput.y = -mouseInput.y; //invert y

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
            return Mathf.Clamp(angle, -cameraMaxVerticalAngleFromHorizon, cameraMaxVerticalAngleFromHorizon);
        }
    }
    #endregion

    
}
