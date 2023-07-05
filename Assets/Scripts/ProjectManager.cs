using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ProjectManager : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private string url = "http://127.0.0.1:7860"; // The URL to the API

    [Header("UI Main Elements")]
    public TMP_InputField textField; // This TextMeshPro InputField is where the prompt is entered.
    public Button sendRequestButton; // This button sends the request.
    public RawImage camImage; // This is the RawImage component where the camera feed is displayed
    public RawImage resultImage; // This is the RawImage where the image is displayed
    public GameObject popUp; // This is the Pop Up window
    public GameObject backgroundImage;
    public Button saveButton;
    public GalleryManager galleryManager;

    [Header("UI Settings Elements")]
    public TMP_InputField negativePrompt; // This TextMeshPro InputField is where the negative prompt is entered.
    public TMP_InputField stepsInput; // This TextMeshPro InputField is where the number of steps is entered.
    public TMP_Dropdown samplerDropdown; // This is the dropdown where the sampler index is selected.
    public TMP_Dropdown controlNetOne; // This is the dropdown where the first controlnet is selected.
    public TMP_Dropdown controlNetTwo; // This is the dropdown where the second controlnet is selected.

    [Header("Stable Diffusion Parameters")]
    public string sd_negative_prompt = "watermark, signature, bad quality"; // The negative prompt to use
    public int sd_steps = 20; // The number of steps to run the model for
    public string sd_sampler_index = "DPM++ 2M Karras"; // The sampler index to use
    public int sd_width = 768; // The width of the output image
    public int sd_height = 512; // The height of the output image

    [Header("ControlNet Parameters")]
    public string cn_module1 = "canny"; // The module of the first image
    public string cn_model1 = "control_v11p_sd15_canny [d14c016b]"; // The model of the first image
    public float cn_weight1 = 1f; // The weight of the first image
    public float cn_guidance_end1 = 1f; // The guidance end value of the first image
    public float cn_threshold_a1; // The threshold A value of the first image

    public string cn_module2 = "depth_midas"; // The module of the second image
    public string cn_model2 = "control_v11f1p_sd15_depth [cfd03158]"; // The model of the second image
    public float cn_weight2 = 1f; // The weight of the second image
    public float cn_guidance_end2 = 1f; // The guidance end value of the second image
    public float cn_threshold_a2; // The threshold A value of the second image

    private int cn_use_first = 1; // Whether to use the first image
    private int cn_use_second = 1; // Whether to use the second image


    [Serializable]
    public class Payload // The payload to send to the API
    {
        public string prompt;
        public string negative_prompt;
        public int steps;
        public int width;
        public int height;
        public string sampler_index;
        public AlwaysonScripts alwayson_scripts;

        [Serializable]
        public class AlwaysonScripts
        {
            public Controlnet controlnet;
        
            [Serializable]
            public class Controlnet
            {
                public List<Args> args;

                [Serializable]
                public class Args
                {
                    public string input_image;
                    public string module;
                    public string model;
                    public float weight;
                    public float guidance_end;
                    public float threshold_a;
                }
            }
        }
    }

    [Serializable]
    public class ResponseData // The response data from the API
    {
        public List<string> images;
        public Dictionary<string, object> parameters;
        public string info;
    }


    // Start is called before the first frame update
    void Start()
    {
        WebCamDevice device = WebCamTexture.devices[0]; // Get the default camera device
        webcamTexture = new WebCamTexture(device.name); // Create a new WebCamTexture with the device name
        camImage.texture = webcamTexture; // Assign the texture to the RawImage
        backgroundImage.GetComponent<Renderer>().material.mainTexture = webcamTexture; // Assign the texture to the background
        
        camImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle); // Rotate the RawImage to match the camera rotation
        camImage.rectTransform.localScale = new Vector3(1f, (float)webcamTexture.height / (float)webcamTexture.width, 1f); // Scale the RawImage to match the camera aspect ratio
        
        webcamTexture.Play(); // Start the camera

        sendRequestButton.onClick.AddListener(SendRequest); // Add a click listener to the button
        saveButton.onClick.AddListener(SaveImage); // Add a click listener to the button

        popUp.SetActive(false); // Hide the RawImage
    }


    // Captures the webcam image and saves it to a JPG file on the device.
    byte[] CaptureImage()
    {
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height); // Create a new Texture2D with the same dimensions as the webcam feed

        texture.SetPixels(webcamTexture.GetPixels()); // Set the pixels of the texture to the pixels of the webcam feed
        texture.Apply(); // Apply the changes to the texture

        byte[] bytes = texture.EncodeToJPG(); // Encode the texture to JPG

        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/image.jpg", bytes); // Save the JPG file to the device
        Debug.Log("Saved to: " + Application.persistentDataPath + "/image.jpg"); // Log the path of the saved image

        return bytes;
    }


    void SendRequest()
    {
        byte[] firstImageBytes = CaptureImage(); // Capture the image and get the bytes
        byte[] secondImageBytes = CaptureImage(); // replace this with the actual method of getting the second image bytes
        string prompt = textField.text; // Get the prompt from the InputField
        textField.text = ""; // Clear the InputField

        StartCoroutine(PostRequest(prompt, firstImageBytes, secondImageBytes)); // Send the request
    }


    IEnumerator PostRequest(string prompt, byte[] firstImageBytes, byte[] secondImageBytes)
    {
        // DEFINING THE PAYLOAD
        // The byte array for the second image

        Payload payload = new Payload();
        payload.prompt = prompt;
        payload.negative_prompt = sd_negative_prompt;
        payload.steps = sd_steps;
        payload.sampler_index = sd_sampler_index;
        payload.width = sd_width; // if this parameter is needed
        payload.height = sd_height; // if this parameter is needed

        if (cn_use_first == 1 || cn_use_second == 1)
        {
            payload.alwayson_scripts = new Payload.AlwaysonScripts();
            payload.alwayson_scripts.controlnet = new Payload.AlwaysonScripts.Controlnet();
            payload.alwayson_scripts.controlnet.args = new List<Payload.AlwaysonScripts.Controlnet.Args>();

            Payload.AlwaysonScripts.Controlnet.Args arg1 = new Payload.AlwaysonScripts.Controlnet.Args();
            // In your request function
            if (controlNetOne.value == 7 && ReferenceManager.SelectedImage != null)
            {
                byte[] imageBytes = ReferenceManager.SelectedImage.EncodeToPNG();
                arg1.input_image = Convert.ToBase64String(imageBytes);
            }
            else
            {
                arg1.input_image = Convert.ToBase64String(firstImageBytes);
            }

            arg1.module = cn_module1;
            arg1.model = cn_model1;
            arg1.weight = cn_weight1;
            arg1.guidance_end = cn_guidance_end1;
            arg1.threshold_a = cn_threshold_a1;

            Payload.AlwaysonScripts.Controlnet.Args arg2 = new Payload.AlwaysonScripts.Controlnet.Args();
            if (controlNetTwo.value == 7 && ReferenceManager.SelectedImage != null)
            {
                byte[] imageBytes = ReferenceManager.SelectedImage.EncodeToPNG();
                arg2.input_image = Convert.ToBase64String(imageBytes);
            }
            else
            {
                arg2.input_image = Convert.ToBase64String(secondImageBytes);
            }

            arg2.module = cn_module2;
            arg2.model = cn_model2;
            arg2.weight = cn_weight2;
            arg2.guidance_end = cn_guidance_end2;
            arg2.threshold_a = cn_threshold_a2;

            Payload.AlwaysonScripts.Controlnet.Args arg3 = new Payload.AlwaysonScripts.Controlnet.Args();
            if (controlNetOne.value == 7 || controlNetTwo.value == 7 && ReferenceManager.SelectedImage != null)
            {
                byte[] imageBytes = ReferenceManager.SelectedImage.EncodeToPNG();
                arg3.input_image = Convert.ToBase64String(imageBytes);
            }

            arg3.module = "shuffle";
            arg3.model = "control_v11e_sd15_shuffle";
            arg3.weight = 1f;
            arg3.guidance_end = 1f;

            if (cn_use_first == 1)
            {
                payload.alwayson_scripts.controlnet.args.Add(arg1);
            }

            if (cn_use_second == 1)
            {
                payload.alwayson_scripts.controlnet.args.Add(arg2);
            }

            if (controlNetOne.value == 7 || controlNetTwo.value == 7 && ReferenceManager.SelectedImage != null)
            {
                payload.alwayson_scripts.controlnet.args.Add(arg3);
            }
        }

        
        

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

                byte[] receivedImageBytes = Convert.FromBase64String(base64Image); // Convert the base64 string to a byte array

                Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D
                
                // Check if the image was loaded successfully
                if (texture.LoadImage(receivedImageBytes)) // Load the image from the byte array
                {
                    Debug.Log("Image loaded successfully"); // Log that the image was loaded successfully

                    popUp.SetActive(true); // Show the RawImage
                    
                    resultImage.texture = texture; // Assign the texture to the RawImage
                    resultImage.gameObject.SetActive(true); // Show the RawImage
                }
            }
        }
        request.Dispose(); // Dispose of the request
    }


    void SaveImage()
    {
        string name = "vensu" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
        Texture2D texture = (Texture2D)resultImage.texture;

        galleryManager.SaveImage(texture, name);
    }


    public void ChangeSteps()
    {   
        sd_steps =  Convert.ToInt32(stepsInput.text);
    }

    public void ChangeSamplerIndex()
    {
        sd_sampler_index = samplerDropdown.options[samplerDropdown.value].text;
    }

    public void ChangeWidth()
    {

    }

    public void ChangeHeight()
    {

    }

    public void ChangeControlNetOne()
    {
        if (controlNetOne.value == 0)
        {
            cn_use_first = 0;
        }
        else if (controlNetOne.value > 0 && controlNetOne.value <= 5)
        {
            cn_use_first = 1;
            ChangeControlNetParams(controlNetOne.value, ref cn_module1, ref cn_model1, ref cn_weight1, ref cn_guidance_end1, ref cn_threshold_a1);
        }
    }

    public void ChangeControlNetTwo()
    {
        if (controlNetTwo.value == 0)
        {
            cn_use_second = 0;
        }
        else if (controlNetTwo.value > 0 && controlNetTwo.value <= 7)
        {
            cn_use_second = 1;
            ChangeControlNetParams(controlNetTwo.value, ref cn_module2, ref cn_model2, ref cn_weight2, ref cn_guidance_end2, ref cn_threshold_a2);
        }
    }

    private void ChangeControlNetParams(int dropdownValue, ref string module, ref string model, ref float weight, ref float guidance_end, ref float threshold_a)
    {
        switch (dropdownValue - 1)
        {
            case 0:
                module = "canny";
                model = "control_v11p_sd15_canny";
                weight = 1f;
                guidance_end = 0.75f;
                Debug.Log("Canny");
                break;
            case 1:
                module = "mlsd";
                model = "control_v11p_sd15_mlsd";
                weight = 1f;
                guidance_end = 0.75f;
                Debug.Log("MLSD");
                break;
            case 2:
                module = "depth_midas";
                model = "control_v11f1p_sd15_depth";
                weight = 1f;
                guidance_end = 0.8f;
                Debug.Log("Depth");
                break;
            case 3:
                module = "normal_bae";
                model = "control_v11p_sd15_normalbae";
                weight = 1f;
                guidance_end = 0.4f;
                Debug.Log("Normal");
                break;
            case 4:
                module = "seg_ofade20k";
                model = "control_v11p_sd15_seg";
                weight = 1f;
                guidance_end = 1f;
                Debug.Log("Segmentation");
                break;
            case 5:
                module = "openpose_full";
                model = "control_v11p_sd15_openpose";
                weight = 1f;
                guidance_end = 1f;
                Debug.Log("Openpose");
                break;
            case 6:
                module = "reference_adain+attn";
                weight = 1f;
                guidance_end = 1f;
                threshold_a = 0.5f;
                Debug.Log("Reference Adain Attn");
                break;
            case 7:
                module = "shuffle";
                model = "control_v11e_sd15_shuffle";
                weight = 1f;
                guidance_end = 1f;
                Debug.Log("Shuffle");
                break;
            default:
                break;
        }
    }

    public void ChangeNegativePrompt()
    {
        sd_negative_prompt = negativePrompt.text;
    }

        private void OnDestroy()
    {
        // stop the webcam when the object is destroyed
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}
