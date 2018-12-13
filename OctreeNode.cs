using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

class OctreeNode
{
	public Vector3 Center { get; private set; }
	public float SideLength { get; private set; }
	public Bounds Bounds { get; private set; }

	public Status State { get { return Intersects() ? /*check if entirely enclosed*/Status.GRAY : Status.VOID; }}
	
	public OctreeNode Parent { get; private set; }
	OctreeNode Root { get; set; }
	OctreeNode[] Children = null;
	bool HasChildren { get { return Children != null; } }
	public List<OctreeNode> Neighbours = new List<OctreeNode>();

	List<GameObject> staticObjects;

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

	OctreeNode(OctreeNode parent, Vector3 center) {
		Parent = parent;
		Center = center;
		SideLength = Parent.SideLength/2;
		Bounds = new Bounds(Center, new Vector3(SideLength, SideLength, SideLength));
		Root = Parent.Root;
		Visible = Parent.Visible;
	}

	OctreeNode(Vector3 centre, float size) {
		Parent = null;
		Center = centre;
		SideLength = size;
		Bounds = new Bounds(Center, new Vector3(SideLength, SideLength, SideLength));
		Root = this;
	}

	public static OctreeNode CreateRoot(Vector3 centre, float size) {
		return new OctreeNode(centre, size);
	}

	OctreeNode CreateChild(Vector3 center) {
		return new OctreeNode(this, center);
	}

	public void PopulateTree(float sizeLimit) {
		if(SideLength > sizeLimit && State == Status.GRAY) {
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
			Children[i] = CreateChild(Center + location);
		}
	}

	public void GetNavigationNeighbours(float sizeLimit) {
		//go down to terminal nodes
		if(HasChildren) {
			foreach(OctreeNode node in Children)
				node.GetNavigationNeighbours(sizeLimit);
			return;
		}

		if(State == Status.FULL) return; //don't worry about neighbours for full cells

		//grow bounds
		float size = SideLength + sizeLimit/2;
		Bounds neighbourBounds = new Bounds(Center, new Vector3(size, size, size));

		//get all contacting bounds
		List<OctreeNode> potentials = Root.Children.ToList();
		while(potentials.Count > 0) {
			OctreeNode node = potentials[0];
			potentials.RemoveAt(0);
			if(!neighbourBounds.Intersects(node.Bounds)) continue; //no intersection, not a neighbour
			if(node.HasChildren) { //non-terminal, add children to list
				potentials.AddRange(node.Children);
			} else { //terminal neighbour
				Neighbours.Add(node);
			}
		}

		//remove all gray/full leaves
		Neighbours.RemoveAll(x => x.State >= Status.FULL);
	}

	public OctreeNode GetContainingNode(Vector3 point) {
		//assumes the point is in the bounds of the root node
		if(HasChildren) {
			foreach(OctreeNode node in Children) {
				if(node.Bounds.Contains(point)) {
					return node.GetContainingNode(point);
				}
			}
			//throw an error here if we want to check if point outside root
		}
		return this;
	}

	bool Intersects() {
		float x = SideLength/2;
		return Physics.CheckBox(Center, new Vector3(x, x, x), Quaternion.identity, Utility.NavLayer);
	}

	public void Update() {
		if(Visible) {
			DisplayNode();
		}
		if(HasChildren) {
			foreach(var child in Children) {
				child.Update();
			}
		}
	}

	public void DisplayNode() {
		if(HasChildren || State == Status.VOID) {
			return;
		}
		Color colour = /*State == Status.FULL ? Color.red : Color.yellow*/ Color.red;
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
			locations[i] = Center + location;
		}
		return locations;
	}

	public enum Status {
		VOID,
		FULL,
		GRAY
	}
}
