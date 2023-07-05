using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Toggles")]
    public Toggle settingsToggle;
    public Toggle galleryToggle;
    public Toggle diffusionToggle;

    [Header("UI Menus")]
    public GameObject settingsMenu;
    public GameObject galleryMenu;
    public GameObject diffusionMenu;

    // Start is called before the first frame update
    void Start()
    {
        diffusionMenu.SetActive(false);
        settingsMenu.SetActive(false);

        // Add listener to settings toggle
        settingsToggle.onValueChanged.AddListener(delegate
        {
            ToggleValueChanged(settingsToggle);
        });

        // Add listener to gallery toggle
        galleryToggle.onValueChanged.AddListener(delegate
        {
            ToggleValueChanged(galleryToggle);
        });

        // Add listener to diffusion toggle
        diffusionToggle.onValueChanged.AddListener(delegate
        {
            ToggleValueChanged(diffusionToggle);
        });
    }

    // If settings toggle is on, turn on settings menu
    // If settings toggle is off, turn off settings menu
    void ToggleValueChanged(Toggle change)
    {
        if (change.isOn)
        {
            if (change == settingsToggle)
            {
                settingsMenu.SetActive(true);
            }
            else if (change == galleryToggle)
            {
                galleryMenu.SetActive(true);
            }
            else if (change == diffusionToggle)
            {
                diffusionMenu.SetActive(true);
            }
        }
        else
        {
            if (change == settingsToggle)
            {
                settingsMenu.SetActive(false);
            }
            else if (change == galleryToggle)
            {
                galleryMenu.SetActive(false);
            }
            else if (change == diffusionToggle)
            {
                diffusionMenu.SetActive(false);
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
