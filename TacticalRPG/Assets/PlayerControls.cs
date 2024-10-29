using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour

   
{
    public GameObject[] playerUnits;
    public GameObject activeUnit;
    [SerializeField] GameObject camParent;

    // Start is called before the first frame update
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

        camParent = activeUnit.transform.Find("CamParent").gameObject;
        transform.parent = camParent.transform;
        gameObject.transform.position = camParent.transform.position;
        gameObject.transform.rotation = camParent.transform.rotation;

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
}
