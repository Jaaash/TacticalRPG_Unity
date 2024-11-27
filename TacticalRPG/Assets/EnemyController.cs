using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyController : MonoBehaviour
{
    PlayerGUI playerGUI;
    ThirdPersonMovement tpmControls;
    public float visionRange = 20f;
    Transform head;
    // Start is called before the first frame update
    void Start()
    {
        tpmControls = GetComponent<ThirdPersonMovement>();
        playerGUI = Camera.main.GetComponent<PlayerGUI>();
        head = transform.Find("Space_Soldier_A/SK_Soldier_Head");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeTurn()
    {
        List<GameObject> allTargets = GetVisiblePlayerUnits().allVisible;
        GameObject nearestTarget = GetVisiblePlayerUnits().closestVisible;
        Debug.Log("Nearest Target: " + nearestTarget);

        if (nearestTarget != null)
        {
            FireAt(nearestTarget);
        }
    }
    (List<GameObject> allVisible, GameObject closestVisible) GetVisiblePlayerUnits()
    {
        List<GameObject> visibleUnits = new List<GameObject>();
        GameObject nearestVisibleUnit = null;
        float shortestRange = visionRange;

        foreach (GameObject playerUnit in playerGUI.playerUnits)
        {
            RaycastHit hit;
            Transform target = playerUnit.transform.Find("Space_Soldier_A/SK_Soldier_Head");

            Ray lineOfSight = new Ray(head.position, Vector3.Normalize(target.position - head.position));
            if (Physics.Raycast(lineOfSight, out hit))     // Stops working if any line of sight can't be found?????
            {
                Debug.Log(hit.transform.gameObject.name);
                if (hit.collider.gameObject == playerUnit.gameObject) 
                {
                    visibleUnits.Add(target.gameObject);
                    if (hit.distance < shortestRange)
                    {
                        nearestVisibleUnit = target.root.gameObject;
                        shortestRange = hit.distance;
                        Debug.DrawLine(head.position, hit.point, Color.magenta, 5f);
                    }
                    else
                    {
                        Debug.DrawLine(head.position, hit.point, Color.yellow, 5f);
                    }
                }
            }
        }
        return (visibleUnits, nearestVisibleUnit);
    }

    void FireAt(GameObject target)
    {
        if (tpmControls.weapon.roundsLoaded > 0)
        {
            transform.LookAt(target.transform.position, Vector3.up);
            tpmControls.weapon.Fire(gameObject);
        }
    }

    void MoveToCover()
    {

    }
}
