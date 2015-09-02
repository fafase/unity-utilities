using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed class ObjectPool
{
    private Dictionary<GameObject, Queue<GameObject>> container = new Dictionary<GameObject, Queue<GameObject>>();
	
    private static ObjectPool instance = null;
    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ObjectPool();
            }
            return instance;
        }
    }
    public void Reset()
    {
        instance = null;
    }
    private ObjectPool() { }
	
    public bool AddToPool(GameObject prefab, int count, Transform parent = null) 
    {
        if (prefab == null || count <= 0) { return false; }
        string name = prefab.name;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = PopFromPool(prefab, true);
            PushToPool(ref obj, true, parent);
        }
        return true;
    }
	
    public GameObject PopFromPool(GameObject prefab, bool forceInstantiate = false, bool instantiateIfNone = false, Transform container = null)
    {
        if (forceInstantiate == true) { return CreateObject(prefab, container); }
        GameObject obj = null;
        Queue<GameObject> queue = FindInContainer(prefab);
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.transform.parent = container;
            obj.SetActive(true);
            IPoolObject poolObject = obj.GetComponent();
            poolObject.Init();
        }
        if (obj == null && instantiateIfNone == true)
        {
            return CreateObject(prefab, container);
        }
        return obj;
    }
    private Queue<GameObject> FindInContainer(GameObject prefab)
    {
        if (container.ContainsKey(prefab) == false)
        {
            container.Add(prefab, new Queue<GameObject>());
        }
        return container[prefab];
    }
    private GameObject CreateObject(GameObject prefab, Transform container)
    {
        IPoolObject poolObjectPrefab = prefab.GetComponent();
        if(poolObjectPrefab== null){Debug.Log ("Wrong type of object"); return null;}

        GameObject obj = (GameObject)Object.Instantiate(prefab);
        IPoolObject poolObject = obj.GetComponent();
        obj.name = prefab.name;
        poolObject.Prefab = prefab;
        obj.transform.parent = container;

        return obj;
    }
	
    public void PushToPool(ref GameObject obj, bool retainObject = true, Transform parent = null)
    {
        if (obj == null) { return; }
        if (retainObject == false)
        {
            Object.Destroy(obj);
            obj = null;
            return;
        }
        if (parent != null)
        {
            obj.transform.parent = parent;
        }
        IPoolObject poolObject = obj.GetComponent();
        GameObject prefab = null;
        if(poolObject != null)
        {
            prefab = poolObject.Prefab;
            Queue<GameObject> queue = FindInContainer(prefab);
            queue.Enqueue(obj);
            obj.SetActive(false);
         }
         obj = null;
    }
    public void ReleaseItems(GameObject prefab, bool destroyObject = false)
    {
        if (prefab == null) { return; }
        Queue<GameObject> queue = FindInContainer(prefab);
        if (queue == null) { return; }
        while (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            if (destroyObject == true)
            {
                Object.Destroy(obj);
            }
        }
    }
    public void ReleasePool() 
    {
        foreach (var kvp in container)
        {
            Queue<GameObject> queue = kvp.Value;
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                Object.Destroy(obj);
            }
         }
         container = null;
         container = new Dictionary<GameObject, Queue<GameObject>>();
    }
}
public interface IPoolObject
{
    GameObject Prefab{get;set;}
    void Init();
}
