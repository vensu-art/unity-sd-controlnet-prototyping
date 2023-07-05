// The Script takes the input field and resizes it to fit the text inside it.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldResize : MonoBehaviour
{
    public TMP_InputField textField;
    public TextMeshProUGUI textMesh;

    void Start()
    {
        
    }

    public void Resize() // Get the text from the InputField and puts it in the text component
    {
        // if there is no text, set the text to a space
        if (textField.text == "")
        {
            textMesh.text = " ";
        }
        else
        {
            textMesh.text = textField.text;
        }
    }
}
