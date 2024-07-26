using UnityEngine;
using System.Collections;
using System;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre) {
		float[,] noiseMap = new float[mapWidth,mapHeight]; // Create a noise map with width and height equal to given width and height

		System.Random prng = new System.Random (settings.seed); // Create a variable that is randomised based on the seed input
		Vector2[] octaveOffsets = new Vector2[settings.octaves]; // Octaves discussed in design

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < settings.octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + settings.offset.x + sampleCentre.x; // Get random number between given values and add to offset in x direction and the x central point
			float offsetY = prng.Next (-100000, 100000) - settings.offset.y - sampleCentre.y; // Same for y
			octaveOffsets [i] = new Vector2 (offsetX, offsetY); // Set the offset of the octave to these values

			maxPossibleHeight += amplitude; // Set max possible height to add the amplitude
			amplitude *= settings.persistance; // Multiply by the persistence value
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f; // Find central point


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) { // For each [x,y]

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < settings.octaves; i++) { // For each octave
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1; // Calculate noise value
					noiseHeight += perlinValue * amplitude; // Set height to this noise value multiplied by the amplitude given

					amplitude *= settings.persistance;
					frequency *= settings.lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) { // Update max and min noise heights as they may be < 0 or > 1
					maxLocalNoiseHeight = noiseHeight;
				}
				if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}

				if (sampleCentre.x == 0 && sampleCentre.y == 0)
				{

                    if (x < 10 | x > mapWidth - 10 | y < 10 | y > mapHeight - 10)
                    {
                        noiseMap[x, y] = noiseHeight;
                    }
                    else if (x < 20 | x > mapWidth - 20 | y < 20 | y > mapHeight - 20)
					{
						noiseMap[x, y] = noiseHeight;
					}
					else
					{
                        noiseMap[x, y] = (noiseHeight - 0.3f) / 6; // Set [x,y] to the noiseHeight value
                    }
                }
				if (sampleCentre.x != 0 || sampleCentre.y != 0)
				{
                    noiseMap[x, y] = noiseHeight; // Set [x,y] to the noiseHeight value
                }

                if (settings.normalizeMode == NormalizeMode.Global) // If in relation to other meshes
				{
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f); // Normalize the height in relation to other meshes
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue); // Clamp value between 0 and the max height value
                }
            }
		}

        if (settings.normalizeMode == NormalizeMode.Local) // If only in relation to itself
        {
			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]); // Calculate the height in relation to the min and max as a decimal
				}
			}
		}

		return noiseMap;
	}

}

[Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;

    public float scale = 50;

    public int octaves = 6;
    [Range(0.2f, 0.5f)]
    public float persistance = 0.4f;
	[Range(1.8f, 2.5f)]
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

	public void ValidateValues()
	{
		scale = Mathf.Max(scale, 0.01f);
		octaves = Mathf.Max(octaves, 1);
	}
}
