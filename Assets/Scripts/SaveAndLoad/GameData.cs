using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int sceneID;
    public List<ObjectData> objectsData = new List<ObjectData>();
}

[Serializable]
public class ObjectData
{
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public string tag;
}
