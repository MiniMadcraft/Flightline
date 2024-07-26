using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

    public enum DrawMode { NoiseMap, Mesh };
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;

    public Material terrainMaterial;



    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;

    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial); // Apply texture

        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight); // Update height of terrain with new min and max heights

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, Vector2.zero); // Generate height map

        if (drawMode == DrawMode.NoiseMap) // Enum
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap)); // Draw Texture from height map
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, meshSettings.editorPreviewLOD)); // Draw mesh
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture; // Set texture to the texture
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f; // Scale the texture

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh(); // Generate a new mesh
        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying) // If not in the playing mode, i.e in the editor
        {
            DrawMapInEditor(); // Draw the new map
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial); // Apply the updated texture to the material
    }

    void OnValidate()
    {

        if (meshSettings != null) // If present
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated; // Unsubscribe
            meshSettings.OnValuesUpdated += OnValuesUpdated; // Resubscribe
        }
        if (heightMapSettings != null) // If present
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated; // Unsubscribe
            heightMapSettings.OnValuesUpdated += OnValuesUpdated; // Resubscribe
        }
        if (textureData != null) // If present
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated; // Unsubscribe
            textureData.OnValuesUpdated += OnTextureValuesUpdated; // Resubscribe
        }
    }
}
