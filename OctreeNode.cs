using UnityEngine;

class OctreeNode
{
	public Vector3 Centre { get; private set; }
	public float SideLength { get; private set; }

	public OctreeNode Parent { get; private set; }
	OctreeNode[] Children = null;
	bool HasChildren { get { return Children != null; } }

	public bool Visible {
		get { return _visible; }
		set {
			_visible = value;
			if(Children != null)
				foreach(OctreeNode child in Children)
					child.Visible = value;
		}
	}
	bool _visible = false;

	OctreeNode(OctreeNode parent, Vector3 centre) {
		Parent = parent;
		Centre = centre;
		SideLength = Parent.SideLength/2;
	}

	OctreeNode(Vector3 centre, float size) {
		Parent = null;
		Centre = centre;
		SideLength = size;
	}

	public static OctreeNode CreateRoot(Vector3 centre, float size) {
		return new OctreeNode(centre, size);
	}

	public void PopulateTree(float sizeLimit) {
		if(SideLength > sizeLimit) {
			CreateChildren();
			foreach(OctreeNode node in Children)
				node.PopulateTree(sizeLimit);
		}
	}

	void CreateChildren() {
		Children = new OctreeNode[8];
		float offset = SideLength/4;
		Vector3 location = new Vector3();

		for(int i = 0; i < 8; i++) {
			location.x = i % 2 == 0 ? -offset : offset;
			location.y = i >= 4 ? offset : -offset;
			location.z = i < 2 || i >= 6 ? offset : -offset;
			Children[i] = new OctreeNode(this, Centre + location);
			Children[i].Visible = Visible;
		}
	}

	public bool PruneTree() { //returns bool value of if we need to keep this node
		bool returnVal = false;
		if(HasChildren) {
			for(int i = 0; i < 8; i++) {
				if(Children[i].PruneTree()) //if any child has necessary children, then so do we
					returnVal = true;
				else //if not then we can prune it
					RemoveChild(i);
			}
		}
		return Intersects(); //If no necessary children, check if this node is necessary
	}

	void RemoveChildren() {
		for(int i = 0; i < 8; i++)
			RemoveChild(i);
	}

	void RemoveChild(int index) {
		Children[index] = null;

		bool noMoreChildren = true;
		foreach(OctreeNode child in Children)
			if(child != null)
				noMoreChildren = false;
		if(noMoreChildren)
			Children = null;
	}

	bool Intersects() {
		RaycastHit hit;
		Vector3[] locations = GetCorners();
		foreach(int i in new[] { 0, 3, 5, 6 }) { //draw lines between the eight points
			if(Physics.Raycast(locations[i], i % 2 == 0 ? Vector3.right : Vector3.left, out hit, SideLength, Physics.DefaultRaycastLayers ,QueryTriggerInteraction.Ignore))
				return true;
			if(Physics.Raycast(locations[i], i < 4 ? Vector3.up : Vector3.down, out hit, SideLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
				return true;
			if(Physics.Raycast(locations[i], i % 5 == 0 ? Vector3.back : Vector3.forward, out hit, SideLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
				return true;
		}
		return false;
	}


	public void Update() {
		if(Visible) {
			DisplayNode();
		}
	}

	public void DisplayNode() {
		if(HasChildren) {
			for(int j = 0; j < 8; j++)
				if(Children[j] != null)
					Children[j].DisplayNode();
		}
		Color colour = Intersects() && !HasChildren ? Color.red : Color.white;
		Vector3[] locations = GetCorners(); //get the 8 points		
		foreach(int i in new[] { 0, 3, 5, 6 }) { //draw lines between the eight points
			Debug.DrawLine(locations[i], locations[i % 2 == 0 ? i + 1 : i - 1], colour);
			Debug.DrawLine(locations[i], locations[i < 4 ? i + 4 : i - 4], colour);
			Debug.DrawLine(locations[i], locations[i % 5 == 0 ? i + 2 : i - 2], colour);
		}
	}

	Vector3[] GetCorners() {
		float offset = SideLength/2;
		Vector3 location = new Vector3();
		Vector3[] locations = new Vector3[8];

		for(int i = 0; i < 8; i++) {
			location.x = i % 2 == 0 ? -offset : offset;
			location.y = i >= 4 ? offset : -offset;
			location.z = i < 2 || i > 3 && i < 6 ? offset : -offset;
			locations[i] = Centre + location;
		}
		return locations;
	}
}
