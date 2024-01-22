
// There are two types of contour outlines in the scene:
// a geometry one and a post-processing (pencil, more stylized) one.
// The geometry one is on the material shader,
// may or may not work well w/ low-poly meshes, as the geometry is LOWWW.
// The pencil one's script is on Main Camera,
// and can be only seen in the "Game" window (not "Scene"!).
// Press O for toggle on/off geometry contour,
// Press P for toggle on/off pencil contour.
// :-D)
// I can also do a macro *KEYWORD* on/off,
// (so you don't need to manually assign the target render object).
// But I kinda forget how to...and I don't want to deal with a lot of shader varients.
// But I can manage if you developers need it QwQ


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    public Renderer targetRenderer;
    private string togglePropertyName = "_OutlineToggle";
    private bool isGeoOutlineEnabled = false;
    private Camera mainCamera;
    private PencilContour pencilContourScript;
    // Start is called before the first frame update
    void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("Target renderer not assigned!");
            return;
        }
        SetGeoOutlineEnabled(isGeoOutlineEnabled);
        mainCamera = Camera.main;
        pencilContourScript = mainCamera.GetComponent<PencilContour>();
        if (pencilContourScript == null)
        {
            Debug.LogError("PencilContour script not found on Main Camera!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            isGeoOutlineEnabled = !isGeoOutlineEnabled;
            SetGeoOutlineEnabled(isGeoOutlineEnabled);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Toggle the state of the PencilContour script on the Main Camera
            if (pencilContourScript != null)
            {
                pencilContourScript.TogglePencilContour();
            }
            else
            {
                Debug.LogError("PencilContour script not found on Main Camera!");
            }
        }
    }
    void SetGeoOutlineEnabled(bool isEnabled)
    {
        targetRenderer.material.SetInt(togglePropertyName, isEnabled ? 1 : 0);
    }
}