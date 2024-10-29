using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControls : MonoBehaviour
{

    public Animator animator;
    public new GameObject camera;
    GameObject activeUnit;
    GameObject camParent;
    PlayerControls playerControls;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        camParent = GameObject.Find("camParent");
        playerControls = camera.GetComponent<PlayerControls>();

    }

    // Update is called once per frame
    void Update()
    {
        activeUnit = playerControls.activeUnit;

        if (activeUnit != gameObject)
        {
            return;
        }
        else
        {

        }
    }
}


