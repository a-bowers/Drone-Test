using UnityEngine;

class TreeGenerator : MonoBehaviour
{
	[SerializeField] Vector3 Centre;
	[SerializeField] float smallestEdge = .1f, size = 1;
	OctreeNode Tree;

	void Start() {
		Tree = OctreeNode.CreateRoot(Centre, size);
		Tree.Visible = true;
		Tree.PopulateTree(smallestEdge);
		Tree.PruneTree();
	}

	void Update() {
		Tree.Update();
	}
}


//check each small box for intersection
//if all children do not intersect, remove them

