using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamMovement : MonoBehaviour
{

    float moveH, moveV, camH, camZoom;
    float xRotate, yRotate;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotateSpeedY = 5f;
    CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        MoveCamera();
    }
    void GetInput()
    {
        moveH = Input.GetAxis("Horizontal") * moveSpeed;
        moveV = Input.GetAxis("Vertical") * moveSpeed;

        //Horizontal = Y, Vertical = X, this is intentional, not a mistake. Horrible, I know.
        camH = Input.GetAxis("QE") * rotateSpeedY * Time.deltaTime;
        camZoom = Input.GetAxis("Mouse ScrollWheel");

    }

    void MoveCamera()
    {
        controller.transform.Rotate(0, camH, 0);
        controller.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.y, 0);
        controller.SimpleMove(new Vector3(moveH, 0, moveV));
    }

}
