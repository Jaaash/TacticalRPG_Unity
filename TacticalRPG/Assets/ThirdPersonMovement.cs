using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ThirdPersonMovement : MonoBehaviour
{

    public float baseSpeed = 10f;
    public float aimingSpeed = 5f;
    float moveSpeed;
    public float gravity = 1f;
    public float jumpForce = 10f;

    public float camSpeedX = 50f;
    public float camSpeedY = 5f;
    public Transform orientation;
    float moveH, moveV, camH, camV;
    public float xRotate, yRotate;
    bool aimButton;

    Vector3 moveDirection;
    Rigidbody body;

    [SerializeField] PlayerControls playerControls;
    [SerializeField] GameObject activeUnit;
    [SerializeField] float camHeight;
    [SerializeField] Transform camPivot;
    [SerializeField] Transform camParent;
    [SerializeField] CinemachineFreeLook freeLookCam;
    [SerializeField] GameObject model;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        playerControls = Camera.main.GetComponent<PlayerControls>();
        freeLookCam = FindFirstObjectByType<CinemachineFreeLook>();
        camPivot = transform.Find("CamPivot");
        camParent = transform.Find("CamPivot/CamParent");
    }

    void Update()
    {
        activeUnit = playerControls.activeUnit;
        if (activeUnit != gameObject) { return; }
        else
        {
            GetInput();
            MoveCamera();
        }
    }

    void FixedUpdate()
    {
        if (activeUnit != gameObject) { return; }
        else
        {
            MovePlayer();
        }
        
    }


    void GetInput()
    {
        moveH = Input.GetAxisRaw("Horizontal");
        moveV = Input.GetAxisRaw("Vertical");

        //H = Y, V = X, this is intentional, not a mistake.
        camH = Input.GetAxis("Mouse Y") * camSpeedY * Time.deltaTime;
        camV = Input.GetAxis("Mouse X") * camSpeedX * Time.deltaTime;

        aimButton = Input.GetKey(KeyCode.Mouse1);
    }
    void MovePlayer()
    {
        if (!aimButton)
        {
            if (moveDirection != Vector3.zero)
            {
                model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(moveDirection), 0.1f);
            }
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
            moveSpeed = baseSpeed;
        }
        else
        {
            moveSpeed = aimingSpeed;
        }
        moveDirection = (orientation.forward * moveV) + (orientation.right * moveH);
        body.AddForce(moveSpeed * 10f * moveDirection);


    }
    void MoveCamera()
    {

        Quaternion cameraFacing = new Quaternion (0, Camera.main.transform.rotation.y, 0, Camera.main.transform.rotation.w);
        orientation.rotation = cameraFacing;


        yRotate += camV;
        xRotate -= camH;
        xRotate = Mathf.Clamp(xRotate, -90f, 90f);


        transform.rotation = Quaternion.Euler(0f, yRotate, 0f);
        camPivot.localRotation = Quaternion.Euler(xRotate, 0f, 0f);

        if (aimButton)
        {
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, orientation.transform.rotation, 0.1f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 35f, 0.2f);

        }

    }
}
