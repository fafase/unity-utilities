using UnityEngine;
public class Node : MonoBehaviour {
	
	private static int naming = 0;
	private Node prevNode = null;
	private Node nextNode = null;
	
	public Node PrevNode
	{
		get{return this.prevNode;} 
	}
	public Node NextNode
	{
		get { return this.nextNode; }
	}
	public void SetNode(Node prev, Node next, float distance) 
	{
		this.name = this.name + naming.ToString();
		naming++;
		this.prevNode = prev;
		this.nextNode = next;
		if (this.prevNode == null) { return; }
		Vector3 pos = this.prevNode.transform.position;
		pos.z += distance;
		this.transform.position = pos;
	}
}