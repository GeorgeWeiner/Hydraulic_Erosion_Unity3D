using System;
using System.Collections;
using System.Collections.Generic;
using Simulation.ObjectPooler;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public List<Pool> pools;

    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
#region Singleton
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;
    }
#endregion

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            //Instantiate as many objects as you wish, and enqueue them.
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.hideFlags = HideFlags.HideInHierarchy;
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }

        StartCoroutine(UpdatePool());
    }

    public GameObject SpawnFromPool(string tag, Transform myTransform)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag" + tag + " doesn't exist");
            return null;
        }
        
        //Get the first object in the pool with the specified tag.
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetPositionAndRotation(myTransform.position, myTransform.rotation);
        objectToSpawn.transform.localScale = myTransform.localScale;
        
        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }
    
    public GameObject SpawnFromPool(GridObject obj)
    {
        if (!poolDictionary.ContainsKey(obj.tag))
        {
            Debug.LogWarning("Pool with tag" + obj.tag + " doesn't exist");
            return null;
        }
        
        //Get the first object in the pool with the specified tag.
        GameObject objectToSpawn = poolDictionary[obj.tag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetPositionAndRotation(obj.transform.position, UnityEngine.Quaternion.Euler(obj.transform.eulerRotation));
        objectToSpawn.transform.localScale = obj.transform.localScale;
        
        poolDictionary[obj.tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }
    
    public IEnumerator UpdatePool()
    {
        for (;;)
        {
            foreach (var obj in PoolingInfo.GridObjects)
            {
                SpawnFromPool(obj);
            }

            yield return null; 
        }
    }
}


