using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGraph;

public class MaterialController : MonoBehaviour
{
    //Declarations
    [Header("Settings")]
    [SerializeField] private List<MeshRenderer> _allMeshes = new();
    [SerializeField] private Shader _blinkableShader;

    [Header("Color Sets")]
    [SerializeField] private List<MeshRenderer> _primaryColorMeshes = new();
    [SerializeField] private List<MeshRenderer> _secondaryColorMeshes = new();
    [SerializeField] private List<MeshRenderer> _tertiaryColorMeshes = new();




    //Monobehaviour




    //Internals
    private void ApplyColorToBlinkableMaterials(List<MeshRenderer> _meshCollection, Color baseColor, Color blinkColor)
    {
        foreach (MeshRenderer renderer in _meshCollection)
        {
            Material material = renderer.material;

            if (material.shader == _blinkableShader)
            {
                material.SetColor("_BaseColor", baseColor);
                material.SetColor("_BlinkColor", blinkColor);
            }
        }
    }



    //Externals
    public void SetColors(Color primary, Color secondary, Color tertiary, Color blinkColor)
    {
        ApplyColorToBlinkableMaterials(_primaryColorMeshes, primary, blinkColor);
        ApplyColorToBlinkableMaterials (_secondaryColorMeshes, secondary, blinkColor);
        ApplyColorToBlinkableMaterials(_tertiaryColorMeshes, tertiary, blinkColor);
    }

    public void SetColors(ColorScheme colorScheme)
    {
        SetColors(colorScheme.Primary(),colorScheme.Secondary(),colorScheme.Tertiary(), colorScheme.BlinkColor());
    }

    public void ToggleBlinkingVisual(bool newState)
    {
        foreach (MeshRenderer renderer in _allMeshes)
        {
            Material material = renderer.material;

            if (material.shader == _blinkableShader)
            {
                if (newState)
                    material.SetFloat("_IsBlinkToggled", 1);
                else
                    material.SetFloat("_IsBlinkToggled", 0);
            }
        }
        
    }





}
