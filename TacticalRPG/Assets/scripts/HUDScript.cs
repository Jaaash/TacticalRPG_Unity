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
    [SerializeField] TextMeshProUGUI tooltipDefault;
    [SerializeField] TextMeshProUGUI tooltipMoving;
    [SerializeField] TextMeshProUGUI tooltipAiming;
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
            weaponDisplay.text = weapon.weaponName + "\nAmmo: " + weapon.roundsLoaded + " / " + weapon.magazineSize + "\nShoot Cost: " + weapon.shotAPCost + " AP" + "\nReload Cost: " + weapon.reloadAPCost + " AP";

            if (unitControls.moving)
            {
                movingIndicator.gameObject.SetActive(true);
                UpdateToolTips(1);
            }
            else if (unitControls.aimButton)
            {
                UpdateToolTips(2);
            }
            else
            {
                movingIndicator.gameObject.SetActive(false);
                UpdateToolTips(0);
            }
        }
    }
    void UpdateToolTips(int index)
    {
        switch (index)
        {

            default:
                tooltipDefault.gameObject.SetActive(true);
                tooltipMoving.gameObject.SetActive(false);
                tooltipAiming.gameObject.SetActive(false);
                break;
            case 1:
                tooltipDefault.gameObject.SetActive(false);
                tooltipMoving.gameObject.SetActive(true);
                tooltipAiming.gameObject.SetActive(false);
                break;

            case 2:
                tooltipDefault.gameObject.SetActive(false);
                tooltipMoving.gameObject.SetActive(false);
                tooltipAiming.gameObject.SetActive(true);
                break;
        }
    }
}
