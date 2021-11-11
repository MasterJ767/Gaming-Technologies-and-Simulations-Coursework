using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines button functions for the menus found in the overworld and level
/// </summary>
public class OverworldMenu : MonoBehaviour
{
    public UnityEngine.Object mainScene;
    public UnityEngine.Object overworldScene;
    public GameObject tip;
    public GameObject menus;
    public GameObject main;
    public GameObject settings;
    public GameObject[] pages;
    public bool isActive;

    private PlayerController pc;
    private OverworldPlayerController opc;
    private int currentPageIndex;

    private void Update()
    {
        if (isActive)
        {
            if (Input.GetButton("Cancel"))
            {
                if (main.activeSelf)
                {
                    // If in overworld use regular Menu Off function 
                    if (opc != null)
                    {
                        MenuOff();
                    }
                    else
                    {
                        MenuOffLevel();
                    }
                }
                else
                {
                    settings.SetActive(false);
                    foreach (GameObject page in pages)
                    {
                        page.SetActive(false);
                    }
                    main.SetActive(true);
                }
            }
        }
    }

    public void MenuOn(OverworldPlayerController playerController)
    {
        opc = playerController;
        MenuOnCommon();
    }
    
    public void MenuOn(PlayerController playerController)
    {
        pc = playerController;
        MenuOnCommon();
    }

    private void MenuOnCommon()
    {
        Cursor.lockState = CursorLockMode.Confined;
        isActive = true;
        tip.SetActive(false);
        menus.SetActive(true);
    }
    
    public void MenuOff()
    {
        MenuOffCommon();
        opc.InUI = false;
    }

    public void MenuOffLevel()
    {
        MenuOffCommon();
        pc.InUI = false;
    }

    private void MenuOffCommon()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isActive = false;
        menus.SetActive(false);
        tip.SetActive(true);
    }

    public void SettingsOn()
    {
        main.SetActive(false);
        settings.SetActive(true);
    }

    public void SettingsOff()
    {
        settings.SetActive(false);
        main.SetActive(true);
    }
    
    public void EnterPages()
    {
        main.SetActive(false);
        pages[0].SetActive(true);
        currentPageIndex = 0;
    }

    public void NextPage()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex = (currentPageIndex + 1) % pages.Length;
        pages[currentPageIndex].SetActive(true);
    }

    public void PreviousPage()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex -= 1;
        if (currentPageIndex < 0)
        {
            currentPageIndex = pages.Length - 1;
        }

        pages[currentPageIndex].SetActive(true);
    }

    public void ExitPages()
    {
        pages[currentPageIndex].SetActive(false);
        main.SetActive(true);
    }

    public void Quit()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().SetParameters(State.Menu, SceneManager.GetActiveScene().name, false, false);
        SceneManager.LoadSceneAsync(mainScene.name, LoadSceneMode.Single);
    }

    public void QuitToOverworld()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().SetParameters(State.Overworld, SceneManager.GetActiveScene().name, false, true);
        SceneManager.LoadSceneAsync(overworldScene.name, LoadSceneMode.Single);
    }
}
