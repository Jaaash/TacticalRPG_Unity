using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewGhost : MonoBehaviour
{
    [SerializeField] int maxBounces = 2;
    int bounces = 0;
    float lifespan = 2f;

    void Start()
    {
        StartCoroutine(DeleteSelf());
    }

    void Update()
    {
        
    }

    IEnumerator DeleteSelf()
    {
        yield return new WaitForSeconds(lifespan);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        bounces++;
        Debug.Log("Boing");
        if (bounces > maxBounces)
        {
            Debug.Log("Bounce limit exceeded");
            Destroy(gameObject);
        }
    }
}
