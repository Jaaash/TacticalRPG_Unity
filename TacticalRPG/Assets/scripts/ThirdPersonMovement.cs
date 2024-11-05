
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ThirdPersonMovement : MonoBehaviour
{

    public float baseSpeed = 8f;
    public float aimingSpeed = 3f;
    float moveSpeed;
    public int actionPoints;
    [SerializeField] int startingAP, tempAP;
    public int maxActionPoints = 10;
    public float movementAPMult = 0.2f;

    [SerializeField] float camSpeedX = 50f;
    [SerializeField] float camSpeedY = 5f;
    public Transform orientation;
    float moveH, moveV, camH, camV;
    float xRotate, yRotate;
    bool fireButton, aimButton;
    bool moving;
    bool spacebarUp, spacebarDown;


    [Header("Weapon Output")]
    public Ray weaponRaycast;
    public RaycastHit rayCollision;
    public GameObject target;
    LayerMask layerMask;
    [Header("Weapon Stats")]
    public float baseVariance = 1f;
    public float accuracyRadius;
    public float accuracyMultiplier = 0.2f;
    public float maxVariance = 2f;
    public float maxRange = 1000f;
    public int rounds = 1;

    Vector3 moveDirection;
    Vector3 startPosition, endPosition;
    Rigidbody body;

    PlayerControls playerControls;
    Animator animator;
    GameObject activeUnit;
    Transform camPivot;
    Camera cam;
    [SerializeField] GameObject model;

    public float animSpeedMultiplier = 0.1f;


    void Start()
    {

        cam = Camera.main;
        body = GetComponent<Rigidbody>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        playerControls = Camera.main.GetComponent<PlayerControls>();
        camPivot = transform.Find("CamPivot");

        Cursor.lockState = CursorLockMode.Locked;
        moving = false;
        actionPoints = maxActionPoints;
    }

    void Update()
    {
        activeUnit = playerControls.activeUnit;

        if (activeUnit != gameObject) { return; }
        else
        {
            GetInput();

            SetAnimationParams();
        }
    }

    void FixedUpdate()
    {
        if (activeUnit != gameObject) { return; }
        else
        {
            MovePlayer();
            MoveCamera();
        }
    }

    void GetInput()
    {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");

        //H = Y, V = X, this is intentional, not a mistake. Horrible, I know.
        camH = Input.GetAxis("Mouse Y") * camSpeedY * Time.deltaTime;
        camV = Input.GetAxis("Mouse X") * camSpeedX * Time.deltaTime;

        fireButton = Input.GetKeyDown(KeyCode.Mouse0);
        aimButton = Input.GetKey(KeyCode.Mouse1);
        spacebarUp = Input.GetKeyUp(KeyCode.Space);
        spacebarDown = Input.GetKeyDown(KeyCode.Space);
    }

    void MovePlayer()
    {
        moveDirection = (orientation.forward * moveV) + (orientation.right * moveH);
        ApplyMovementLimit();

        if (!aimButton)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
            moveSpeed = baseSpeed;
        }
        else
        {
            moveSpeed = aimingSpeed;
            if (fireButton)
            {
                FireWeapon();
            }
        }

    }

    void MoveCamera()
    {
        float modelFacing = model.transform.eulerAngles.y;

        Quaternion cameraFacing = new Quaternion(0, Camera.main.transform.rotation.y, 0, Camera.main.transform.rotation.w);
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

    public void FireWeapon()
    {
        float randX = cam.transform.forward.x + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randY = cam.transform.forward.y + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randZ = cam.transform.forward.z + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);

        Vector3 rayForward = new Vector3(randX, randY, randZ);
        weaponRaycast = new Ray(camPivot.transform.position, CirulariseAccuracyBloom(cam.transform.forward, rayForward));

        if (Physics.Raycast(weaponRaycast, out rayCollision, maxRange, layerMask))
        {
            target = rayCollision.transform.gameObject;
            Debug.Log(target + " was hit");
            Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.blue, 10f);
        }
        else
        {
            target = null;
            Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.red, 10f);
        }

    }

    void ApplyMovementLimit()
    {
        startingAP = actionPoints;
        tempAP = actionPoints;

        if (spacebarDown)   // PLACEHOLDER - Start unit's movement for turn
        {
            moving = true;
            startPosition = transform.position;
        }
        if (moving)
        {
            endPosition = transform.position;
            accuracyRadius = AccuracyBloom(startPosition, endPosition);
            tempAP = MovementCost(startingAP, startPosition, endPosition);
            if (tempAP >= 0)
            {
                body.AddForce(moveSpeed * moveDirection);
            }
            else
            {
                body.AddForce(startPosition - body.transform.position, ForceMode.Force);
                // Find a way to reduce 'bouncing' when reaching tempAP == -1?
                // Potentially will cause problems when NavMesh pathfinding is implemented. May need to alter to push towards the last 'corner' in the path instead.
            }
        }
        if (spacebarUp) // PLACEHOLDER - End unit's movement for turn
        {
            moving = false;
            actionPoints = tempAP;
            Debug.DrawLine(startPosition, endPosition, Color.red);
        }
    }

    float AccuracyBloom(Vector3 startPosition, Vector3 endPosition)
    {
        float result = (baseVariance / 100) + (DistanceMoved(startPosition, endPosition) / 10) * accuracyMultiplier;

        result = Mathf.Clamp(result, 0f, (maxVariance / 100));
        return result;

        //TO DO - Recoil
    }

    Vector3 CirulariseAccuracyBloom(Vector3 crosshair, Vector3 randomised)
    {
        float adjacent = Vector3.Magnitude(crosshair);
        float opposite = accuracyRadius;

        float tan = (Mathf.Tan(opposite / adjacent)) * Mathf.Rad2Deg;
        float angle = Vector3.Angle(crosshair, randomised);

        if (angle > tan)
        {
            float difference = angle - tan;
            randomised = crosshair;
            Debug.Log("angle adjusted: " + angle + " / " + tan);
        }
        return randomised;
    }

    int MovementCost(int startingAP, Vector3 startPosition, Vector3 endPosition)
    {
        int apCost = Convert.ToInt32(Math.Round(DistanceMoved(startPosition, endPosition) * movementAPMult));
        tempAP -= apCost;
        return tempAP;
    }

    public float DistanceMoved(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 shortestPath = endPosition - startPosition;
        return Vector3.Magnitude(shortestPath);  // TO DO - Implement NavMesh + pathfinding around obstacles.
    }

    void SetAnimationParams()
    {
        animator.SetFloat("Speed", Vector3.Magnitude(body.velocity * animSpeedMultiplier));
        animator.SetBool("Aiming", aimButton);
        if (fireButton) { animator.SetTrigger("Shoot"); }
    }
}
