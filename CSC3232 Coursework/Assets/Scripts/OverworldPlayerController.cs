using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls every aspect of the 2D, overworld player
/// </summary>
public class OverworldPlayerController : MonoBehaviour
{
    public OverworldMenu menuController;
    
    private GameManager gm;
    private bool _inUi = false;

    public bool InUI
    {
        get => _inUi;
        set
        {
            _inUi = value;
            gm.Pause(value);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        transform.position = gm.playerPosition;
        
        InUI = false;
    }

    private void FixedUpdate()
    {
        if (!_inUi)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.up * y;
            move = Vector3.ClampMagnitude(move, 1f) * 80;

            Vector3 destination = transform.position + (move * Time.fixedDeltaTime);

            // define broad bounding box
            if (destination.x > -560 && destination.x < 560 && destination.y > -80 && destination.y < 240)
            {
                // If in the main corridor allow
                if (destination.y > 0 && destination.y < 80)
                {
                    transform.position = destination;
                }
                // If in the small side corridors
                else if (destination.x > -400 && destination.x < -320 && destination.y <= 0)
                {
                    transform.position = destination;
                }
                else if (destination.x > -160 && destination.x < -80 && destination.y >= 80)
                {
                    transform.position = destination;
                }
                else if (destination.x > 160 && destination.x < 240 && destination.y <= 0)
                {
                    transform.position = destination;
                }
                else if (destination.x > 400 && destination.x < 480 && destination.y >= 80)
                {
                    transform.position = destination;
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Menu"))
        {
            InUI = true;
            menuController.MenuOn(this);
        }
    }
}
