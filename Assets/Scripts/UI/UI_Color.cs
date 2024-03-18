using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Color : MonoBehaviour
{
    public Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (!material)
        {
            Debug.LogError("No material! Failed to setup");
            return;
        }
        GetComponent<Image>().color = material.color;
    }
}
