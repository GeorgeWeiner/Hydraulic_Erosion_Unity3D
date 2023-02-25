using UnityEngine;

namespace Terrain
{
    public enum ObjectType
    {
        Prefab,
        Tree,
        GpuInstanced
    }
    
    [CreateAssetMenu(fileName = "New Placeable Object", menuName = "Placeable Objects")]
    public class PlaceableObject : ScriptableObject
    {
        [Header("Prefab")] [Tooltip("What kind of object is it? Prefab gets placed as Game Object. Tree gets placed as a Terrain Tree Instance.")]
        public ObjectType objectType;
        [Tooltip("List of prefabs to use for this group.")]
        public GameObject[] prefabs;
        public float minScale = .1f, maxScale = 1f;
        [Tooltip("Is the prefab static?")]
        public bool isStatic = true;
        [Header("Placement")] [Tooltip("The seed used for the Random offset. Same seed = same result.")]
        public int seed;
        [Range(.1f, 500f)] [Tooltip("How large is the gap within the grid of placement.")]
        public float cellSize = 10f;
        [Range(0f, 100f)] [Tooltip("How much offset to apply in x and z direction when placing along the grid")]
        public float randomOffset = 20f;
        [Tooltip("How much should the object be pushed into the ground?")]
        public float yOffset;
        [Tooltip("Does the object get rotated along the surface normal or the global up Vector?")]
        public bool useSurfaceNormal;
        [Tooltip("Does the object have child objects, that should get placed separately?")]
        public bool objectCluster;
        [Range(0f, 90f)] [SerializeField] [Tooltip("The slope in angles for the minimum and maximum slope of placement.")]
        private float minSlope, maxSlope = 1f;
        [Range(0f, 1f)] [Tooltip("Normalized Height of Terrain. 0 = min.y / 1 = max.y.")]
        public float minHeight, maxHeight = 1f;
        [Tooltip("What tag should the GameObject have, that the Scatterer uses.")]
        public string tagMask = "Terrain";
        [Range(1, 15)] [Tooltip("The base scale of the noise.")]
        public float noiseScale = 6f;
        
        [Range(0, 1)] [Tooltip("The min value of the noise for a placement of the prefab.")]
        public float minNoiseStrength = .5f;
        [Tooltip("Multiplies each value of the noise, to get more defined edges.")]
        public float noiseMultiplier = 1f;

        [Header("Experimental")] [Tooltip("Returns inverted noise of a seed. Useful when trying to place certain objects where others aren't.")]
        public bool useInverseNoise;

        public float MinSlope => 1 - minSlope / 90;
        public float MaxSlope => 1 - maxSlope / 90;
    }
}