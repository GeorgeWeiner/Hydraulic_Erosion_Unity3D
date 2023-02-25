using System.Diagnostics;
using Terrain;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ObjectScatterer))]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private bool generateSeed;
    [SerializeField] private bool erode;
    [SerializeField] private int seed;
    
    [Header("Size, Scale & Curves")]
    [SerializeField] public int terrainSize;
    [SerializeField] public int terrainHeight;
    [SerializeField] private float perlinScale;
    [SerializeField] private AnimationCurve generationCurve;
    
    [Header("Noise Octaves")]
    [Range(1, 10)]
    [Tooltip("The number of octaves to layer.")]
    [SerializeField] private int numOctaves = 7;
    [Range(0, 1)]
    [Tooltip("Persistence modifies the weight of each new octave. Lower value = less contribution of higher octaves.")]
    [SerializeField] private float persistence = .5f;
    [Range(0, 4)]
    [Tooltip("Lacunarity modifies the scale of the noise octave during each iteration of layering the noise.")]
    [SerializeField] private float lacunarity = 2;
    [Range(0, 1)]
    [SerializeField] private float heightNormalizationFactor = 1;
    
    [SerializeField] ChunkGenerator _chunkGenerator;
    
    private readonly Stopwatch _sw = new();
    private MapDisplay _mapDisplay;
    private float offsetX, offsetZ;

    private HydraulicErosion hydraulicErosion;

    public int CurrentSize { get; set; }
    

    public void Generate()
    {
        InitializeSeed();
        hydraulicErosion = GetComponent<HydraulicErosion>();
        
        float[,] heightMap = GenerateNoiseMap(offsetX, offsetZ);

        UnityEngine.Terrain terrain = FindObjectOfType<UnityEngine.Terrain>();
        TerrainData terrainData = TerrainSetter.CreateTerrainData(heightMap, terrainSize, terrainHeight);
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        
        TerrainSetter.ApplyTerrainData(terrain, terrainData, terrainCollider);
        if (erode)
        {
            TerrainSetter.ApplyMaterialMasks(terrain, hydraulicErosion.erosionMask, hydraulicErosion.depositionMask);
        }
        else
        {
            TerrainSetter.ApplyMaterialMasks(terrain, hydraulicErosion.erosionMask); 
        }
        
        CurrentSize = terrainSize;
    }

    private void InitializeSeed()
    {
        if (!generateSeed)
            Random.InitState(seed);
        
        offsetX = Random.Range(-10000, 10000);
        offsetZ = Random.Range(-10000, 10000);
    }


    public float[,] GenerateNoiseMap(float offsetX, float offsetZ)
    {
        _sw.Start();
        float scale = perlinScale;
        var noiseMap = new float[terrainSize, terrainSize];

        var offsets = new Vector2[numOctaves]; 
        for (var i = 0; i < numOctaves; i++) 
        {
            offsets[i] = new Vector2 (offsetX, offsetZ);
        }
    
        for (var zIndex = 0; zIndex < terrainSize; zIndex++)
        {
            for (var xIndex = 0; xIndex < terrainSize; xIndex++)
            {
                scale = perlinScale;
                var weight = 1f;
                
                //Apply octaves / fractals of noise, aka fractional Browning motion (fBm)
                for (var octave = 0; octave < numOctaves; octave++)
                {
                    Vector2 position = offsets[octave] + new Vector2(zIndex / (float) terrainSize, xIndex / (float) terrainSize) * scale;
                    noiseMap[zIndex, xIndex] += Mathf.PerlinNoise(position.y, position.x) * weight * heightNormalizationFactor;
                
                    weight *= persistence;
                    scale *= lacunarity;
                }
            }
        }

        var heightMap = new float[terrainSize, terrainSize];

        for (var z = 0; z < noiseMap.GetLength(0); z++)
        for (var x = 0; x < noiseMap.GetLength(1); x++)
        {
            heightMap[z, x] = generationCurve.Evaluate(noiseMap[z, x]);
        }

        if (erode)
        {
            heightMap = hydraulicErosion.Erode(heightMap, terrainSize);
        }
        

        _sw.Stop();
        Debug.LogFormat("Generated Noise Map in {0} ms.", _sw.ElapsedMilliseconds);
        _sw.Reset();
        
        return heightMap;
    }

    public void ScatterObjects()
    {
        GetComponent<ObjectScatterer>().ScatterObjects();
    }
}   
    