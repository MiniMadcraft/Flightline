using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 10f;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0, numSupportedLODs - 1)]
    public int editorPreviewLOD;

    // Number of vertices per line of mesh rendered at LOD = 0. Includes 2 extra vertices that are excluded from final mesh but used for calculating normals
    public int numVerticesPerLine
    {
        get
        {
            return supportedChunkSizes[chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize
    {
        get
        {
            return (numVerticesPerLine - 3) * meshScale;
        }
    }
}
