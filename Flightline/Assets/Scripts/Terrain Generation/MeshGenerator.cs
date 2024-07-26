using UnityEngine;
using System.Collections;

public static class MeshGenerator {
	public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail) {

		int skipIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2; // Depending on the LOD depends on how many vertices are skipped for LOD calculations
		int numVertsPerLine = meshSettings.numVerticesPerLine; // Store the number of vertices per line in the mesh, depending on the supportedLODChunkSize index

		Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f; // Calculate half the length and width of the mesh and store this

		MeshData meshData = new MeshData (numVertsPerLine, skipIncrement); // Create a new meshData

		int[,] vertexIndicesMap = new int[numVertsPerLine,numVertsPerLine]; // Create a vertex map of length and width equal to the number of vertices per line + 5 (+5 because needed to merge 2 meshes together)
		int meshVertexIndex = 0;
		int outOfMeshVertexIndex = -1;

		for (int y = 0; y < numVertsPerLine; y++) {
			for (int x = 0; x < numVertsPerLine; x++) { // For every [x,y] in map
				bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1; // Check if its an outOfMeshVertex
				bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x-2)%skipIncrement != 0 || (y-2)%skipIncrement != 0); // Check if its a skipped vertex depending on LOD
				if (isOutOfMeshVertex) {
					vertexIndicesMap [x, y] = outOfMeshVertexIndex; // Set the value in the vertex map to the current value stored in outOfMeshVertex - By doing this each [x,y] has a different negative value
					outOfMeshVertexIndex--;
				} else if (!isSkippedVertex) {
					vertexIndicesMap [x, y] = meshVertexIndex; // Set the value in the vertex map to the current value stored in the meshVertexIndex - By doing this each [x,y] has a different positive value
					meshVertexIndex++;
				}
			}
		}

		for (int y = 0; y < numVertsPerLine; y++) {
			for (int x = 0; x < numVertsPerLine; x++) { // For each [x,y] in map
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0); // Check if its a skipped vertex
				if (!isSkippedVertex) // If not
				{
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1; // Is out of mesh if x or y is 0, OR x or y is 1 less than number of vertices per line - true because we +5 to numVerticesPerLine for this reason
					bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex; // Is edge if x or y is 1 (out of mesh is x or y is 0) or x or y is - 2 verticesPerLine
					bool isMainVertex = (x-2)%skipIncrement == 0 && (y-2)%skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex; // Main vertex if x and y is a factor of skip increment and isnt meshEdge or OutOfMesh
                    bool isEdgeConnectionVertex = (y==2 || y==numVertsPerLine - 3 || x==2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex; // Mesh edge connection if is a corner of the squares created with MainVertex
					
					int vertexIndex = vertexIndicesMap[x, y];
					Vector2 percent = new Vector2(x - 1, y - 1)/(numVertsPerLine - 3); //Work out the percentage of the way from the vertex position along the lines of vertices
					Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize; // Add the central point to this percent multiplied by the meshWorldSize
					float height = heightMap[x, y]; // Find the height at this [x,y]

					if (isEdgeConnectionVertex) // If is a corner of a main vertex...
					{
						bool isVertical = (x == 2 || x == numVertsPerLine - 3); // Check if its a vertical edge connection or horizontal edge connection
						int distanceToMainVertexA = (isVertical?y -2 : x - 2) % skipIncrement; // Check if its a vertical vertex, if so subtract 2 from x and y to make it a corner, then use modulus to find integar distance to another main vertex
						int distanceToMainVertexB = skipIncrement - distanceToMainVertexA; // Subtract distance to the above vertex from the skip increment
						float distancePercentFromAtoB = distanceToMainVertexA / (float)skipIncrement; // Divide A by B to find the percentage

						float heightMainVertexA = heightMap[(isVertical) ? x : x - distanceToMainVertexA, (isVertical) ? y - distanceToMainVertexA : y]; // Use the values to calcultae the height the terrain should be at point A
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + distanceToMainVertexB, (isVertical) ? y + distanceToMainVertexB : y]; // Same for height B

						height = heightMainVertexA * (1-distancePercentFromAtoB) + heightMainVertexB * distancePercentFromAtoB; // Use the percentage calculated to work out what height the terrain should be at the vertex to fill in the seam
                    }

					meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex); // Add this vertex to the OutOfMeshVertices array, or the vertices and uvs arrays

					bool createTriangle = x < numVertsPerLine -1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2)); // Work out if this vertex is used to create a triangle on the mesh
					if (createTriangle)
					{
						int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1; // If not a main vertex on the corner of the main vertex area set to skipIncrement else 1

						int a = vertexIndicesMap[x, y]; // Set a corner of triangle to [x,y]
						int b = vertexIndicesMap[x + currentIncrement, y]; // Set b corner of triangle to [x + increment, y] - Increment added as each mainVertex is currentIncrement apart
						int c = vertexIndicesMap[x, y + currentIncrement]; // Set c corner of triangle to [x, y + increment]
						int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement]; // Set d corner of triangle to [x + increment, y + increment]
						meshData.AddTriangle(a, d, c); // Create 2 triangles from these 4 points
						meshData.AddTriangle(d, a, b);
					}
				}
            }
		}

		meshData.ProcessMesh(); // Bake and calculate normals 

		return meshData;

	}
}

