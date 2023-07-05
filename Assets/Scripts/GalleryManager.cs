using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalleryManager : MonoBehaviour
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private ProjectManager projectManager;

    private string imageDirectoryPath;

    private void Start()
    {
        imageDirectoryPath = Application.persistentDataPath + "/Images/";
        Directory.CreateDirectory(imageDirectoryPath);

        LoadImages();
    }

    public void SaveImage(Texture2D texture, string imageName)
    {
        // Create a path to a new file with the given name
        string filePath = Path.Combine(imageDirectoryPath, imageName);

        // Try to convert the texture to jpg data
        byte[] imageBytes;
        try
        {
            imageBytes = texture.EncodeToJPG();
            Debug.Log("Successfully encoded texture to JPG.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to encode texture to JPG: " + e.Message);
            return;
        }

        // Try to write the jpg data to the file
        try
        {
            File.WriteAllBytes(filePath, imageBytes);
            Debug.Log("Successfully wrote JPG to file at " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write JPG to file: " + e.Message);
            return;
        }

        // Try to add the image to the gallery
        try
        {
            AddImageToGallery(texture, filePath);
            Debug.Log("Successfully added image to gallery.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to add image to gallery: " + e.Message);
            return;
        }
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

        // Add a listener to the delete button to remove the image
        Button deleteButton = imageObject.transform.GetChild(2).GetComponent<Button>(); 
        deleteButton.onClick.AddListener(() => DeleteImage(imageObject, imagePath));
    }

    // Display the image in the resultImage RawImage
    private void DisplayImage(Sprite imageSprite)
    {
        projectManager.resultImage.texture = imageSprite.texture;
    }


    private void DeleteImage(GameObject imageObject, string imagePath)
    {
        File.Delete(imagePath);
        Destroy(imageObject);
    }
}
