
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
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
    Transform grenadeParent;
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
    Vector3 lastFacing;

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
    public bool fireButton, aimButton, moving;
    float moveH, moveV, camH, camV;
    float xRotate, yRotate;
    bool spacebarDown, reloadButton, grenadeButton;

    [Header("Weapon Setup")]
    public WeaponHandler weapon;
    public GrenadeHandler grenade;
    public bool grenadeEquipped;


    void Start()
    {

        cam = Camera.main;
        body = GetComponent<Rigidbody>();
        playerControls = Camera.main.GetComponent<PlayerGUI>();
        collider = transform.GetComponent<CapsuleCollider>();
        grenade = transform.GetComponent<GrenadeHandler>();
        camPivot = transform.Find("CamPivot");
        grenadeParent = camPivot.Find("GrenadeParent");
        

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        moving = false;
        grenadeEquipped = false;
        actionPoints = maxActionPoints;
        weapon.accuracyRadius = weapon.baseVariance;
        weapon.startingVariance = weapon.baseVariance;

        lastFacing = model.transform.position + Vector3.forward;
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
            RotateModel();

            if (!aimButton)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, 0.2f);
                moveSpeed = baseSpeed;
                fireButton = false;
            }
            else
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 35f - weapon.aimingZoom, 0.2f);
                moveSpeed = aimingSpeed;
                if (fireButton)
                {
                    moving = false;
                    actionPoints = tempAP;
                    if (grenadeEquipped)
                    {
                        grenade.Throw(grenadeParent);
                        grenade.Unequip();
                        fireButton = false;
                    }
                    else
                    {
                        FireWeapon();
                    }
                }
            }
            if (grenadeButton)
            {
                if (!grenadeEquipped)
                {
                    grenade.Equip(grenadeParent);
                }
                else if (grenadeEquipped)
                {
                    grenade.Unequip();
                }
                grenadeButton = false;
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
        spacebarDown |= Input.GetKeyDown(KeyCode.Space);
        reloadButton |= Input.GetKeyDown(KeyCode.R);
        grenadeButton |= Input.GetKeyDown(KeyCode.G); 
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
        moveDirection = Vector3.Normalize(moveDirection); // Analog input not properly supported currently, revisit in future.

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

    }

    void RotateModel()
    {

        if (aimButton)  // while aiming
        {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, orientation.transform.rotation, 0.1f);
            lastFacing = model.transform.position + model.transform.forward;

            Debug.DrawLine(lastFacing, transform.position, Color.red, 0.1f);
        }
        else if (moveDirection != Vector3.zero)  // while moving
        {
            Vector3 slopeCorrected = new Vector3(moveDirection.x, 0, moveDirection.z);
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(slopeCorrected, Vector3.up), 0.1f);
            lastFacing = model.transform.position + model.transform.forward;

            Debug.DrawLine(lastFacing, transform.position, Color.blue, 0.1f);
        }
        else  // while no WASD or RightMouse
        {
            lastFacing = new Vector3(lastFacing.x, model.transform.position.y, lastFacing.z);
            model.transform.LookAt(lastFacing);
            Debug.DrawLine(lastFacing, transform.position, Color.yellow, 0.1f);
        }

    }

    public void FireWeapon()
    {
        if (weapon.shotAPCost <= actionPoints && weapon.roundsLoaded > 0)
        {
            weapon.Fire(gameObject);
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
            fireButton = false;

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
        if (spacebarDown && moving) // PLACEHOLDER - End unit's movement for turn
        {
            moving = false;
            actionPoints = tempAP;
            weapon.startingVariance = weapon.accuracyRadius;
            spacebarDown = false;
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
        playerControls.GetAllUnits();
        Destroy(gameObject);
    }

}