public class MeshData {
	Vector3[] vertices;
	int[] triangles;
	Vector2[] uvs;
	Vector3[] bakedNormals;

	Vector3[] outOfMeshVertices;
	int[] outOfMeshTriangles;

	int triangleIndex;
	int outOfMeshTriangleIndex;

	public MeshData(int numVertsPerLine, int skipIncrement) {

		int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
		int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
		int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
		int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

		vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
		uvs = new Vector2[vertices.Length];

		int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4); // 2 triangles per square, - 4 double counted corners, x4 edges
		int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2; //Calculate squares then double for triangles
		triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

		outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
		outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
	}

	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {
		if (vertexIndex < 0) {
			outOfMeshVertices [-vertexIndex - 1] = vertexPosition; // Add vertices to the correct arrays
		} else {
			vertices [vertexIndex] = vertexPosition;
			uvs [vertexIndex] = uv;
		}
	}

	public void AddTriangle(int a, int b, int c) {
		if (a < 0 || b < 0 || c < 0) { // If out of mesh triangle
			outOfMeshTriangles [outOfMeshTriangleIndex] = a;
			outOfMeshTriangles [outOfMeshTriangleIndex + 1] = b;
			outOfMeshTriangles [outOfMeshTriangleIndex + 2] = c;
			outOfMeshTriangleIndex += 3; // Increment index by 3 as 3 new points added
		} else { // If mesh triangle to be displayed
			triangles [triangleIndex] = a;
			triangles [triangleIndex + 1] = b;
			triangles [triangleIndex + 2] = c;
			triangleIndex += 3;
		}
	}

	Vector3[] CalculateNormals() {

		Vector3[] vertexNormals = new Vector3[vertices.Length]; // Set to number of vertices
		int triangleCount = triangles.Length / 3; // Each triangle has 3 vertices so divide by 3
		for (int i = 0; i < triangleCount; i++) { // For each triangle
			int normalTriangleIndex = i * 3;
			int vertexIndexA = triangles [normalTriangleIndex];
			int vertexIndexB = triangles [normalTriangleIndex + 1];
			int vertexIndexC = triangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC); // Calculate surface normals
			vertexNormals [vertexIndexA] += triangleNormal; // Add the normal to this vertex
			vertexNormals [vertexIndexB] += triangleNormal;
			vertexNormals [vertexIndexC] += triangleNormal;
		}

		int borderTriangleCount = outOfMeshTriangles.Length / 3; // Each triangle has 3 vertices so divide by 3
		for (int i = 0; i < borderTriangleCount; i++) {
			int normalTriangleIndex = i * 3;
			int vertexIndexA = outOfMeshTriangles [normalTriangleIndex];
			int vertexIndexB = outOfMeshTriangles [normalTriangleIndex + 1];
			int vertexIndexC = outOfMeshTriangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC);
			if (vertexIndexA >= 0) { // If this specific vertex is within the mesh to be generated...
				vertexNormals [vertexIndexA] += triangleNormal;
			}
			if (vertexIndexB >= 0) {
				vertexNormals [vertexIndexB] += triangleNormal;
			}
			if (vertexIndexC >= 0) {
				vertexNormals [vertexIndexC] += triangleNormal;
			}
		}


		for (int i = 0; i < vertexNormals.Length; i++) {
			vertexNormals [i].Normalize (); // Normalize the values between 0 and 1
		}

		return vertexNormals;

	}

	Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
		Vector3 pointA = (indexA < 0)?outOfMeshVertices[-indexA-1] : vertices [indexA]; // If out of mesh vertex, set to the negative
		Vector3 pointB = (indexB < 0)?outOfMeshVertices[-indexB-1] : vertices [indexB];
		Vector3 pointC = (indexC < 0)?outOfMeshVertices[-indexC-1] : vertices [indexC];

		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;
		return Vector3.Cross (sideAB, sideAC).normalized; // Calculate cross product and normalize
	}

	public void ProcessMesh()
	{
		BakeNormals();
	}
	private void BakeNormals()
	{
		bakedNormals = CalculateNormals();
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
        mesh.normals = bakedNormals;
		return mesh;
	}

}