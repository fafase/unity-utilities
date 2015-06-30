	public GameObject prefab;
	public GameObject prefabB;
	public Transform tr, trB;
	private ObjectPool objectPool = null;
	void Start () 
	{
		objectPool = ObjectPool.Instance;
		objectPool.AddToPool(prefab,20,tr);	
		objectPool.AddToPool(prefabB,5,trB);	
	}

	List<GameObject>list = new List<GameObject>();
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			GameObject obj = objectPool.PopFromPool(prefab);	
			if(obj != null)
			{
				obj.name = "Found"; 
				list.Add (obj);
			}
		}
		if(Input.GetKeyDown(KeyCode.A))
		{
			if(list.Count > 0)
			{
				int index = Random.Range(0, list.Count - 1);
				GameObject obj = list[index];
				list.RemoveAt(index);
				objectPool.PushToPool(prefab,obj,tr);
			}
		}
	}
