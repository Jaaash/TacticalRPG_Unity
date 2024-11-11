using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour

{
    public Camera cam;                             // TO DO???? - Move script from Camera to an empty handler?
    public GameObject topDownViewpoint;
    public GameObject[] playerUnits;
    public GameObject[] enemyUnits;
    public GameObject activeUnit;
    public Canvas thirdPersonUI;
    public Canvas topDownUI;
    GameObject camParent;
    [SerializeField] Button endTurn;


    void Start()
    {
        cam = Camera.main;
        playerUnits = GameObject.FindGameObjectsWithTag("playerunit");
        enemyUnits = GameObject.FindGameObjectsWithTag("enemyunit");
        endTurn.onClick.AddListener(EndPlayerTurn);
    }

    // Update is called once per frame
    void Update()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("playerunit");
        enemyUnits = GameObject.FindGameObjectsWithTag("enemyunit");
        if (activeUnit == null)
        {
            Cursor.lockState = CursorLockMode.Confined;
            cam.transform.parent = topDownViewpoint.transform;
            cam.transform.SetPositionAndRotation(topDownViewpoint.transform.position, topDownViewpoint.transform.rotation);
            topDownUI.enabled = true;
            thirdPersonUI.enabled = false;

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.transform.root.CompareTag("playerunit"))
                    {
                        activeUnit = hit.collider.transform.root.gameObject;
                        AttachCamToActiveUnit();
                    }
                }
            }

        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            topDownUI.enabled = false;
            thirdPersonUI.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleActiveUnit();
        }
        if (Input.GetKeyDown(KeyCode.Return)) { activeUnit = null; } // Return to Top-Down view

        

        if (enemyUnits.Length == 0)
        {
            GameOver(true);
        }
        else if (playerUnits.Length == 0) // If the final enemy and final friendly both die at the same time, it's still a victory for the player.
        {
            GameOver(false);
        }
    }
    void CycleActiveUnit()
    {

        if (activeUnit == null) { activeUnit = playerUnits[0]; }
        else
        {
            int index = System.Array.IndexOf(playerUnits, activeUnit);
            index++;
            if (index > (playerUnits.Length - 1))
            {
                index = 0;
            }
            activeUnit = playerUnits[index];
            AttachCamToActiveUnit();
        }
    }
    void AttachCamToActiveUnit() 
    { 
        camParent = activeUnit.transform.Find("CamPivot/CamParent").gameObject;
        transform.parent = camParent.transform;
        cam.transform.SetPositionAndRotation( camParent.transform.position, camParent.transform.rotation );

    }
    void EndPlayerTurn()
    {
        RemovePlayerControl();
        foreach (GameObject unit in playerUnits) 
        {
            ThirdPersonMovement unitControls = unit.GetComponent<ThirdPersonMovement>();
            unitControls.actionPoints = unitControls.maxActionPoints;
            unitControls.startingVariance -= unitControls.varianceReductionRate / 100;
            unitControls.startingVariance = Mathf.Clamp(unitControls.startingVariance, unitControls.baseVariance / 100, unitControls.maxVariance /100);
        }
        EnemyTurn();
    }
    void EnemyTurn()
    {
        Debug.Log("Starting enemy turn");

        foreach (GameObject unit in enemyUnits)
        {
            Debug.Log(unit.name + " starting turn");
        }

        Debug.Log("Ending enemy turn");
        ReturnPlayerControl();
    }
    void RemovePlayerControl()
    {
        activeUnit = null;
        foreach (GameObject unit in playerUnits)
        {
            ThirdPersonMovement unitControls = unit.GetComponent<ThirdPersonMovement>();
            unitControls.enabled = false;
        }
    }
    void ReturnPlayerControl()
    {
        
        foreach (GameObject unit in playerUnits)
        {
            ThirdPersonMovement unitControls = unit.GetComponent<ThirdPersonMovement>();
            unitControls.enabled = true;
        }
    }
    void GameOver(bool playerWin)
    {
        if (playerWin)  { Debug.Log("Area secure. Mission Complete."); }
        else            { Debug.Log("Squad eliminated. Mission Failed.");  }
    }

}
