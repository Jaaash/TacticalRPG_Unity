using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ThirdPersonMovement : MonoBehaviour
{

    public float moveSpeed = 10f;
    public float gravity = 1f;
    public float jumpForce = 10f;

    public float camSpeed = 10f;
    public Transform orientation;
    float moveH, moveV, camH, camV, xRotate, yRotate; 

    Vector3 moveDirection;
    Rigidbody body;

    [SerializeField] PlayerControls playerControls;
    [SerializeField] GameObject activeUnit;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        playerControls = Camera.main.GetComponent<PlayerControls>();
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
        MovePlayer();        
    }
    void GetInput()
    {
        moveH = Input.GetAxisRaw("Horizontal");
        moveV = Input.GetAxisRaw("Vertical");

        //H = Y, V = X, this is intentional, not a mistake.
        camH = Input.GetAxis("Mouse Y") * camSpeed * Time.deltaTime;
        camV = Input.GetAxis("Mouse X") * camSpeed * Time.deltaTime;
    }
    void MovePlayer()
    {
        body.drag = 15.0f;
        moveDirection = (orientation.forward * moveV) + (orientation.right * moveH);
        body.AddForce(moveSpeed * 10f * moveDirection);
    }
    void MoveCamera()
    {
        yRotate += camV;
        xRotate -= camH;
        xRotate = Mathf.Clamp(xRotate, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotate, yRotate, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotate, 0f);
    }
}
