using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 5f;

    public event System.Action<TerrainChunk, bool> onVisibilityChanged;

    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale; // Sample noise from this position
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) // For each detail level
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod); // Create an LODMesh at the index [i], passing through the LOD
            lodMeshes[i].updateCallback += UpdateTerrainChunk; // Subscribe to the UpdateTerrainChunk event
            if (i == colliderLODIndex) // If i is equal to the colliderLODIndex passed through
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh; // Subscribe to the UpdateCollisionMesh event
            }
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }
    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z); // Get the viewer's position in the 2d plane
        }
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); // Calculate distance from viewer to nearest edge of chunk

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDistance; // If viewer distance is <= max distance to view the chunk, set true

            if (visible)
            { // If the chunk is visible this frame...
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1; // Increase LOD index until correct for viewer distnace
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                { // If the LOD has changed...
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex; // Set previous LOD index to current LOD index
                        meshFilter.mesh = lodMesh.mesh; // Update the mesh
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    { // If it hasn't requested the mesh yet...
                        lodMesh.RequestMesh(heightMap, meshSettings); // Request the mesh
                    }
                }
            }

            if (wasVisible != visible) // If the chunk has changed visiblity state
            {
                SetVisible(visible); //Set the visiblity to true or false
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold) // If the user's distance to edge of terrain chunk is less than the threshold for a change in LOD
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh) // if it hasn't requested a new mesh yet for the new lod
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings); // immediately get it to so that the collider is also updated
                }
            }
            if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) // If viewer distance to chunk edge is less than the threshold
            {
                if (lodMeshes[colliderLODIndex].hasMesh) // If the lodMesh at the collider index does have a mesh
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh; // Set the collider's mesh to this mesh
                    hasSetCollider = true; // Set the collider to true
                }
            }
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible); // Set mesh visible / not visible
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf; // Return true or false if its currently visible or not
    }

}

class LODMesh
{

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }

}
