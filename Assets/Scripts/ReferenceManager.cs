using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ReferenceManager : MonoBehaviour
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] public GameObject referencePrefab;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private ProjectManager projectManager;
    [SerializeField] private TMP_Dropdown dropdown1;
    [SerializeField] private TMP_Dropdown dropdown2;
    [SerializeField] private GameObject referenceImage;

    private string imageDirectoryPath;

    public static Texture2D SelectedImage { get; private set; } // static variable to store the selected image

    private void Start()
    {
        imageDirectoryPath = Application.persistentDataPath + "/References/";
        Directory.CreateDirectory(imageDirectoryPath);

        LoadImages();

        // Make sure the referenceImage and the dropdowns are set
        if (referenceImage == null || dropdown1 == null || dropdown2 == null)
        {
            Debug.LogError("Controlled object or dropdowns are not set.");
            return;
        }

        // Add listeners to the dropdowns
        dropdown1.onValueChanged.AddListener(delegate
        {
            UpdateReferenceImageState();
        });

        dropdown2.onValueChanged.AddListener(delegate
        {
            UpdateReferenceImageState();
        });

        // Initialize the state of the controlled object
        UpdateReferenceImageState();
    }

    private void LoadImages()
    {
        string[] imagePaths = Directory.GetFiles(imageDirectoryPath);

        foreach (string imagePath in imagePaths)
        {
            Texture2D imageTexture = new Texture2D(2, 2);
            imageTexture.LoadImage(File.ReadAllBytes(imagePath));
            AddImageToGallery(imageTexture, imagePath);
        }
    }

    private void AddImageToGallery(Texture2D imageTexture, string imagePath)
    {
        GameObject imageObject = Instantiate(imagePrefab, scrollViewContent);

        // Get the Image component from the child object
        Image uiImage = imageObject.transform.GetChild(0).GetComponent<Image>(); 
        uiImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Add a listener to the Button component to display the image when the button is clicked
        Button displayButton = imageObject.transform.GetChild(1).GetComponent<Button>(); 
        displayButton.onClick.AddListener(() => DisplayImage(uiImage.sprite));
    }

    // Display the image in the resultImage RawImage
    private void DisplayImage(Sprite imageSprite)
    {
        Image imageComponent = referencePrefab.transform.GetChild(0).GetComponent<Image>();
        SelectedImage = imageSprite.texture;
        
        if (imageComponent != null)
        {
            imageComponent.sprite = imageSprite;
            Debug.Log("Selected image: " + imageSprite.name);
        }
        else
        {
            Debug.LogError("Image component is null. Unable to display image.");
        }
    }


    private void UpdateReferenceImageState()
    {
        // Check if either dropdown has a value of 6
        if (dropdown1.value == 7 || dropdown2.value == 7)
        {
            // Set the controlled object to active if either dropdown's value is 6
            referenceImage.SetActive(true);
        }
        else
        {
            // Set the controlled object to inactive if neither dropdown's value is 6
            referenceImage.SetActive(false);
        }
    }
}
