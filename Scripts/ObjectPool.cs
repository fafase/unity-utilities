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
/// 	GameObject bullet = ObjectPool.Instance.PopFromPool(bulletPrefab);
/// }
/// 
/// void Hit(GameObject bullet)
/// {
///     ObjectPool.Instance.PushToPool(ref bulletPrefab, ref bullet, bulletContainer);
/// }
/// </summary>
public sealed class ObjectPool
{
	private Dictionary <GameObject, List<GameObject>> container = new Dictionary<GameObject, List<GameObject>>();

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
        instance = null;
    }

	private ObjectPool() {}    

	private List<GameObject> FindInContainer(ref GameObject prefab)
	{
		if(container.ContainsKey(prefab) == false)
		{
			container.Add(prefab, new List<GameObject>());
		}
		return container[prefab];
	}
	
	public void AddToPool(GameObject prefab, int count, Transform parent = null)
	{
		if(prefab == null || count <= 0)
		{
			return;
		}
		for (int i = 0; i < count ; i++)
		{
			GameObject obj = PopFromPool(prefab, true);
			PushToPool(ref prefab,ref obj, parent);
		}
	}

	public GameObject PopFromPool(GameObject prefab, bool forceInstantiate = false)
	{
		if(prefab == null)
		{
			return null;
		} 
		GameObject o = null;
		if( forceInstantiate == true)
		{
			o = (GameObject)Object.Instantiate(prefab);
			return o;
		}
		List<GameObject> list = FindInContainer(ref prefab);
		if(list.Count > 0)
		{
			o = list[0];
			list.RemoveAt(0);
			o.SetActive(true);
		}
		return o;
	}

	public void PushToPool(ref GameObject prefab, ref GameObject obj, Transform parent = null)
	{
		if(prefab == null||obj == null)
		{
			return;
		}
		if(parent != null)
		{
			obj.transform.parent = parent;
		}
		List<GameObject> list = FindInContainer(ref prefab);
		list.Add (obj);
		obj.SetActive(false);
	}

	public void ReleaseItems(ref GameObject prefab, ref GameObject obj)
	{
		if(prefab == null || obj == null)
		{
			return;
		}
		var list = FindInContainer(ref prefab);
		int index = list.IndexOf(obj);
		list.RemoveAt (index);
		Object.Destroy(obj);
	}

	public void ReleasePool()
	{
		foreach (var kvp in container)
		{
			var list = kvp.Value;
			foreach(var obj in list)
			{
				Object.Destroy(obj);
			}
		}
		container = null;
		container = new Dictionary<GameObject, List<GameObject>>();
	}
}
