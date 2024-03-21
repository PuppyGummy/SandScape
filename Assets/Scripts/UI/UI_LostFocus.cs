using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LostFocus : MonoBehaviour
{
    void Update()
    {
        if (InteractionManager.Instance.GetSelectedObjects().Count == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
