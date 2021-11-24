using System.Collections.Generic;
using NoiseTest;
using UnityEngine;
using Random = System.Random;

public class WorldSettings
{
    private static Random randomness = new Random();
    private static readonly int seed = randomness.Next(int.MinValue, int.MaxValue);
    public static Dictionary<Vector3, Chunk> loadedChunks = new Dictionary<Vector3, Chunk>();

    public static OpenSimplexNoise noise = new OpenSimplexNoise(seed);
    
    public const int chunkWidth = 32;
    public const int chunkHeight = 128;
}
