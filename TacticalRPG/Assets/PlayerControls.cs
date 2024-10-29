using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour

   
{
    public GameObject[] playerUnits;
    public GameObject activeUnit;
    public GameObject plumbob;
    public float plumbobOffset = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("playerunit"); // populate array of player units
        activeUnit = playerUnits[0];
        Debug.Log("Player units found:");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            Debug.Log(playerUnits[i].name);
        }
        Debug.Log("Unit Count:" + playerUnits.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleActiveUnit();
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
        Debug.Log("Active unit: " + activeUnit.name + " // Index: " + index);

        float plumbobHeight = plumbobOffset + activeUnit.transform.position.y;
        plumbob.transform.position = new Vector3(activeUnit.transform.position.x, plumbobHeight, activeUnit.transform.position.z);
    }
}
