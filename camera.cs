using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public float CamMoveSpeed = 120.0f;
    public GameObject CameraFollowObj;
    Vector3 FollowPos;
    public float clampAngl = 80.0f;
    public float inputSens = 150.0f;
    public GameObject CamObj;
    public GameObject playerObj;
    public float camDistanceX2Plyr;
    public float camDistanceY2Plyr;
    public float camDistanceZ2Plyr;
    public float mouseX;
    public float mouseY;
    public float finalXInput;
    public float finalZInput;
    public float smoothX;
    public float smoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float InputX = Input.GetAxis("Horizontal");
        float InputZ = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        finalXInput = mouseX;
        finalZInput =  mouseY;

        rotY += finalXInput * inputSens * Time.deltaTime;
        rotX += finalZInput * inputSens * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -clampAngl, clampAngl);

        Quaternion localRot = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRot;
    }

    void LateUpdate()
    {
        CamUpdate();  
    }

    void CamUpdate()
    {
        Transform target = CameraFollowObj.transform;

        float stp = CamMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, stp);
    }
}
