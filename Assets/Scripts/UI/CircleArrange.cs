using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleArrange : MonoBehaviour
{
    [SerializeField] float radius = 100;
    int count;
    private List<GameObject> objects;

    private void Start()
    {
        count = transform.childCount;
        // Debug.Log("Count is: " + count);

        for (int i = 0; i < count; ++i)
        {
            float circleposition = (float)i / (float)count;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius;
            float y = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;

            transform.GetChild(i).transform.localPosition = new Vector3(x, y);
        }
    }
}
