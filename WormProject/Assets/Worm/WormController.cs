using UnityEngine;

public class WormController : MonoBehaviour 
{
	[SerializeField] private int nodeLength = 2;
	[SerializeField] private float distance = 2f;
	
	private Node [] nodeObjects = null;
	public Node[] NodeObjects { get { return this.nodeObjects; } }
	
	private void Start() 
	{
		this.nodeObjects = new Node[this.nodeLength];
		for (int i = 0; i < this.nodeLength; i++ )
		{
			this.nodeObjects[i] = new GameObject("Worm_Node_").AddComponent<Node>();
			this.nodeObjects[i].transform.parent = this.transform;
		}
		for (int i = 0; i < this.nodeLength; i++) 
		{
			if (i == 0) 
			{
				this.nodeObjects[i].SetNode(null, this.nodeObjects[i + 1], this.distance) ;
				continue;
			}
			else if (i == this.nodeLength - 1) 
			{
				this.nodeObjects[i].SetNode(this.nodeObjects[i - 1], null, this.distance);
				continue;
			}
			this.nodeObjects[i].SetNode(this.nodeObjects[i - 1], this.nodeObjects[i + 1], this.distance);
		}
		this.gameObject.GetComponent<WormRendering >().Init(this.nodeObjects);
	}
}