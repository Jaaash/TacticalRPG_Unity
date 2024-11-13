using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TopDownCamMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotateSpeed = 5f;
    [SerializeField] float maxZoom = 15f;
    [SerializeField] float minZoom = 1f;
    [SerializeField] GameObject camParentY, camParentZ;
    [SerializeField] Transform orientation;
    GameObject activeUnit;
    Camera cam;
    Rigidbody body;
    float moveH, moveV, rotateAmount, zoomAmount, floorHeight = 0f;
    public float heightOffset = 2f;


    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        activeUnit = cam.GetComponent<PlayerGUI>().activeUnit;
        if (activeUnit == null)
        {
            moveH = Input.GetAxis("Horizontal") * moveSpeed;
            moveV = Input.GetAxis("Vertical") * moveSpeed;
            rotateAmount = Input.GetAxis("QE") * rotateSpeed * Time.deltaTime;
            zoomAmount = Input.GetAxis("Mouse ScrollWheel") * -rotateSpeed * Time.deltaTime;

            MoveCamera();
            RotateCamera();
            ZoomCamera();
            cam.transform.LookAt(transform);
        }
    }

    void MoveCamera()
    {
        if (GetFloorHeight() != 0)
        {
            floorHeight = GetFloorHeight();
        }

        Quaternion cameraFacing = new Quaternion(0, Camera.main.transform.rotation.y, 0, Camera.main.transform.rotation.w);
        orientation.rotation = cameraFacing;

        Vector3 moveDirection = (orientation.forward * moveV) + (orientation.right * moveH);
        moveDirection = Vector3.Normalize(moveDirection);
        // Analog input not properly supported currently, revisit in future.

        body.AddForce((moveSpeed * moveDirection) * Time.deltaTime * 100f);
        body.transform.position = new Vector3(body.transform.position.x, floorHeight + heightOffset, body.transform.position.z);
    }
    void RotateCamera()
    {
        transform.Rotate(Vector3.up, rotateAmount);
    }
    void ZoomCamera()
    {
        float newZoomZ = Mathf.Clamp(camParentZ.transform.localPosition.z + zoomAmount, minZoom, maxZoom);
        float newZoomY = Mathf.Clamp(newZoomZ * newZoomZ / 10f, 0.1f, maxZoom);

        camParentY.transform.localPosition = new Vector3(camParentY.transform.localPosition.x, newZoomY, camParentY.transform.localPosition.z);
        camParentZ.transform.localPosition = new Vector3(camParentZ.transform.localPosition.x, camParentZ.transform.localPosition.y, newZoomZ);
    }
    float GetFloorHeight()
    {
        Ray surfaceDetect = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(surfaceDetect, out RaycastHit rayHitFloor))
        {
            return rayHitFloor.point.y;   
        }
        else { return 0f; }
    }
}
