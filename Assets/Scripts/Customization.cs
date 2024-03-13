using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Customization : MonoBehaviour
{
    public enum BodyShape {Small, Medium, Large}
    
    public bool allowColorChange;
    public bool allowShapeChange;
    public bool allowStyleChange;

    public List<Material> materials = new List<Material>();
}
