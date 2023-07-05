using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneFitScreen : MonoBehaviour
{
    private Vector2 sceenResolution;

    void Start()
    {
        sceenResolution = new Vector2(Screen.width, Screen.height);
        MatchPlaneToScreen();
    }

    void Update()
    {
        if (sceenResolution.x != Screen.width || sceenResolution.y != Screen.height)
        {
            MatchPlaneToScreen();
            sceenResolution = new Vector2(Screen.width, Screen.height);
        }
    }

    private void MatchPlaneToScreen()
    {   
        float planeToCameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float planeHeight = (2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * planeToCameraDistance) / 10.0f;
        float planeWidth = planeHeight * Camera.main.aspect;
        transform.localScale = new Vector3(planeWidth, 1.0f, planeHeight);
    }
}
