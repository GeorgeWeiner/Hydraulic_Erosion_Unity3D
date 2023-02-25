using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using Terrain;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class ObjectScatterer : MonoBehaviour
{
	[SerializeField] private PlaceableObject[] placeableObjectGroups;
	[SerializeField, HideInInspector] private List<GameObject> placedObjects = new();
	[SerializeField] private TerrainCollider terrainCollider;
	[SerializeField] private LayerMask layerMask;

	private List<Collider> _targetColliders;
	private TerrainGenerator _generator;
	private ObjectPoolingGrid _poolingGrid;
	private GPUInstancer _gpuInstancer;

	private TerrainData _terrainData;
	private int[,] _detailLayer;

	private List<TreeInstance> _treeInstances = new();
	private List<InstancedMesh> _meshInstances = new();

	[SerializeField] private bool scatter;

	private void Start()
	{
		if (scatter)
			ScatterObjects();
	}

	private void Initialize()
	{
		_terrainData = terrainCollider.terrainData;
		_terrainData.treePrototypes = Array.Empty<TreePrototype>();

		foreach (var placeableObject in placeableObjectGroups)
		{
			if (placeableObject.objectType == ObjectType.Tree)
			{
				var prototypes = new TreePrototype[placeableObject.prefabs.Length];
				for (var i = 0; i < placeableObject.prefabs.Length; i++)
				{
					var prototype = new TreePrototype
					{
						prefab = placeableObject.prefabs[i],
						navMeshLod = 0
					};
					prototypes[i] = prototype;
				}
				var combined = _terrainData.treePrototypes.Concat(prototypes).ToArray();
				_terrainData.treePrototypes = combined;
				_terrainData.RefreshPrototypes();
			}
		}
		
		_poolingGrid = FindObjectOfType<ObjectPoolingGrid>();
		_gpuInstancer = FindObjectOfType<GPUInstancer>();

		_meshInstances = new List<InstancedMesh>();
	}

	//Just place the player on the center of the map.
	private void PlacePlayer(Vector3 min, Vector3 max)
	{
		var player = FindObjectOfType<PlayerController>();
		var centerZ = max.z / 2;
		var centerX = max.x / 2;

		var raycastOrigin = new Vector3(centerX, max.y, centerZ);
		var raycastLength = max.y - min.y;

		if (Physics.Raycast(raycastOrigin, Vector3.down, out var hit, raycastLength,layerMask))
		{
			player.transform.position = hit.point + Vector3.up;
		}
	}


	[ContextMenu("Scatter Objects")]
	public void ScatterObjects()
	{
		RemoveObjects();
		Initialize();

		var bounds = terrainCollider.bounds;
		var min = bounds.min;
		var max = bounds.max;

		PlacePlayer(min, max);
		
		if (terrainCollider == null)
		{
			Debug.LogWarning("No terrain collider found!");
			return;
		}

		if (placeableObjectGroups.Length == 0)
		{
			Debug.LogWarning("No placeable objects set!");
			return;
		}

		foreach (var placeableObject in placeableObjectGroups)
		{
			if (placeableObject.prefabs.Length == 0)
			{
				Debug.LogWarningFormat("{0} has no prefabs! Please check the scriptable object!", placeableObject.name);
				continue;
			}
			if(placeableObject.cellSize <= 0)
			{
				Debug.LogWarningFormat("{0} has a cell size of 0. Cell size needs to be larger than 0!", placeableObject.name); 
				continue;
			}
		
			var noise = GenerateNoiseMap(placeableObject, (int) max.z, (int) max.x, placeableObject.noiseScale, placeableObject.seed);
		
			foreach (var prefab in placeableObject.prefabs)
			{
				for (var posX = min.x; posX <= max.x; posX += placeableObject.cellSize)
				{
					for (var posZ = min.z; posZ <= max.z; posZ += placeableObject.cellSize)
					{
						//Set random offset and scale for each placed object.
						var rScale = Random.Range(placeableObject.minScale, placeableObject.maxScale);
						var offsetX = Random.Range(-placeableObject.randomOffset, placeableObject.randomOffset);
						var offsetZ = Random.Range(-placeableObject.randomOffset, placeableObject.randomOffset);
					
						var pos = new Vector3(posX + offsetX, max.y, posZ + offsetZ);
						var normal = new Vector3();


						//This goes through all the masks for the placeable object, to check if the position is valid.
						if (!Physics.Raycast(new Ray(pos, Vector3.down), out var hit, max.y - min.y)) continue;

						if (!hit.collider.CompareTag(placeableObject.tagMask)) continue;
							
						pos = hit.point - Vector3.up * placeableObject.yOffset;
						normal = hit.normal;
						
						if (placeableObject.MinSlope < hit.normal.y || placeableObject.MaxSlope > hit.normal.y) continue;
							
						if (noise[(int) posX, (int) posZ] <= placeableObject.minNoiseStrength) continue;
						
						var normalizedHeight = Mathf.InverseLerp(min.y, max.y,pos.y);
						if (normalizedHeight <= placeableObject.minHeight || 
						    normalizedHeight >= placeableObject.maxHeight) continue;

						DecideObjectType(placeableObject, pos, prefab, rScale, normal);
					}
				}
			}
		}
		
		SetObjects();
	}

	private void SetObjects()
	{
		_terrainData.SetTreeInstances(_treeInstances.ToArray(), false);
		TerrainSetter.ApplyTerrainData(UnityEngine.Terrain.activeTerrain, _terrainData, terrainCollider);
		
		if (_gpuInstancer != null)
			_gpuInstancer.InitializeInstances(_meshInstances);
	}

	//suboptimal solution, as this breaks the open-closed principle.
	private void DecideObjectType(PlaceableObject placeableObject, Vector3 pos, GameObject prefab, float scale, Vector3 normal)
	{
		switch (placeableObject.objectType)
		{
			case(ObjectType.Prefab):
				PlaceObject(placeableObject, pos, prefab, scale, normal);
				break;
			case(ObjectType.Tree):
				PlaceTreeInstance(placeableObject, pos, prefab, scale);
				break;
			case(ObjectType.GpuInstanced):
				CreateObjectInstance(pos, prefab, scale);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	
	private void PlaceObject(PlaceableObject placeableObject, Vector3 pos, GameObject prefab, float scale, Vector3 normal)
	{
		var rotation = placeableObject.useSurfaceNormal ? normal : Vector3.up;
		rotation = new Vector3(rotation.x, Random.Range(0, 360f), rotation.z);

#if UNITY_EDITOR
		GameObject go;
		if (Application.isPlaying)
		{
			go = Instantiate(prefab, pos, Quaternion.Euler(rotation));
		}
		else
		{
			go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, gameObject.scene) as GameObject;
			go.transform.SetPositionAndRotation(pos, quaternion.Euler(rotation));
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
		}
#else
		GameObject go = Instantiate(prefab, pos, Quaternion.identity);
#endif
		
		go.transform.localScale *= scale;
		go.hideFlags = HideFlags.HideInHierarchy;
		
		if (placeableObject.isStatic)
			go.isStatic = true;

		if (placeableObject.objectCluster)
		{
			foreach (Transform child in go.transform)
			{
				if (child.root == child) continue;
				
				Physics.Raycast(new Ray(child.position + Vector3.up * 10, -child.up), out var hit, 100);
				if (hit.collider != null)
				{
					child.position = hit.point - Vector3.up * placeableObject.yOffset;
					child.rotation = Quaternion.Euler(placeableObject.useSurfaceNormal ? hit.normal : Vector3.up);
				}
			}
		}
		
		placedObjects.Add(go);
	}

	//Add the tree instance to the list, so they can be set in batch after all iterations are finished.
	private void PlaceTreeInstance(PlaceableObject placeableObject, Vector3 pos, GameObject prefab, float scale)
	{
		var tree = new TreeInstance()
		{
			heightScale = scale,
			widthScale = scale,
			position = new Vector3(pos.x / _terrainData.heightmapResolution, pos.y / _terrainData.size.y, pos.z / _terrainData.heightmapResolution),
			prototypeIndex = FindTreePrototypeIndex(prefab),
			rotation = Random.Range(0, 360)
		};
		_treeInstances.Add(tree);
	}

	//Get corresponding index of prototype to prefab.
	private int FindTreePrototypeIndex(GameObject prefab)
	{
		if (_terrainData.treePrototypes.Length == 0)
		{
			Debug.LogError("Tree Prototypes array has 0 elements.");
			return 0;
		}
		
		for (var index = 0; index < _terrainData.treePrototypes.Length; index++)
		{
			var tree = _terrainData.treePrototypes[index];
			if (tree.prefab == prefab)
			{
				return index;
			}
		}
		
		Debug.LogError("Could not find tree prototype index, returning 0");
		return 0;
	}

	//This creates instanced meshes, that get rendered via a direct batch draw call.
	//Currently not really viable, as I need more experience in Compute Shaders and Buffers.
	private void CreateObjectInstance(Vector3 pos, GameObject prefab, float scale)
	{
		var mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
		var materials = prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials;

		var instance = new InstancedMesh(mesh, materials, pos, Vector3.up, Vector3.one * scale);
		_meshInstances.Add(instance);
	}

	//EXPERIMENTAL: Break map up into grid and make small objects a pool, that set themselves around the camera.
	private void StorePoolingInformation(string tag, CustomTransform transform)
	{
		var obj = new GridObject
		{
			tag = tag
		};

		obj.transform.position = transform.position;
		obj.transform.eulerRotation = transform.eulerRotation;
		obj.transform.localScale = transform.localScale;

		var gridX = (int) obj.transform.position.x / _poolingGrid.gridSize;
		var gridZ = (int) obj.transform.position.z / _poolingGrid.gridSize;

		var respectiveGrid = _poolingGrid.grids[gridX, gridZ];
		respectiveGrid.objs.Add(obj);
	}

	private float[,] GenerateNoiseMap(PlaceableObject placeableObject, int zGrid, int xGrid, float scale, int seed)
	{
		_generator = GetComponent<TerrainGenerator>();

		var noiseMap = new float[zGrid, xGrid];
		
		Random.InitState(seed);
		var offset = new Vector2 (Random.Range(-1000, 1000), Random.Range(-1000, 1000));

		for (var zIndex = 0; zIndex < zGrid; zIndex++)
		{
			for (var xIndex = 0; xIndex < xGrid; xIndex++)
			{
				var position = offset + new Vector2(zIndex / (float) _generator.terrainSize, xIndex / (float) _generator.terrainSize) * scale;
				noiseMap[zIndex, xIndex] = Mathf.PerlinNoise(position.y, position.x) * placeableObject.noiseMultiplier;
				
				if (placeableObject.useInverseNoise)
					noiseMap[zIndex, xIndex] = 1 - noiseMap[zIndex, xIndex];
			}
		}
		return noiseMap;
	}

	[ContextMenu("Remove Objects")]
	private void RemoveObjects()
	{
		foreach (var instance in placedObjects.Where(instance => instance != null))
		{
			DestroyImmediate(instance);
		}
		
		placedObjects.Clear();
		_treeInstances.Clear();
	}
}

