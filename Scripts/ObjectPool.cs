using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool.
/// Basic example:
/// 
/// [SerializeField] private GameObject enemyPrefab = null;
/// [SerializeField] private GameObject bulletPrefab = null;
/// [SerializeField] private Transform enemyContainer = null;
/// [SerializeField] private Transform weaponContainer = null;
/// void Start()
/// {
/// 	ObjectPool.Instance.AddToPool(enemyPrefab, 10 , enemyContainer);
/// 	ObjectPool.Instance.AddToPool(bulletPrefab, 100, weaponContainer);
/// }
/// 
/// void Shoot()
/// {
/// 	var obj = ObjectPool.Instance.PopFromPool("prefB", false, true, this.transform);
/// }
/// 
/// void Hit(GameObject bullet)
/// {
///     ObjectPool.Instance.PushToPool(ref bulletPrefab, ref bullet, bulletContainer);
/// }
/// </summary>
public sealed class ObjectPool
{
	private Dictionary <string, Queue<GameObject>> container = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabContainer = new Dictionary<string, GameObject>();

	private static ObjectPool instance = null;
	public static ObjectPool Instance {
		get
		{
			if (instance==null)
			{
				instance = new ObjectPool();
			}
			return instance;
		}
	}

    public void Reset() {
        ReleasePool();
        container.Clear();
        container = null;
        instance = null;
    }

	private ObjectPool() {}    

	private Queue<GameObject> FindInContainer(string prefabName)
	{
		if(container.ContainsKey(prefabName) == false)
		{
			container.Add(prefabName, new Queue<GameObject>());
		}
		return container[prefabName];
	}
	
	public void AddToPool(GameObject prefab, int count, Transform parent = null)
	{
		if(prefab == null || count <= 0)
		{
			return;
		}
        string name = prefab.name;
        if (prefabContainer.ContainsKey(name) == false) 
        {
            prefabContainer.Add(name, prefab);
        }
        if (prefabContainer[name] == null) 
        {
            prefabContainer[name] = prefab;
        }
		for (int i = 0; i < count ; i++)
		{
			GameObject obj = PopFromPool(name, true);
            PushToPool(ref obj, true, parent);
		}
	}
    public GameObject PopFromPool(string prefabName, bool forceInstantiate = false, bool instantiateIfNone = false, Transform container = null) 
    {
        if (prefabName == null) { return null; }
        if (prefabContainer.ContainsKey(prefabName) == false) { return null; }
        Debug.Log(container);
        GameObject obj = null;
        if (forceInstantiate == true)
        {
            return CreateObject(prefabName, container); ;
        }
        Queue<GameObject> queue = FindInContainer(prefabName);
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.transform.parent = container;
            obj.SetActive(true);
        }
        if (obj == null && instantiateIfNone == true)
        {
            return CreateObject(prefabName, container);
        }
        return obj;
    }
    private GameObject CreateObject(string prefabName, Transform container)
    {
        GameObject obj = (GameObject)Object.Instantiate(prefabContainer[prefabName]);
        obj.name = prefabName;
        obj.transform.parent = container;
        return obj;
    }
	public GameObject PopFromPool(GameObject prefab, bool forceInstantiate = false)
	{
		if(prefab == null)
		{
			return null;
		}
        return PopFromPool(prefab.name, forceInstantiate);
	}

	public void PushToPool( ref GameObject obj, bool retainObject = true, Transform parent = null)
	{
		if(obj == null)
		{
			return;
		}
        if(retainObject == false)
        {
            Object.Destroy(obj);
            obj = null;
            return;
        }
		if(parent != null)
		{
			obj.transform.parent = parent;
		}
		Queue<GameObject> queue = FindInContainer(obj.name);
        queue.Enqueue(obj);
		obj.SetActive(false);
        obj = null;
	}

	public void ReleaseItems(GameObject prefab, bool destroyObject = false)
	{
		if( prefab == null)
		{
            Debug.Log("null prefab");
			return;
		}
        Debug.Log(prefab.name);
		Queue<GameObject> queue = FindInContainer(prefab.name);
        if (queue == null) 
        {
            return;
        }
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
			while(queue.Count > 0)
			{
                GameObject obj = queue.Dequeue();
				Object.Destroy(obj);
			}
		}
		container = null;
		container = new Dictionary<string, Queue<GameObject>>();
        prefabContainer.Clear();
        prefabContainer = null;
        prefabContainer = new Dictionary<string, GameObject>();
	}
}
