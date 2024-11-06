
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Health/Armour")]
    public int health = 10;

    [Header("Movement")]
    public float baseSpeed = 8f;
    public float aimingSpeed = 3f;
    float moveSpeed;
    RaycastHit rayHitFloor;

    [Header("Action Points")]
    public int actionPoints;
    public int tempAP;
    public int maxActionPoints = 10;
    public float movementAPMult = 0.2f;

    [Header("Camera")]
    [SerializeField] float camSpeedX = 50f;
    [SerializeField] float camSpeedY = 5f;
    public Transform orientation;
    float moveH, moveV, camH, camV;
    float xRotate, yRotate;
    bool fireButton, aimButton;
    public bool moving;
    bool backspaceDown, spacebarDown;


    [Header("Weapon Output")]
    public Ray weaponRaycast;
    public RaycastHit rayCollision;
    public GameObject target;
    public LayerMask layerMask;

    [Header("Weapon Stats")]
    public float baseVariance = 1f;
    public float accuracyRadius;
    public float accuracyMultiplier = 0.15f;
    public float maxVariance = 2f;
    public float startingVariance;
    public float varianceReductionRate;
    public float maxRange = 1000f;
    public int rounds = 5;
    public int shotAPCost = 5;
    public int damage = 6;

    Vector3 moveDirection;
    Vector3 startPosition, endPosition;
    Rigidbody body;

    PlayerControls playerControls;
    new CapsuleCollider collider;
    [SerializeField] Animator modelAnimator;
    [SerializeField] Animator hitboxAnimator;
    GameObject activeUnit;
    Transform camPivot;
    Camera cam;
    [SerializeField] GameObject model;

    public float animSpeedMultiplier = 0.1f;


    void Start()
    {

        cam = Camera.main;
        body = GetComponent<Rigidbody>();
        playerControls = Camera.main.GetComponent<PlayerControls>();
        collider = transform.GetComponent<CapsuleCollider>();
        camPivot = transform.Find("CamPivot");

        Cursor.lockState = CursorLockMode.Locked;
        moving = false;
        actionPoints = maxActionPoints;
    }

    void Update()
    {
        activeUnit = playerControls.activeUnit;
        if (health < 1)
        {
            modelAnimator.SetBool("Dead", true);
            hitboxAnimator.SetBool("Dead", true);
        }

        if (activeUnit != gameObject) { return; }
        else
        {
            GetInput();
            SetAnimationParams();
        }
        hitboxAnimator.gameObject.transform.position = modelAnimator.gameObject.transform.position;
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


    void GetInput()    // TO DO - Normalise WASD input, but not Analog stick input.
    {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");

        //H = Y, V = X, this is intentional, not a mistake. Horrible, I know.
        camH = Input.GetAxis("Mouse Y") * camSpeedY * Time.deltaTime;
        camV = Input.GetAxis("Mouse X") * camSpeedX * Time.deltaTime;

        fireButton = Input.GetKeyDown(KeyCode.Mouse0);
        aimButton = Input.GetKey(KeyCode.Mouse1);
        backspaceDown = Input.GetKeyDown(KeyCode.Backspace);
        spacebarDown = Input.GetKeyDown(KeyCode.Space);
    }

    void MovePlayer()
    {
        Ray surfaceDetect = new Ray(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), Vector3.down);

        Vector3 floorNormal = Vector3.up;

        if (Physics.Raycast(surfaceDetect, out rayHitFloor, collider.height * 0.55f))
        {
            floorNormal = rayHitFloor.normal;
        }

        moveDirection = Vector3.ProjectOnPlane((orientation.forward * moveV) + (orientation.right * moveH), floorNormal);
        // moveDirection = Vector3.Normalize(moveDirection);
        // Revisit when separate keyboard and gamepad control schemes have been implemented.

        ApplyMovementEffects();

        if (!aimButton)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
            moveSpeed = baseSpeed;
        }
        else
        {
            moveSpeed = aimingSpeed;
            if (fireButton && !moving)
            {
                actionPoints = tempAP;
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
            Vector3 slopeCorrected = new Vector3(moveDirection.x, 0, moveDirection.z);
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(slopeCorrected, Vector3.up), 0.1f);

        }
        else
        {
            model.transform.rotation = Quaternion.Euler(0, modelFacing, 0f);
        }

    }

    public void FireWeapon()
    {
        if (shotAPCost <= actionPoints)
        {
            float randX = cam.transform.forward.x + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
            float randY = cam.transform.forward.y + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
            float randZ = cam.transform.forward.z + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);

            Vector3 rayForward = new Vector3(randX, randY, randZ);
            weaponRaycast = new Ray(camPivot.transform.position, CirulariseAccuracyBloom(cam.transform.forward, rayForward));

            if (Physics.Raycast(weaponRaycast, out rayCollision, maxRange, layerMask))
            {
                target = rayCollision.transform.gameObject;
                if (target.tag == "critbox")
                {
                    target.GetComponent<ThirdPersonMovement>().health -= damage * 2;
                    Debug.Log(target + " was CRIT");
                    Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.green, 10f);
                }
                if (target.tag == "hitbox")
                {
                    target.GetComponent<ThirdPersonMovement>().health -= damage;
                    Debug.Log(target + " was hiy");
                    Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.green, 10f);
                }

            }
            else
            {
                target = null;
                Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.red, 10f);
            }

            actionPoints -= shotAPCost;
        }
        else { Debug.Log("Not enough AP!"); }
    }

    void ApplyMovementEffects()
    {
        tempAP = actionPoints;
        accuracyRadius = startingVariance;

        if (spacebarDown && !moving)   // PLACEHOLDER - Start unit's movement for turn
        {
            moving = true;
            startPosition = transform.position;
        }
        if (moving)
        {
            endPosition = transform.position;
            accuracyRadius = AccuracyBloom(startPosition, endPosition);
            tempAP = MovementCost(startPosition, endPosition);

            if (tempAP >= 0)
            {
                body.AddForce(moveSpeed * Vector3.ProjectOnPlane(moveDirection, rayHitFloor.normal));
            }
            else
            {
                body.AddForce(startPosition - body.transform.position, ForceMode.Force); // towards startingPosition
                // Potentially will cause problems when NavMesh pathfinding is implemented. Alter to push towards the last 'corner' in the path instead?
            }
        }
        if (backspaceDown || !moving) // PLACEHOLDER - End unit's movement for turn
        {
            moving = false;
            actionPoints = tempAP;
            startingVariance = accuracyRadius;
        }
    }

    float AccuracyBloom(Vector3 startPosition, Vector3 endPosition)
    {
        float result = (accuracyRadius / 100) + (DistanceMoved(startPosition, endPosition) / 10) * accuracyMultiplier;

        result = Mathf.Clamp(result, (startingVariance), (maxVariance / 100));
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
        }
        return randomised;
    }

    int MovementCost(Vector3 startPosition, Vector3 endPosition)
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
        modelAnimator.SetFloat("Speed", Vector3.Magnitude(body.velocity * animSpeedMultiplier));
        modelAnimator.SetBool("Aiming", aimButton);
        if (fireButton) { modelAnimator.SetTrigger("Shoot"); }

        hitboxAnimator.SetFloat("Speed", Vector3.Magnitude(body.velocity * animSpeedMultiplier));
        hitboxAnimator.SetBool("Aiming", aimButton);
        if (fireButton) { hitboxAnimator.SetTrigger("Shoot"); }
    }
}
