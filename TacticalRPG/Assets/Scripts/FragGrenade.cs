using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    public float fuseTime = 3f;
    public CapsuleCollider coll;
    public Rigidbody body;
    public float radius = 15f;
    public int baseDamage = 15;
    public float dmgFalloff = 1f;
    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ActivateGrenade());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ActivateGrenade()
    {
        yield return new WaitForSeconds(fuseTime);
        Debug.Log("Kaboom!");

        Collider[] targets = Physics.OverlapSphere(transform.position, radius, layerMask);

        foreach (Collider target in targets) 
        {
            Ray lineOfSight = new Ray(transform.position, Vector3.Normalize(target.transform.position - transform.position));
            RaycastHit hit;

            if (Physics.Raycast(lineOfSight, out hit, radius))
            {
                if (hit.collider.transform.IsChildOf(target.transform.root))
                {
                    Vector3 targetVector = target.transform.position - transform.position;
                    float distance = Vector3.Magnitude(targetVector);

                    int damage = Convert.ToInt32(baseDamage - (distance * dmgFalloff));

                    target.transform.root.gameObject.GetComponent<ThirdPersonMovement>().health -= damage;

                    Debug.Log("targetVector" + targetVector);
                    Debug.Log(baseDamage + " - (" + distance + " * " + dmgFalloff + ")");
                    Debug.Log(target.name + " took dmg: " + damage);
                }
                else
                {
                    Debug.Log("Couldn't Get LOS to " + target.name);
                }
            }
        }
        Destroy(gameObject);
    }
}
