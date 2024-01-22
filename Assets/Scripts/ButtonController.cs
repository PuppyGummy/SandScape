using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public GameObject associatedObject;

    public void OnButtonClick()
    {
        GameObject.FindObjectOfType<ObjectSpawner>().SendMessage("SpawnObject", associatedObject);
    }
}
