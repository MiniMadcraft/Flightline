using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;

	public Transform viewer;
	public Material mapMaterial;

	public Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	async void Start() {
		await PhysicsCalculations.Instance.UpdateUserChoices();
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDistance = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

		UpdateVisibleChunks ();
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		if (viewerPosition != viewerPositionOld) // If the player has moved
		{
			foreach (TerrainChunk chunk in  visibleTerrainChunks) // For every chunk loaded
			{
				chunk.UpdateCollisionMesh(); // Check if the collider needs to be enabled or disabled
			}
		}

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
	}
		
	void UpdateVisibleChunks() {
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

		for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) { // For all chunks visible in the last frame from the last index to the first...
			alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord); // Record that the co ordinate was updated
			visibleTerrainChunks[i].UpdateTerrainChunk(); // Update their visibility and if they remain in the list
		}
			
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / meshWorldSize); // Calculate the co-ordinate of the current chunk
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / meshWorldSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) // If we haven't already updated the co ordinate
				{
					if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) // If the dictionary contains the co ordinate for this chunk
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk(); // Update the chunk
					}
					else
					{
						TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial); // Create a new terrain chunk

                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk); // Add to dictionary
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged; // Subscribe to the event
						newChunk.Load(); // Load the new chunk
					}
				}
			}
		}
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
	{
		if (isVisible)
		{
			visibleTerrainChunks.Add(chunk);
		}
		else
		{
			visibleTerrainChunks.Remove(chunk);
		}
	}
}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;

    public float sqrVisibleDistanceThreshold
    {
        get
        {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}