using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerGUI : MonoBehaviour

{
    public Camera cam;                             // TO DO???? - Move script from Camera to an empty handler?
    public GameObject topDownViewpoint;
    public GameObject[] playerUnits;
    public GameObject[] enemyUnits;
    public GameObject activeUnit;
    public Canvas thirdPersonUI;
    public Canvas topDownUI;
    [SerializeField] Button endTurn;


    void Start()
    {
        cam = Camera.main;
        endTurn.onClick.AddListener(EndPlayerTurn);
        GetAllActiveUnits();
    }

    // Update is called once per frame
    void Update()
    {

        if (activeUnit == null)
        {
            SetTopDownCam(true);

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.DrawRay(cam.transform.position, hit.point, Color.red, 1f);

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
            SetTopDownCam(false);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleActiveUnit();
        }
        if (Input.GetKeyDown(KeyCode.Return)) // Return to Top-Down view
        { 
            activeUnit = null;
        } 

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
        if (activeUnit == null) 
        {
            activeUnit = playerUnits[0];
            AttachCamToActiveUnit();
        }
        else
        {
            activeUnit.GetComponent<ThirdPersonMovement>().moving = false;
            int i = System.Array.IndexOf(playerUnits, activeUnit);
            i++;
            if (i > (playerUnits.Length - 1))
            {
                i = 0;
            }
            activeUnit = playerUnits[i];
            AttachCamToActiveUnit();
        }
    }
    void AttachCamToActiveUnit() 
    { 
        GameObject camParent = activeUnit.transform.Find("CamPivot/CamParent").gameObject;
        transform.parent = camParent.transform;
        cam.transform.SetPositionAndRotation( camParent.transform.position, camParent.transform.rotation );
    }
    void EndPlayerTurn()
    {
        SetPlayerControl(false);
        foreach (GameObject unit in playerUnits) 
        {
            ThirdPersonMovement unitControls = unit.GetComponent<ThirdPersonMovement>();
            unitControls.moving = false;
            unitControls.actionPoints = unitControls.maxActionPoints;
            unitControls.weapon.startingVariance -= unitControls.weapon.varianceReductionRate;
            if (unitControls.weapon.startingVariance < unitControls.weapon.baseVariance)
            {
                unitControls.weapon.startingVariance = unitControls.weapon.baseVariance;
            }
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
        SetPlayerControl(true);
    }

    void SetPlayerControl(bool isPlayerTurn)
    {
        if (!isPlayerTurn)
        {
            activeUnit = null;
        }
        foreach (GameObject unit in playerUnits)
        {
            ThirdPersonMovement unitControls = unit.GetComponent<ThirdPersonMovement>();
            unitControls.enabled = isPlayerTurn;
        }
    }
    void GameOver(bool playerWin)
    {
        if (playerWin)  { Debug.Log("Area secure. Mission Complete."); }
        else            { Debug.Log("Squad eliminated. Mission Failed.");  }
    }

    void SetTopDownCam(bool isTopdownMode)
    {
        topDownUI.enabled = isTopdownMode;
        thirdPersonUI.enabled = !isTopdownMode;

        if (isTopdownMode)
        {
            Cursor.lockState = CursorLockMode.Confined;
            cam.transform.parent = topDownViewpoint.transform;
            cam.transform.SetPositionAndRotation(topDownViewpoint.transform.position, cam.transform.rotation);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            AttachCamToActiveUnit();
        }
    }

    public void GetAllActiveUnits()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("playerunit");
        enemyUnits = GameObject.FindGameObjectsWithTag("enemyunit");
    }

}
