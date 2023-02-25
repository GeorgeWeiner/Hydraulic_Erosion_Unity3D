using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    //This was an experiment in infinite generation of the map.
    //This is not viable, as it takes too much time to scatter the objects.
    //Another example of where compute shaders would be necessary.
    public class ChunkGenerator : MonoBehaviour
    {
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private UnityEngine.Terrain startingTerrain;
        [SerializeField] private Material material;

        private Chunk _currentChunk;
        private List<Chunk> activeChunks;
        private TerrainGenerator _generator;

        private Transform camera;
        private void Awake()
        {
            camera = Camera.main.transform;
            _generator = FindObjectOfType<TerrainGenerator>();

            activeChunks = new List<Chunk>();
            
            _currentChunk.offsetZ = 0;
            _currentChunk.offsetX = 0;
            _currentChunk.terrain = startingTerrain;
            
            GenerateSurroundingChunks();
        }

        private void Update()
        {
            FindCurrentChunk();
        }

        private void FindCurrentChunk()
        {
            if (Physics.Raycast(new Ray(camera.position, -Vector3.up), out RaycastHit hit, float.MaxValue, terrainLayer))
            {
                if (hit.collider.TryGetComponent(out UnityEngine.Terrain terrain))
                {
                    if (_currentChunk.terrain != terrain)
                    {
                        print("Found new chunk");
                        GenerateSurroundingChunks();
                        _currentChunk.terrain = terrain;
                    }
                }
            }
        }

        public void GenerateSurroundingChunks()
        {
            Vector3 currentOffset = _currentChunk.terrain.transform.position;
            int offsetAdd = _generator.terrainSize / 2;
            
            Chunk north = new Chunk(currentOffset.x, currentOffset.z + offsetAdd, new UnityEngine.Terrain());
            Chunk south = new Chunk(currentOffset.x, currentOffset.z - offsetAdd, new UnityEngine.Terrain());
            Chunk east  = new Chunk(currentOffset.x + offsetAdd, currentOffset.z, new UnityEngine.Terrain());
            Chunk west  = new Chunk(currentOffset.x - offsetAdd, currentOffset.z, new UnityEngine.Terrain());

            Chunk[] surroundingChunks = new[]
            { north, south, east, west };

            foreach (var chunk in surroundingChunks)
            {
                float[,] heightMap = _generator.GenerateNoiseMap(chunk.offsetX, chunk.offsetZ);
                TerrainData data = TerrainSetter.CreateTerrainData(heightMap, _generator.terrainSize, _generator.terrainHeight);
                GameObject terrain = UnityEngine.Terrain.CreateTerrainGameObject(data);
                terrain.transform.position = new Vector3(chunk.offsetX, 0f, chunk.offsetZ);
                terrain.layer = LayerMask.NameToLayer("Terrain");
                terrain.GetComponent<UnityEngine.Terrain>().materialTemplate = material;
            }

        }
    }

    public struct Chunk
    {
        public float offsetX;
        public float offsetZ;
        public UnityEngine.Terrain terrain;

        public Chunk(float offsetX, float offsetZ, UnityEngine.Terrain terrain)
        {
            this.offsetX = offsetX;
            this.offsetZ = offsetZ;
            this.terrain = terrain;
        }
    }
}