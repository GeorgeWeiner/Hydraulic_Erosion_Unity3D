using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var _generator = (TerrainGenerator) target;
        DrawDefaultInspector();
        
        if (GUILayout.Button("Generate Height Map"))
        {
            _generator.Generate();
        }

        if (GUILayout.Button("Scatter Objects"))
        {
            _generator.ScatterObjects();
        }
    }
}
