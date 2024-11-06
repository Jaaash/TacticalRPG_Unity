using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDScript : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI apDisplay;
    [SerializeField] TextMeshProUGUI movingIndicator;
    [SerializeField] GameObject activeUnit;
    [SerializeField] ThirdPersonMovement unitControls;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        activeUnit = cam.GetComponent<PlayerControls>().activeUnit;
        unitControls = activeUnit.GetComponent<ThirdPersonMovement>();
        apDisplay.text = "AP: " + unitControls.tempAP + " / " + unitControls.maxActionPoints;

        if (unitControls.moving)
        { movingIndicator.enabled = true; }
        else
        { movingIndicator.enabled = false; }
    }
}
