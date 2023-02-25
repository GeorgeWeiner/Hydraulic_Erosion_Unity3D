using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Terrain
{
    public static class TerrainSetter
    {
        private static readonly int ErosionShaderMainTextureProperty = Shader.PropertyToID("_ErosionMask");
        private static readonly int DepositionShaderMainTextureProperty = Shader.PropertyToID("_DepositionMask");

        public static TerrainData CreateTerrainData(float[,] heightMap, int heightMapResolution, int terrainHeight)
        {
            var _terrainData = new TerrainData
            {
                heightmapResolution = heightMapResolution,
                size = new Vector3(heightMapResolution, terrainHeight, heightMapResolution)
            };

            _terrainData.SetHeights(0, 0, heightMap);
            return _terrainData;
        }

        public static void ApplyTerrainData(UnityEngine.Terrain terrain, TerrainData terrainData, TerrainCollider terrainCollider)
        {
            terrain.tag = "Terrain";
            terrain.terrainData = terrainData;
            terrain.Flush();

            terrainCollider.terrainData = terrainData;
        }

        public static void ApplyMaterialMasks(UnityEngine.Terrain terrain, float[,] erosionMask, float[,] depositionMask)
        {
            var erosionMaskTex = WriteArrayToTexture(erosionMask, depositionMask);
            var depositionMaskTex = WriteArrayToTexture(depositionMask, depositionMask);
            terrain.materialTemplate.SetTexture(ErosionShaderMainTextureProperty, erosionMaskTex);
            terrain.materialTemplate.SetTexture(DepositionShaderMainTextureProperty, depositionMaskTex);
        }
        
        public static void ApplyMaterialMasks(UnityEngine.Terrain terrain, float[,] erosionMask)
        {
            var erosionMaskTex = WriteArrayToTexture(erosionMask);
            terrain.materialTemplate.SetTexture(ErosionShaderMainTextureProperty, erosionMaskTex);
        }

        private static Texture2D WriteArrayToTexture(float[,] erosionMask, float[,] depositionMask)
        {
            Texture2D tex = new Texture2D(erosionMask.GetLength(0), erosionMask.GetLength(1));

            for (var x = 0; x < erosionMask.GetLength(0); x++)
            {
                for (var y = 0; y < erosionMask.GetLength(1); y++)
                {
                    var colorErosion = erosionMask[x, y] * 20;
                    var colorDeposition = depositionMask[x, y] * 255;
                    tex.SetPixel(y, x, new Color(colorErosion, colorErosion, colorErosion));
                }
            }

            tex.Apply();
            return tex;
        }
        
        private static Texture2D WriteArrayToTexture(float[,] heightmap)
        {
            Texture2D tex = new Texture2D(heightmap.GetLength(0), heightmap.GetLength(1));

            for (var x = 0; x < heightmap.GetLength(0); x++)
            {
                for (var y = 0; y < heightmap.GetLength(1); y++)
                {
                    var colorErosion = heightmap[x, y] * 2;
                    tex.SetPixel(y, x, new Color(colorErosion, colorErosion, colorErosion));
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
