using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrenadeHandler : MonoBehaviour
{
    public float throwForce = 20f;
    bool unequipButton;
    float secondsBetweenPreviews = 0.5f;
    int apCost = 6;

    Rigidbody previewBody;
    GameObject currentGrenade;
    ThirdPersonMovement unitControls;

    [SerializeField] GameObject grenadePrefab;
    [SerializeField] GameObject ghostGrenade;

    void Start()
    {
        unitControls = transform.root.GetComponent<ThirdPersonMovement>();
    }

    void Update()
    {
        if (unitControls.grenadeEquipped == true)
        {
            unequipButton |= Input.GetKeyDown(KeyCode.G);
        }
    }

    public void Equip(Transform originPoint)
    {
        StartCoroutine(PreviewThrow(originPoint));
        unitControls.grenadeEquipped = true;
    }

    public void Unequip()
    {
        StopAllCoroutines();
        unitControls.grenadeEquipped = false;
    }

    public void Throw(Transform originPoint)
    {
        if (apCost <= unitControls.actionPoints)
        {
            unitControls.actionPoints -= apCost;
            currentGrenade = Instantiate(grenadePrefab, originPoint);
            currentGrenade.transform.parent = null;
            Rigidbody body = currentGrenade.GetComponent<Rigidbody>();

            body.AddForce(originPoint.forward * throwForce, ForceMode.Impulse);
            body.AddTorque(Vector3.up);
        }
        else
        {
            Unequip();
            Debug.Log("Not enough AP");
        }
    }

    IEnumerator PreviewThrow(Transform originPoint)
    {
        while (true)
        {
            GameObject previewGrenade = Instantiate(ghostGrenade, originPoint);
            previewBody = previewGrenade.GetComponent<Rigidbody>();
            previewBody.AddForce(originPoint.forward * throwForce, ForceMode.Impulse);
            previewBody.AddTorque(Vector3.up);
            yield return new WaitForSeconds(secondsBetweenPreviews);
        }
    }
}
