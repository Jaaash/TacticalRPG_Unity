using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour

   // TO DO: Add top-down interface
{
    public GameObject[] playerUnits;
    public GameObject activeUnit;
    GameObject camParent;


    void Start()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("playerunit");
        activeUnit = playerUnits[0];
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleActiveUnit();
        }

        camParent = activeUnit.transform.Find("CamPivot/CamParent").gameObject;
        transform.parent = camParent.transform;
        gameObject.transform.position = camParent.transform.position;
        gameObject.transform.rotation = camParent.transform.rotation;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            NewTurn();
        }

    }
    void CycleActiveUnit()
    {

        int index = System.Array.IndexOf(playerUnits, activeUnit);
        index++;
        if (index > (playerUnits.Length - 1))
        {
            index = 0;
        }
        activeUnit = playerUnits[index];


    }
    void NewTurn()
    {
        foreach (GameObject unit in playerUnits) 
        {
            unit.GetComponent<ThirdPersonMovement>().actionPoints = unit.GetComponent<ThirdPersonMovement>().maxActionPoints;
        }
    }
}
