using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ThirdPersonMovement : MonoBehaviour
{

    public float baseSpeed = 8f;
    public float aimingSpeed = 3f;
    float moveSpeed;
    public float jumpForce = 10f;
    public float vel;
    public int actionPoints;
    public int maxActionPoints = 10;

    public float camSpeedX = 50f;
    public float camSpeedY = 5f;
    public Transform orientation;
    float moveH, moveV, camH, camV;
    float xRotate, yRotate;
    bool aimButton;
    bool moving;


    [Header("Weapon")]
    public Vector3 crosshair;
    public Ray weaponRaycast;
    public RaycastHit rayCollision;
    public GameObject target;
    public float baseVariance = 1f;
    public float accuracyRadius;
    public float accuracyMultiplier = 0.2f;
    public float maxVariance = 2f;
    public float maxRange = 1000f;
    public int rounds = 1;
    LayerMask layerMask;

    Vector3 moveDirection;
    public Vector3 startPosition, endPosition;
    Rigidbody body;

    PlayerControls playerControls;
    GameObject activeUnit;
    Transform camPivot;
    Transform camParent;
    Camera cam;
    [SerializeField] GameObject model;

    public float movementCost = 1;

    void Start()
    {

        cam = Camera.main;
        body = GetComponent<Rigidbody>();
        playerControls = Camera.main.GetComponent<PlayerControls>();
        camPivot = transform.Find("CamPivot");
        camParent = transform.Find("CamPivot/CamParent");

        Cursor.lockState = CursorLockMode.Locked;
        moving = false;
        actionPoints = maxActionPoints;
    }

    void Update()
    {
        activeUnit = playerControls.activeUnit;

        if (activeUnit != gameObject) { return; }
        else if (actionPoints != 0)
        {
            GetInput();
            MoveCamera();
            if (Input.GetKeyDown(KeyCode.Space))   // Start movement for turn
            {
                moving = true;
                startPosition = transform.position;
                Debug.Log("Start movement");
            }
            if (moving) 
            {
                endPosition = transform.position;
                AccuracyBloom(startPosition, endPosition, actionPoints, movementCost);
            }
            if (Input.GetKeyUp(KeyCode.Space)) // End movement for turn
            {
                moving = false;
                Debug.Log("End Movement");
                Debug.DrawLine(startPosition, endPosition, Color.red);
            }
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
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");

        //H = Y, V = X, this is intentional, not a mistake. Horrible, I know.
        camH = Input.GetAxis("Mouse Y") * camSpeedY * Time.deltaTime;
        camV = Input.GetAxis("Mouse X") * camSpeedX * Time.deltaTime;

        aimButton = Input.GetKey(KeyCode.Mouse1);
    }
    void MovePlayer()
    {
        moveDirection = (orientation.forward * moveV) + (orientation.right * moveH);
        body.AddForce(moveSpeed * 10f * moveDirection);
        vel = Mathf.Max(Mathf.Abs(body.velocity.x), Mathf.Abs(body.velocity.z));

        if (!aimButton)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
            moveSpeed = baseSpeed;
        }

        else
        {
            moveSpeed = aimingSpeed;
            if (Input.GetAxisRaw("Fire1") == 1f)
            {
                FireWeapon();
            }
        }



    }
    void MoveCamera()
    {
        float modelFacing = model.transform.eulerAngles.y;

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
        else if (moveDirection != Vector3.zero)
        {
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(moveDirection), 0.1f);
        }
        else
        {
            model.transform.rotation = Quaternion.Euler(0, modelFacing, 0f);
        }

    }
    public void AccuracyBloom(Vector3 startPosition, Vector3 endPosition, int actionPoints, float movementCost)   // TO DO: Tidy up these Arguments.
    {
        Vector3 distanceMoved = endPosition - startPosition;
        accuracyRadius = (baseVariance / 100) + ((Vector3.Magnitude(distanceMoved) / 10) * accuracyMultiplier);
        int apCost = Convert.ToInt16(Math.Round(Vector3.Magnitude(distanceMoved) * movementCost));
        Debug.Log(apCost);
        actionPoints -= apCost;
        accuracyRadius = Mathf.Clamp(accuracyRadius, 0f, (maxVariance / 100));
    }

    public void FireWeapon()
    {
        float randX = cam.transform.forward.x + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randY = cam.transform.forward.y + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randZ = cam.transform.forward.z + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);

        Vector3 rayForward = new Vector3(randX, randY, randZ);

        weaponRaycast = new Ray(camPivot.transform.position, rayForward);

        if (Physics.Raycast(weaponRaycast, out rayCollision, maxRange, layerMask))
        {
            target = rayCollision.transform.gameObject;
            Debug.Log(target + " was hit");
        }
        else
        {
            target = null;
        }
        Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.red, 10f);

    }
}
