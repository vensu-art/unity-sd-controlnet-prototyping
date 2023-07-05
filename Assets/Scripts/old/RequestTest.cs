using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;

public class RequestTest : MonoBehaviour
{
    // The URL of your API
    private string url = "http://127.0.0.1:7860";

    // This is your TextMeshPro InputField where you enter the prompt
    public TMP_InputField textField;

    // This is your button that sends the request
    public Button sendRequestButton;

    // This is your RawImage where you display the image
    public RawImage rawImage;

    // This is the Pop Up
    public GameObject popUp;


    void Start()
    {
        // Add a click listener to the button
        sendRequestButton.onClick.AddListener(SendRequest);

        // Hide the RawImage
        popUp.SetActive(false);
    }


    void SendRequest()
    {
        string prompt = textField.text;
        textField.text = "";

        StartCoroutine(PostRequest(prompt));
    }


    [Serializable]
    public class Payload
    {
        public string prompt;
        public int steps;
        public int width;
        public int height;
        public string sampler_index;
    }

    [Serializable]
    public class ResponseData
    {
        public List<string> images;
        public Dictionary<string, object> parameters;
        public string info;
    }


    IEnumerator PostRequest(string prompt)
    {
        Payload payload = new Payload();
        payload.prompt = prompt;
        payload.steps = 20;
        payload.sampler_index = "DPM++ 2M Karras";
        payload.width = 768;
        payload.height = 512;

        string jsonPayload = JsonUtility.ToJson(payload);

        byte[] jsonRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(url + "/sdapi/v1/txt2img", "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Response received");

            // Parse the JSON response
            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);

            // Check if there are any images in the response
            if (data.images.Count > 0)
            {
                Debug.Log("Image received");


                // Get the first image from the response
                string image = data.images[0];
                string base64Image;

                // Check if the image string contains a comma
                if (image.Contains(","))
                {
                    base64Image = image.Split(',')[1];
                }
                else
                {
                    base64Image = image;
                }

                // Decode the base64 string to a byte array
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Create a new Texture2D and load the image data into it
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    // The image was successfully loaded into the texture
                    // You can now use the texture to display the image in your UI, save it to a file, etc.
                    Debug.Log("Image loaded successfully");

                    // Set the raw image active
                    popUp.SetActive(true);
                    
                    // Assign the texture to the RawImage and enable it
                    rawImage.texture = texture;
                    rawImage.gameObject.SetActive(true);
                }
            }
        }

        // Dispose of the request object to free up native resources
        request.Dispose();
    }
}
