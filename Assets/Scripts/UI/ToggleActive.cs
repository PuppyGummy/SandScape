using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class ToggleActive : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
