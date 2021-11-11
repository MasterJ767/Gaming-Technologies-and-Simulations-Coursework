using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls camera for 2d, in-level player
/// </summary>
public class CameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public GameObject head;

    [NonSerialized]
    public bool inUI;
    private float xRotation = 0f;
    

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        if (!inUI)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Apply rotations to camera and the player's head model
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            head.transform.localRotation = transform.localRotation;

            // Rotate the player's body to follow the direction the head is facing
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
