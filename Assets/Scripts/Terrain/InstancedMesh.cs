using UnityEngine;

namespace Terrain
{
    public class InstancedMesh
    {
        public readonly Mesh mesh;
        public readonly Material[] materials;

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public InstancedMesh(Mesh mesh, Material[] materials, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.mesh = mesh;
            this.materials = materials;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}