using UnityEngine;
using System.Collections;

public class WormRendering : MonoBehaviour 
{
    [SerializeField]
    private Color startColor = Color.white;
    [SerializeField]
    private Color endColor = Color.blue;
    [SerializeField]
    private float widthStart = 2f;
    [SerializeField]
    private float widthEnd = 1f;
    [SerializeField]
    private int accuracy = 1;

    private Node[] nodes = null;
    private LineRenderer lineRenderer = null;
    private int length = 0;
    private int nodeLength = 0;

	public void Init (Node[] nodeArray) 
    {
        this.nodes = nodeArray;
        this.nodeLength = this.nodes.Length;

		this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
		this.lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		this.lineRenderer.SetColors(this.startColor, this.endColor);
		this.lineRenderer.SetWidth(this.widthStart, this.widthEnd);
		this.length = this.nodes.Length * (this.accuracy - 1) + 1;
        lineRenderer.SetVertexCount(length);
   	}

    private void Update()
    {
        int j = 0;
		float acc = 1f / (float)this.accuracy;
        for (int index = 0; index < this.nodeLength-1; index++ )
        {
            Node n1 = this.nodes[index].PrevNode;
            Vector3 p1 = (n1 == null) ? this.nodes[index].transform.position : n1.transform.position;

            Vector3 p2 = this.nodes[index].transform.position;
            Vector3 p3 = this.nodes[index + 1].transform.position;

            Node n4 = this.nodes[index + 1].NextNode;
            Vector3 p4 = (n4 == null) ? this.nodes[index + 1].transform.position : n4.transform.position;

            float t = 0f;
            for (; t < 1; t = t + acc, j++)
            {
                Vector3 pos = WormCurve.CatmullRom(p1, p2, p3, p4, t);
                this.lineRenderer.SetPosition(j, pos);
            }
        }
		this.lineRenderer.SetPosition(this.length - 1, this.nodes[this.nodes.Length-1].transform.position);
    }
}
