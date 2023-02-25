using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class GPUInstancer : MonoBehaviour
    {
        private List<List<InstancedMesh>> Batches;

        /// <summary>
        /// Creates Batches of Mesh Instances, that get sent over to the GPU.
        /// Each Batch can contain a max of 1000 elements, each with the same mesh and materials.
        /// If another mesh is used, a new batch gets initialized.
        /// </summary>
        /// <param name="instances"></param>
        public void InitializeInstances(List<InstancedMesh> instances)
        {
            Batches = new List<List<InstancedMesh>> {new()};
            var lastMesh = instances[0].mesh;
            var addedInstances = 0;
            
            for (var i = 0; i < instances.Count; i++)
            {
                //If the batch has less than 1000 and the mesh is the same.
                if (addedInstances < 1000 && instances[i].mesh == lastMesh)
                {
                    Batches[^1].Add(instances[i]);
                    addedInstances += 1;
                    print("Added new instance to batch.");
                }
                else
                {
                    Batches.Add(new List<InstancedMesh>());
                    lastMesh = instances[i].mesh;
                    addedInstances = 0;
                    print("Initialized new batch.");
                }
            }
        }
        
        private void Update()
        {
            if (Batches.Count == 0) return;
            RenderBatches();
        }
        
        /// <summary>
        /// Render all the batches each frame.
        /// This is a brute force method, which at its current state is not viable for terrains of large sizes.
        /// To optimize this I have to learn about Compute Buffers.
        /// </summary>
        private void RenderBatches()
        {
            foreach (List<InstancedMesh> batch in Batches)
            {
                var mesh = batch[0].mesh;
                Material[] materials = batch[0].materials;
                var matrices = new Matrix4x4[batch.Count];

                for (var index = 0; index < batch.Count; index++)
                {
                    matrices[index] = Matrix4x4.TRS(batch[index].position, Quaternion.Euler(batch[index].rotation), batch[index].scale);
                }

                for (var i = 0; i < mesh.subMeshCount; i++)
                {
                    Graphics.DrawMeshInstanced(mesh, i, materials[i], matrices);
                }
            }
        }
    }
    
    
}
