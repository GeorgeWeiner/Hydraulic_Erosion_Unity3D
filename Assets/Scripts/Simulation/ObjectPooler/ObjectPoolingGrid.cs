using System.Collections.Generic;
using Simulation.ObjectPooler;
using UnityEngine;

public class ObjectPoolingGrid : MonoBehaviour
{
    public int gridSize;
    private TerrainGenerator _generator;
    public Grid[,] grids;

    private Grid currentGrid;
    private Transform player;
    private ObjectPooler _objectPooler;

    private void Awake()
    {
        _generator = FindObjectOfType<TerrainGenerator>();

        int gridArraySize = _generator.terrainSize / gridSize;
        grids = new Grid[gridArraySize, gridArraySize];
        player = Camera.main.transform;
    }
    
    private void Update()
    {
        int gridX = (int) player.position.x / gridSize;
        int gridZ = (int) player.position.z / gridSize;

        currentGrid = grids[gridX, gridZ];
        PoolingInfo.GridObjects = currentGrid.objs;
    }
}

//Get this by calculating current position / grid size
public struct Grid
{
    public HashSet<GridObject> objs;

    public Grid(HashSet<GridObject> objs)
    {
        this.objs = objs;
    }
}

public struct GridObject
{
    public string tag;
    public CustomTransform transform;

    public GridObject(string tag, CustomTransform transform)
    {
        this.tag = tag;
        this.transform = transform;
    }
}

public struct CustomTransform
{
    public Vector3 position;
    public Vector3 eulerRotation;
    public Vector3 localScale;

    public CustomTransform(Vector3 position, Vector3 eulerRotation, Vector3 localScale)
    {
        this.position = position;
        this.eulerRotation = eulerRotation;
        this.localScale = localScale;
    }
}