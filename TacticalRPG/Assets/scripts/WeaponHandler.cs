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
    public void Fire(GameObject shooter)
    {
        weaponRayStart = shooter.transform.Find("CamPivot/WeaponRayStart").transform;
        Debug.Log(shooter);

        if (isShotgun)
        {
            for (int i = 0; i < shotsPerClick; i++)
            {
                HitScan(shooter);
            }
            Recoil(); // Recoil only after all rays have been fired.
            roundsLoaded--;
        }
        if (isBurstFire)
        {
            StartCoroutine(BurstFire(0.1f, shooter));
        }
        else if (!isShotgun && !isBurstFire) 
        {
            HitScan(shooter);
            Recoil();
            roundsLoaded--;
        }
    }

    public void Reload()
    {
        roundsLoaded = magazineSize;
    }

    public void HitScan(GameObject shooter)
    {

        Ray weaponRaycast;
        RaycastHit rayCollision;
        GameObject target;

        activeUnit = cam.GetComponent<PlayerGUI>().activeUnit;
        if (activeUnit != null)
        {
            if (activeUnit.CompareTag("playerunit"))
            {
                shooter = cam.gameObject;
            }
        }

        float randX = shooter.transform.forward.x + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randY = shooter.transform.forward.y + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);
        float randZ = shooter.transform.forward.z + UnityEngine.Random.Range(-accuracyRadius, accuracyRadius);

        Vector3 rayForward = new Vector3(randX, randY, randZ);
        weaponRaycast = new Ray(weaponRayStart.transform.position, CirulariseAccuracyBloom(shooter.transform.forward, rayForward));

        Debug.DrawRay(weaponRaycast.origin, weaponRaycast.direction * maxRange, Color.red, 10f);

        if (Physics.Raycast(weaponRaycast, out rayCollision, maxRange, layerMask))
        {
            target = rayCollision.collider.gameObject;

            if (target.CompareTag("critbox"))
            {
                Debug.DrawLine(weaponRaycast.origin, rayCollision.point, Color.green, 10f);
                target.transform.root.GetComponent<ThirdPersonMovement>().health -= damage * 2;
            }
            else if (target.CompareTag("hitbox"))
            {
                Debug.DrawLine(weaponRaycast.origin, rayCollision.point, Color.blue, 10f);
                target.transform.root.GetComponent<ThirdPersonMovement>().health -= damage;
            }
            else
            {
                target = null;
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

    IEnumerator BurstFire(float delay, GameObject shooter)
    {
        for (int i = 0; i < shotsPerClick; i++)
        {
            if (shotsPerClick > 0)
            {
                HitScan(shooter);
                Recoil();
                roundsLoaded--;
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
