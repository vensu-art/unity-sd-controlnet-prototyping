using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class WebcamAccessUI : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    
    // This is the RawImage component where you want to display the camera feed.
    // You can set it in the inspector.
    public RawImage rawImage;
    public Button captureButton;

    // This TextMeshPro InputField is where you can enter the prompt you want to send.
    public TMP_InputField textField;


    // Start is called before the first frame update
    void Start()
    {
        // Get the default camera
        WebCamDevice device = WebCamTexture.devices[0];
        
        webcamTexture = new WebCamTexture(device.name);
        
        // Assign the texture to the RawImage
        rawImage.texture = webcamTexture;
        
        // Adjust the RawImage aspect ratio to the one of the webcam
        rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
        rawImage.rectTransform.localScale = new Vector3(1f, (float)webcamTexture.height / (float)webcamTexture.width, 1f);
        
        // Start the camera
        webcamTexture.Play();

        // Add a click listener to the button
        //captureButton.onClick.AddListener(CaptureAndSendImage);
        captureButton.onClick.AddListener(CaptureImage);
    }


    // Captures the webcam image and saves it to a JPG file on the device.
    void CaptureImage()
    {
        // Create a Texture2D with the size of the webcam texture
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);
        
        // Fill the texture with the pixels from the webcam
        texture.SetPixels(webcamTexture.GetPixels());
        
        // Apply the changes to the texture
        texture.Apply();

        // Encode the texture to a JPG
        byte[] bytes = texture.EncodeToJPG();

        // Save the image to a file
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/image.jpg", bytes);

        // Console log the path to the saved image
        Debug.Log("Saved to: " + Application.persistentDataPath + "/image.jpg");
    }
}
