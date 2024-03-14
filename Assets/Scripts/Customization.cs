using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Customization : MonoBehaviour
{
    public enum BodyShape {Small, Medium, Large}

    public BodyShape shape;
    public bool allowColorChange;
    public bool allowShapeChange;
    public bool allowStyleChange;

    public List<Material> materials = new List<Material>();

    private MeshFilter headFilter;
    private MeshFilter bodyFilter;
    private MeshFilter topFilter;
    private MeshFilter bottomFilter;
    private MeshFilter shoeFilter;
    private MeshFilter hairFilter;

    private int currentHair = 0;
    private int currentTop = 0;
    private int currentBottom = 0;
    private int currentShoes = 0;

    public void Start()
    {
        SetComponents();
    }

    public void SetFacialExpression(int id)
    {
        headFilter.sharedMesh = CustomizationItemManager.Instance.expressions[id].sharedMesh;
    }

    public void SetBodyShape(int id)
    {
        bodyFilter.sharedMesh = CustomizationItemManager.Instance.bodyShapes[id].sharedMesh;
        shape = (BodyShape)id;
        SetClothesSize();
    }

    public void SetHair(int id)
    {
        hairFilter.sharedMesh = CustomizationItemManager.Instance.hair[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentHair = id;
        SetClothesSize();
    }
    
    public void SetTop(int id)
    {
        topFilter.sharedMesh = CustomizationItemManager.Instance.tops[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentTop = id;
        SetClothesSize();
    }
    
    public void SetBottom(int id)
    {
        bottomFilter.sharedMesh = CustomizationItemManager.Instance.bottoms[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentBottom = id;
        SetClothesSize();
    }
    
    public void SetShoe(int id)
    {
        shoeFilter.sharedMesh = CustomizationItemManager.Instance.shoes[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentShoes = id;
        SetClothesSize();
    }

    private void SetClothesSize()
    {
        topFilter.sharedMesh = CustomizationItemManager.Instance.tops[currentTop].GetComponent<ClothingItem>().itemVariants[(int)shape];
        bottomFilter.sharedMesh = CustomizationItemManager.Instance.bottoms[currentBottom].GetComponent<ClothingItem>().itemVariants[(int)shape];
    }

    private void SetComponents()
    {
        //Get subobject mesh filters, ie. clothes and hair
        headFilter = transform.GetChild(2).GetComponent<MeshFilter>();
        hairFilter = transform.GetChild(3).GetComponent<MeshFilter>();
        topFilter = transform.GetChild(4).GetComponent<MeshFilter>();
        bottomFilter = transform.GetChild(5).GetComponent<MeshFilter>();
        shoeFilter = transform.GetChild(6).GetComponent<MeshFilter>();
        
        //Get the core body mesh
        bodyFilter = GetComponent<MeshFilter>();
        
        Debug.Log("Components set!");
    }
}
