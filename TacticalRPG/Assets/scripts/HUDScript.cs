using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDScript : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI apDisplay;
    [SerializeField] TextMeshProUGUI hpDisplay;
    [SerializeField] TextMeshProUGUI movingIndicator;
    [SerializeField] TextMeshProUGUI weaponDisplay;
    [SerializeField] GameObject activeUnit;
    [SerializeField] ThirdPersonMovement unitControls;
    Camera cam;
    WeaponHandler weapon;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {

        activeUnit = cam.GetComponent<PlayerGUI>().activeUnit;

        if (activeUnit != null)
        {
            unitControls = activeUnit.GetComponent<ThirdPersonMovement>();
            weapon = unitControls.weapon;
            apDisplay.text = "AP: " + unitControls.tempAP + " / " + unitControls.maxActionPoints;
            hpDisplay.text = "HP: " + unitControls.health + " / " + unitControls.maxHealth;
            weaponDisplay.text = weapon.weaponName + ": " + weapon.roundsLoaded + " / " + weapon.magazineSize;

            if (unitControls.moving)
            { movingIndicator.gameObject.SetActive(true); }
            else
            { movingIndicator.gameObject.SetActive(false); }
        }
    }
}
