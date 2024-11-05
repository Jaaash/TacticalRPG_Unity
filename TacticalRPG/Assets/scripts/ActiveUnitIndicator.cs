using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveUnitIndicator : MonoBehaviour
{

    [SerializeField] new GameObject camera;
    [SerializeField] float yOffset = 3.0f;
    GameObject activeUnit;
    PlayerControls playerControls;

    // Start is called before the first frame update
    void Start()
    {
        playerControls = camera.GetComponent<PlayerControls>();
    }

    // Update is called once per frame
    void Update()
    {
        
        activeUnit = playerControls.activeUnit;

        float plumbobHeight = yOffset + activeUnit.transform.position.y;
        transform.position = new Vector3(activeUnit.transform.position.x, plumbobHeight, activeUnit.transform.position.z);
    }
}
