using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImageFlipFlop : MonoBehaviour
{
    private bool flipped;

    [SerializeField]
    public Image imageElement;
    
    public Sprite normalImage;
    public Sprite flippedImage;
    
    public void Flip()
    {
        switch (flipped)
        {
            case true:
                imageElement.sprite = normalImage;
                flipped = false;
                break;
            case false:
                imageElement.sprite = flippedImage;
                flipped = true;
                break;
        }
    }
}
