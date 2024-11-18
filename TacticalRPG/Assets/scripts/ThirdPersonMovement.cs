
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
    [Header("Unit Setup")]
    [SerializeField] Animator modelAnimator;
    [SerializeField] Animator hitboxAnimator;
    [SerializeField] GameObject model;
    Rigidbody body;
    PlayerGUI playerControls;
    new CapsuleCollider collider;
    GameObject activeUnit;
    Transform camPivot;
    Camera cam;

    [Header("Health/Armour")]
    public int health = 10;
    public int maxHealth = 10;

    [Header("Movement")]
    public float baseSpeed = 8f;
    public float aimingSpeed = 3f;
    public float animSpeedMultiplier = 0.1f;
    float moveSpeed;
    RaycastHit rayHitFloor;
    Vector3 moveDirection;
    Vector3 lastDirection;

    [Header("Action Points")]
    public int actionPoints;
    public int tempAP;
    public int maxActionPoints = 10;
    public float movementAPMult = 0.2f;
    Vector3 startPosition, endPosition;

    [Header("Camera")]
    [SerializeField] float camSpeedX = 50f;
    [SerializeField] float camSpeedY = 5f;
    public Transform orientation;

    [Header("Input")]
    bool fireButton, aimButton;
    public bool moving;
    float moveH, moveV, camH, camV;
    float xRotate, yRotate;
    bool spacebarDown, backspaceDown, reloadButton;

    [Header("Weapon Setup")]
    public WeaponHandler weapon;


    void Start()
    {

        cam = Camera.main;
        body = GetComponent<Rigidbody>();
        playerControls = Camera.main.GetComponent<PlayerGUI>();
        collider = transform.GetComponent<CapsuleCollider>();
        camPivot = transform.Find("CamPivot");

        Cursor.lockState = CursorLockMode.Locked;
        moving = false;
        actionPoints = maxActionPoints;
        weapon.accuracyRadius = weapon.baseVariance;
        weapon.startingVariance = weapon.baseVariance;

        lastDirection = transform.forward * 2f;
    }

    void Update()
    {

        if (health < 1)
        {
            modelAnimator.SetBool("Dead", true);
            hitboxAnimator.SetBool("Dead", true);
            StartCoroutine(UnitDead());
        }

        if (reloadButton && weapon.roundsLoaded < weapon.magazineSize)
        {
            if (actionPoints >= weapon.reloadAPCost)
            {
                actionPoints -= weapon.reloadAPCost;
                weapon.Reload();
                reloadButton = false;
            }
            else { Debug.Log("Not enough AP to reload"); reloadButton = false; }
        }

        activeUnit = playerControls.activeUnit;
        if (activeUnit != gameObject) { return; }
        else
        {
            GetInput();
            MoveCamera();

            if (!aimButton)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
                moveSpeed = baseSpeed;
                fireButton = false;
            }
            else
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 35f + -weapon.aimingZoom, 0.2f);
                moveSpeed = aimingSpeed;
                if (fireButton && !moving)
                {
                    actionPoints = tempAP;
                    FireWeapon();
                }
            }
        }

        hitboxAnimator.gameObject.transform.position = modelAnimator.gameObject.transform.position;
    }

    void FixedUpdate()
    {
        MovePlayer();
        SetAnimationParams();
    }


    void GetInput()    // TO DO - Normalise WASD input, but not Analog stick input.
    {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");

        //H = Y, V = X, this is intentional, not a mistake. Horrible, I know.
        camH = Input.GetAxis("Mouse Y") * camSpeedY;
        camV = Input.GetAxis("Mouse X") * camSpeedX;

        fireButton |= Input.GetKeyDown(KeyCode.Mouse0);
        aimButton = Input.GetKey(KeyCode.Mouse1);
        backspaceDown |= Input.GetKeyDown(KeyCode.Backspace);
        spacebarDown |= Input.GetKeyDown(KeyCode.Space);
        reloadButton |= Input.GetKeyDown(KeyCode.R);
    }

    void MovePlayer()
    {
        if (moveDirection != Vector3.zero)
        {
            lastDirection = moveDirection;
        }
        Ray surfaceDetect = new Ray(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), Vector3.down);

        Vector3 floorNormal = Vector3.up;

        if (Physics.Raycast(surfaceDetect, out rayHitFloor, collider.height * 0.55f))
        {
            floorNormal = rayHitFloor.normal;
        }

        moveDirection = Vector3.ProjectOnPlane((orientation.forward * moveV) + (orientation.right * moveH), floorNormal);
        moveDirection = Vector3.Normalize(moveDirection);
        // Analog input not properly supported currently, revisit in future.

        ApplyMovementCost();
    }

    void MoveCamera()
    {
        Quaternion cameraFacing = new Quaternion(0, Camera.main.transform.rotation.y, 0, Camera.main.transform.rotation.w);
        orientation.rotation = cameraFacing;


        yRotate += camV;
        xRotate -= camH;
        xRotate = Mathf.Clamp(xRotate, -90f, 90f);


        transform.rotation = Quaternion.Euler(0f, yRotate, 0f);
        camPivot.localRotation = Quaternion.Euler(xRotate, 0f, 0f);

        RotateModel();

    }

    void RotateModel()
    {
        float modelFacing = (model.transform.position + lastDirection).y;


        if (aimButton)  // while aiming
        {
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, orientation.transform.rotation, 0.1f);
            lastDirection = new Vector3(0, orientation.transform.eulerAngles.y, 0);
        }
        else if (moveDirection != Vector3.zero)  // while moving
        {
            Vector3 slopeCorrected = new Vector3(moveDirection.x, 0, moveDirection.z);
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(slopeCorrected, Vector3.up), 0.1f);
            lastDirection = new Vector3(0, orientation.transform.eulerAngles.y, 0);
        }
        else  // while standing still
        {
            model.transform.rotation = Quaternion.Euler(0, modelFacing, 0f);
        }
        
        //Debug.DrawLine(model.transform.position, modelFacing, Color.magenta, 0.1f);
    }

    public void FireWeapon()
    {
        if (weapon.shotAPCost <= actionPoints && weapon.roundsLoaded > 0)
        {
            weapon.Fire();
            actionPoints -= weapon.shotAPCost;
        }
        else
        { 
            Debug.Log("Not enough AP!");
        }

        fireButton = false;
    }

    void ApplyMovementCost()
    {
        tempAP = actionPoints;
        weapon.accuracyRadius = weapon.startingVariance;

        if (spacebarDown && !moving)   // PLACEHOLDER - Start unit's movement for turn
        {
            moving = true;
            startPosition = transform.position;
            spacebarDown = false;
        }
        if (moving)
        {
            endPosition = transform.position;
            weapon.accuracyRadius = weapon.AccuracyPenalty(startPosition, endPosition);
            tempAP = MovementCost(startPosition, endPosition);

            if (tempAP >= 0)
            {
                body.AddForce(moveSpeed * Vector3.ProjectOnPlane(moveDirection, rayHitFloor.normal));
            }
            else
            {
                body.AddForce(startPosition - body.transform.position, ForceMode.Force); // Push unit towards startingPosition
                // Potentially will cause problems when NavMesh pathfinding is implemented. Alter to push towards the last 'corner' in the path instead?
            }
        }
        if (backspaceDown || !moving) // PLACEHOLDER - End unit's movement for turn
        {
            moving = false;
            actionPoints = tempAP;
            weapon.startingVariance = weapon.accuracyRadius;
            backspaceDown = false;
        }
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
    IEnumerator UnitDead()
    {
        if (activeUnit == gameObject)
        {
            playerControls.activeUnit = null;
        }
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
        playerControls.GetAllActiveUnits();
    }

}
