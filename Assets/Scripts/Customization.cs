using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Customization : MonoBehaviour
{
    public enum BodyShape { Small, Medium, Large }

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
    [SerializeField] private List<Material> blackListedMaterials = new List<Material>();

    public void Start()
    {
        if (allowStyleChange)
            SetComponents();
        if (allowShapeChange)
            shape = (BodyShape)1;

        CountMaterials();
    }

    public void SetFacialExpression(int id)
    {
        headFilter.sharedMesh = CustomizationItemManager.Instance.expressions[id].sharedMesh;
        currentFaceID = id;
        ReloadOutline();
    }

    public void SetBodyShape(int id)
    {
        if (!allowShapeChange)
            return;
        bodyFilter.sharedMesh = CustomizationItemManager.Instance.bodyShapes[id].sharedMesh;
        shape = (BodyShape)id;
        SetClothesSize();
        ReloadOutline();
    }

    public void SetHair(int id)
    {
        hairFilter.sharedMesh = CustomizationItemManager.Instance.hair[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentHair = id;
        SetClothesSize();
        ReloadOutline();
    }

    public void SetTop(int id)
    {
        topFilter.sharedMesh = CustomizationItemManager.Instance.tops[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentTop = id;
        SetClothesSize();
        RefreshMaterials(meshRenderers[1], topFilter);
        ReloadOutline();
    }

    public void SetBottom(int id)
    {
        bottomFilter.sharedMesh = CustomizationItemManager.Instance.bottoms[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentBottom = id;
        SetClothesSize();
        RefreshMaterials(meshRenderers[2], bottomFilter);
        ReloadOutline();
    }

    public void SetShoe(int id)
    {
        shoeFilter.sharedMesh = CustomizationItemManager.Instance.shoes[id].GetComponent<ClothingItem>().itemVariants[(int)shape];
        currentShoes = id;
        SetClothesSize();
        RefreshMaterials(meshRenderers[3], shoeFilter);
        ReloadOutline();
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
        ReloadOutline();
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

    public void CountMaterials()
    {
        //Clean the list, so we have no overlaps
        materials.Clear();

        //Hack to not count outline materials - this caused me too much pain :.^(
        Outline outlineComp = null;

        if (InteractionManager.Instance.selectedObjects.Count > 0)
        {
            outlineComp = InteractionManager.Instance.selectedObjects[0].GetComponent<Outline>();
            outlineComp.enabled = false;
        }

        switch (allowStyleChange)
        {
            //Count materials only in clothes
            case true:
                {
                    foreach (var sharedMaterial in meshRenderers.SelectMany(meshRenderer => meshRenderer.sharedMaterials))
                    {
                        materials.Add(sharedMaterial);
                    }

                    break;
                }
            //Count materials in mesh
            case false:
                {
                    foreach (var material in gameObject.GetComponent<MeshRenderer>().sharedMaterials)
                    {
                        //Exclude black listed materials here...
                        if (blackListedMaterials.Count <= 0)
                        {
                            materials.Add(material);
                            continue;
                        }

                        foreach (var blackListedMaterial in blackListedMaterials)
                        {
                            if (blackListedMaterial.name != material.name)
                                materials.Add(material);
                        }
                    }
                    break;
                }
        }

        //Reenable outline after count
        if (outlineComp != null) outlineComp.enabled = true;

        // Debug.Log("Counted materials. Count is: " + materials.Count);
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
        meshRenderer.sharedMaterials = tempMats.ToArray();

        //Recount all materials
        CountMaterials();
    }

    private void RefreshColors()
    {
        if (!allowStyleChange) //Color updates for regular miniatures
        {
            List<Material> tempMats = new List<Material>();

            for (int i = 0; i < materials.Count; i++)
            {
                tempMats.Add(materials[i]);
            }

            //Re-append blacklisted material
            if (blackListedMaterials.Count > 0)
            {
                tempMats.Add(blackListedMaterials[0]);
            }

            gameObject.GetComponent<MeshRenderer>().sharedMaterials = tempMats.ToArray();
        }
        else //Color updates for style avatars
        {
            //Disable the outline
            Outline outlineComp = gameObject.GetComponent<Outline>();
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
    public void RefreshUI()
    {
        CustomizationItemManager.Instance.uiManager.RefreshUI();
    }

    private void ReloadOutline()
    {
        Outline[] outlines = gameObject.GetComponentsInChildren<Outline>();

        foreach (var outline in outlines)
        {
            outline.RefreshOutline();
        }
    }
    public void LoadCustomization(CustomizationData data)
    {
        InteractionManager.Instance.AddIndicator(gameObject);

        if (allowStyleChange)
            SetComponents();
        if (allowShapeChange)
            shape = (BodyShape)1;

        CountMaterials();

        if (allowStyleChange)
        {
            SetFacialExpression(data.currentFaceID);
            SetHair(data.currentHair);
            SetTop(data.currentTop);
            SetBottom(data.currentBottom);
            SetShoe(data.currentShoes);
            // Debug.Log("faceID: " + data.currentFaceID + " hairID: " + data.currentHair + " topID: " + data.currentTop + " bottomID: " + data.currentBottom + " shoeID: " + data.currentShoes);
        }
        if (allowShapeChange)
            SetBodyShape(data.shape);

        materials = new List<Material>(data.materials);
        // Debug.Log("Loading customization");
        // Debug.Log("materials: ");
        // PrintMaterials();
        RefreshColors();
        RefreshUI();
    }
    public CustomizationData SaveCustomization()
    {
        CustomizationData data = new CustomizationData();
        if (allowStyleChange)
        {
            data.currentFaceID = currentFaceID;
            data.currentHair = currentHair;
            data.currentTop = currentTop;
            data.currentBottom = currentBottom;
            data.currentShoes = currentShoes;
            // Debug.Log("faceID: " + currentFaceID + " hairID: " + currentHair + " topID: " + currentTop + " bottomID: " + currentBottom + " shoeID: " + currentShoes);
        }
        if (allowShapeChange)
            data.shape = (int)shape;

        data.materials = new List<Material>(materials);
        // Debug.Log("Saving customization");
        // Debug.Log("materials: ");
        // PrintMaterials();
        return data;
    }
    // public void PrintMaterials()
    // {
    //     foreach (var material in materials)
    //     {
    //         Debug.Log(material.name);
    //     }
    // }
}
