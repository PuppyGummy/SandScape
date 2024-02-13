using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneCameraController : MonoBehaviour
{
    [Header("Components")] public GameObject cameraControllerObject;
    
    private float currentRotation;

    public float rotationYAmount;
    [FormerlySerializedAs("rotationAmount")] public float rotationXAmount;
    public float maxUp;
    public float maxDown;
    
    public void TurnCameraRight()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), rotationYAmount, Space.World);
    }

    public void TurnCameraLeft()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), rotationYAmount *- 1, Space.World);
    }

    public void TurnCameraUp()
    {
        if(currentRotation + rotationXAmount >= maxUp)
            return;
        
        gameObject.transform.Rotate(new Vector3(1, 0, 0), rotationXAmount, Space.Self);
        currentRotation += rotationXAmount;
    }

    public void TurnCameraDown()
    {
        if(currentRotation - rotationXAmount < maxDown)
            return;
        
        gameObject.transform.Rotate(new Vector3(1, 0, 0), rotationXAmount * -1,  Space.Self);
        currentRotation -= rotationXAmount;
    }

    public void ResetCameraPositionAndRotation()
    {
        gameObject.transform.rotation = Quaternion.identity;
        currentRotation = 0.0f;
    }
}
