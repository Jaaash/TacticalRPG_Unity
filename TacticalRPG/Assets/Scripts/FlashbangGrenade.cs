using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashbangGrenade : MonoBehaviour
{
    float fuseTime = 3f;
    public CapsuleCollider coll;
    public Rigidbody body;
    Light flash;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ActivateGrenade());
        flash = GetComponent<Light>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ActivateGrenade()
    {

        yield return new WaitForSeconds(fuseTime);
        Debug.Log("Kaboom!");
        flash.enabled = true;
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
