using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public float spawnHeight = 5f;
    public void SpawnObject(GameObject associatedObject)
    {
        Instantiate(associatedObject, new Vector3(0f, spawnHeight, 0f), transform.rotation);
    }
}
