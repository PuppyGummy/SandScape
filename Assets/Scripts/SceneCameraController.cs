using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCameraController : MonoBehaviour
{
    [Header("Components")] public GameObject cameraControllerObject;
    
    private float currentRotation;

    public float rotationAmount;
    public float maxUp;
    public float maxDown;
    
    public void TurnCameraRight()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), rotationAmount, Space.World);
    }

    public void TurnCameraLeft()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), rotationAmount *- 1, Space.World);
    }

    public void TurnCameraUp()
    {
        if(currentRotation + rotationAmount >= maxUp)
            return;
        
        gameObject.transform.Rotate(new Vector3(1, 0, 0), rotationAmount, Space.World);
        currentRotation += rotationAmount;
    }

    public void TurnCameraDown()
    {
        if(currentRotation - rotationAmount < maxDown)
            return;
        
        gameObject.transform.Rotate(new Vector3(1, 0, 0), rotationAmount * -1,  Space.World);
        currentRotation -= rotationAmount;
    }
}
