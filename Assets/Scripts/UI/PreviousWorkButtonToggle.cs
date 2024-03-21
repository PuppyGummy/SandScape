using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class PreviousWorkButtonToggle : MonoBehaviour
{
    private Button button;
    private Image img;
    private void Awake()
    {
        button = GetComponent<Button>();
        img = GetComponent<Image>();
        if (File.Exists(SaveLoadManager.SavePath))
        {
            button.interactable = true;
            img.color = Color.white;
        }
        else
        {
            button.interactable = false;
            img.color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }
}