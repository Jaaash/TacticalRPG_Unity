using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponHandler : MonoBehaviour
{
    public string weaponName;
    public int damage, magazineSize, roundsLoaded, shotAPCost, reloadAPCost, shotsPerClick;
    public float accuracyRadius, startingVariance, baseVariance, movementVarianceMultiplier, maxVariance, varianceReductionRate, maxRange, recoil, aimingZoom;
    public bool isShotgun, isBurstFire;

    Camera cam;
    public GameObject activeUnit;
    [SerializeField] LayerMask layerMask;
    Transform weaponRayStart;
    Transform muzzle;

    private void Start()
    {
        cam = Camera.main;
        accuracyRadius = baseVariance;
        startingVariance = baseVariance;
    }
    public void Fire()
    {
        activeUnit = cam.GetComponent<PlayerGUI>().activeUnit;
        weaponRayStart = activeUnit.transform.Find("CamPivot/WeaponRayStart").transform;

        if (isShotgun)
        {
            for (int i = 0; i < shotsPerClick; i++)
            {
                HitScan();
            }
            Recoil(); // Recoil only after all rays have been fired.
            roundsLoaded--;
        }
        if (isBurstFire)
        {
            StartCoroutine(BurstFire(0.1f));
        }
        else
        {
            HitScan();
            Recoil();
            roundsLoaded--;
        }
    }

    public void Reload()
    {
        roundsLoaded = magazineSize;
    }

    public void HitScan()
    {

        Ray weaponRaycast;
        RaycastHit rayCollision;
        GameObject target;

        float randX = cam.transform.forward.x + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randY = cam.transform.forward.y + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randZ = cam.transform.forward.z + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);

        Vector3 rayForward = new Vector3(randX, randY, randZ);
        weaponRaycast = new Ray(weaponRayStart.transform.position, CirulariseAccuracyBloom(cam.transform.forward, rayForward));

        Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.red, 10f);

        if (Physics.Raycast(weaponRaycast, out rayCollision, maxRange, layerMask))
        {
            target = rayCollision.collider.gameObject;

            if (target.CompareTag("critbox"))
            {
                Debug.Log(target.transform.root + " was CRIT");
                Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.green, 10f);
                target.transform.root.GetComponent<ThirdPersonMovement>().health -= damage * 2;
            }
            else if (target.CompareTag("hitbox"))
            {
                Debug.Log(target.transform.root + " was hit");
                Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.blue, 10f);
                target.transform.root.GetComponent<ThirdPersonMovement>().health -= damage;
            }
            else
            {
                target = null;
                Debug.Log("Miss!");
            }
        }
    }

    void Recoil()
    {
        accuracyRadius = Mathf.Min(accuracyRadius + recoil, maxVariance);
        startingVariance = accuracyRadius;
    }
    public float AccuracyPenalty(Vector3 startPosition, Vector3 endPosition)
    {
        float result = accuracyRadius + (DistanceMoved(startPosition, endPosition) / 10) * movementVarianceMultiplier;
        result = Mathf.Clamp(result, (startingVariance), (maxVariance));
        return result;
    }


    public float DistanceMoved(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 shortestPath = endPosition - startPosition;
        return Vector3.Magnitude(shortestPath);  // TO DO - Implement NavMesh + pathfinding around obstacles.
    }


    Vector3 CirulariseAccuracyBloom(Vector3 crosshair, Vector3 randomised)
     //Use TAN trigonometric function to calculate a circle around crosshair with radius of accuracyRadius, and re-target any shots which would fall outside that circle.
    {
        float adjacent = Vector3.Magnitude(crosshair);
        float opposite = accuracyRadius;

        float tan = (Mathf.Tan(opposite / adjacent)) * Mathf.Rad2Deg;
        float angle = Vector3.Angle(crosshair, randomised);

        if (angle > tan)
        {
            randomised = crosshair;
            Debug.Log("Lucky shot!");
        }
        return randomised;
    }

    IEnumerator BurstFire(float delay)
    {
        for (int i = 0; i < shotsPerClick; i++)
        {
            HitScan();
            Recoil();
            roundsLoaded--;
            yield return new WaitForSeconds(delay);
        }
    }
}
