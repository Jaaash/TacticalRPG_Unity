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
    public float accuracyRadius = 0.1f;
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
        float randX = cam.transform.forward.x + Random.Range(-accuracyRadius, accuracyRadius);
        float randY = cam.transform.forward.y + Random.Range(-accuracyRadius, accuracyRadius);
        float randZ = cam.transform.forward.z + Random.Range(-accuracyRadius, accuracyRadius);

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
