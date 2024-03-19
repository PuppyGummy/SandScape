using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Customization : MonoBehaviour
{
    public enum BodyShape {Small, Medium, Large}

    public BodyShape shape = BodyShape.Medium;
    public int currentFaceID = 1;
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
    
    [SerializeField] private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    public void Start()
    {
        SetComponents();
        shape = (BodyShape)1;
        CountMaterials();
    }

    public void SetFacialExpression(int id)
    {
        headFilter.sharedMesh = CustomizationItemManager.Instance.expressions[id].sharedMesh;
        currentFaceID = id;
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
        RefreshMaterials(meshRenderers[1], topFilter);
    }
    
    public void SetBottom(int id)
    {
        bottomFilter.sharedMesh = CustomizationItemManager.Instance.bottoms[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentBottom = id;
        SetClothesSize();
        RefreshMaterials(meshRenderers[2], bottomFilter);
    }
    
    public void SetShoe(int id)
    {
        shoeFilter.sharedMesh = CustomizationItemManager.Instance.shoes[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentShoes = id;
        SetClothesSize();
        RefreshMaterials(meshRenderers[3], shoeFilter);
    }

    public void SetColor(int id, Material newColor)
    {
        materials[id] = newColor;
        Debug.Log("Set material");
        
        RefreshColors();
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

    private void CountMaterials()
    {
        //Clean the list, so we have no overlaps
        materials.Clear();
        
        //Hack to not count outline materials - this caused me too much pain :.^(
        Outline outlineComp = InteractionManager.Instance.selectedObjects[0].GetComponent<Outline>();
        outlineComp.enabled = false;

        //Count materials
        foreach (var sharedMaterial in meshRenderers.SelectMany(meshRenderer => meshRenderer.sharedMaterials))
        {
            materials.Add(sharedMaterial);
        }
        
        //Reenable outline after count
        outlineComp.enabled = true;
    }

    private void RefreshMaterials(MeshRenderer meshRenderer, MeshFilter meshFilter)
    {
        //Copy and save the core color
        Material[] newMats = new Material[1];
        newMats[0] = meshRenderer.sharedMaterials[0];

        //Apply only the core color as color 1, remove all other colors
        meshRenderer.sharedMaterials = newMats;
        
        //Create temp list used for appending colors to the list
        List<Material> tempMats = new List<Material>();
        
        //For each submesh, copy the material and add it for transfer
        for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
        {
            //Copy material 0 and add it to the list
            tempMats.Add(meshRenderer.sharedMaterials[0]);
        }
        //Apply the new materials
        meshRenderer.sharedMaterials =  tempMats.ToArray();
        
        //Recount all materials
        CountMaterials();
    }

    private void RefreshColors()
    {
        //Disable the outline
        Outline outlineComp = InteractionManager.Instance.selectedObjects[0].GetComponent<Outline>();
        outlineComp.enabled = false;
        
        //Match colors to the list, using the submeshes as reference
        
        //Hair
        meshRenderers[0].sharedMaterials = new[] { materials[0] };
        
        //Top
        int topsMatCount = 1 + topFilter.sharedMesh.subMeshCount;
        List<Material> tempTopsMats = new List<Material>();
        
        Debug.Log("Top count = " + topsMatCount);
        
        for (int i = 1; i < topsMatCount; i++)
        {
            tempTopsMats.Add(materials[i]);
        }
        
        meshRenderers[1].sharedMaterials = tempTopsMats.ToArray();
        
        //Bottom
        int bottomsMatCount = topsMatCount + bottomFilter.sharedMesh.subMeshCount;
        List<Material> tempBottomMats = new List<Material>();
        
        Debug.Log("Bottom count = " + bottomsMatCount);
        
        for (int i = topsMatCount; i < bottomsMatCount; i++)
        {
            tempBottomMats.Add(materials[i]);
        }
        
        meshRenderers[2].sharedMaterials = tempBottomMats.ToArray();
        
        //Shoes
        int shoesMatCount = bottomsMatCount + shoeFilter.sharedMesh.subMeshCount;
        List<Material> tempShoeMats = new List<Material>();
        
        Debug.Log("Shoe count = " + shoesMatCount);
        
        for (int i = bottomsMatCount; i < shoesMatCount; i++)
        {
            tempShoeMats.Add(materials[i]);
        }
        
        meshRenderers[3].sharedMaterials = tempShoeMats.ToArray();

        outlineComp.enabled = true;
    }
}
