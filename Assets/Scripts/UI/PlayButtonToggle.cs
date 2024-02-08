using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonToggle : MonoBehaviour
{
    public Button button;

    private void Update()
    {
        button.interactable = InteractionManager.Instance.playerObject;
    }
}
