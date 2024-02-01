using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterCustomizationManager : MonoBehaviour
{
    [Header("Hair Options")] public List<Mesh> HairMeshes;
    [Header("Hair Colors")] public List<Material> HairColors;
    [Header("Body Colors")] public List<Material> BodyColors;

    [Header("Meshes")] 
    public MeshRenderer HairMeshRenderer;
    public MeshFilter hairMesh;
    public MeshRenderer BodyMesh;

    private int hairID = 0;
    private int hairColorID = 0;
    private int bodyColorID = 0;

    public void ChangeHairModel()
    {
        hairID++;

        if (hairID > HairMeshes.Count - 1)
            hairID = 0;

        hairMesh.mesh = HairMeshes[hairID];
    }
    
    public void ChangeHairColor()
    {
        hairColorID++;

        if (hairColorID > HairColors.Count - 1)
            hairColorID = 0;

        var newHairMaterial = HairMeshRenderer.materials.ToList();
        newHairMaterial[0] = HairColors[hairColorID];
        
        HairMeshRenderer.SetMaterials(newHairMaterial);
    }
    
    public void ChangeBodyColor()
    {
        bodyColorID++;

        if (bodyColorID > BodyColors.Count - 1)
            bodyColorID = 0;

        var newBodyMaterial = BodyMesh.materials.ToList();
        newBodyMaterial[0] = BodyColors[bodyColorID];
        
        BodyMesh.SetMaterials(newBodyMaterial);   
    }
}
