using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControls : MonoBehaviour
{
    public Vector3 crosshair;
    public Ray weaponRaycast;
    public RaycastHit rayCollision;
    public GameObject target;
    Transform camPivot;
    public float baseVariance = 1f;
    public float accuracyRadius;
    public float accuracyMultiplier = 0.2f;
    public float maxVariance = 2f;
    public float maxRange = 1000f;
    public int rounds = 1;
    public LayerMask layerMask;
    Animator animator;
    Camera cam;
    PlayerControls playerControls;
    GameObject activeUnit;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();
        playerControls = cam.GetComponent<PlayerControls>();
        camPivot = transform.Find("CamPivot");

    }

    // Update is called once per frame
    void Update()
    {
        activeUnit = playerControls.activeUnit;
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
    public void AccuracyBloom(Vector3 startPosition, Vector3 endPosition, int actionPoints, float movementCost)
    {
        Vector3 distanceMoved = endPosition - startPosition;
        accuracyRadius = ( baseVariance / 100 ) + ((Vector3.Magnitude(distanceMoved) / 10) * accuracyMultiplier);
        int apCost = Convert.ToInt16(Math.Round(Vector3.Magnitude(distanceMoved) * movementCost));
        Debug.Log(apCost);
        actionPoints -= apCost;
        accuracyRadius = Mathf.Clamp(accuracyRadius, 0f, (maxVariance / 100));

        // TO DO:
        // Make bloom only shrink by a certain amount each turn, instead of instantly resetting to zero
        // Merge WeaponControls into ThirdPersonMovement, and rename the whole thing 'UnitControls' for simplicity.
    }
}
