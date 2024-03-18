using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_TabImage : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite activeImage;
    [SerializeField] private Sprite inactiveImage;
    [SerializeField] private bool startActive = false;
    
    private void Start()
    {
        image = gameObject.GetComponent<Image>();
        
        if(startActive)
            SetActive();
    }

    public void SetActive()
    {
        image.sprite = activeImage;
    }

    public void SetInactive()
    {
        image.sprite = inactiveImage;
    }
}
