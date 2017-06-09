using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelKnitter : MonoBehaviour {

	//TODO navigation when off grid

	[SerializeField] float delay = 0;
	[SerializeField] Vector3 BoundCentre;
	[SerializeField] Vector3 BoundExtents;
	Bounds bounds;
	[SerializeField] float VoxelSize;
	[SerializeField] float AgentSize;
	List<AAPrism> a = new List<AAPrism>();
	List<AAPrism> seeds = new List<AAPrism>();
	List<NavNode> navNodes = new List<NavNode>();
	System.Random rand = new System.Random(1);
	List<GameObject> staticObjects = new List<GameObject>();
	bool knitDone = false, graphDone = false;
	[SerializeField] int navlayer;

	[SerializeField] GameObject start; //to move around for debugging
	[SerializeField] GameObject finish; //to move around for debugging

	void Start () {
		bounds = new Bounds(BoundCentre, BoundExtents*2);
		//Voxelize space within bounds
		Vector3 startloc = BoundCentre - BoundExtents + new Vector3(VoxelSize/2, VoxelSize/2, VoxelSize/2);
		for(float i = startloc.x; i < BoundCentre.x + BoundExtents.x; i += VoxelSize)
			for(float j = startloc.y; j < BoundCentre.y + BoundExtents.y; j += VoxelSize)
				for(float k = startloc.z; k < BoundCentre.z + BoundExtents.z; k += VoxelSize)
					seeds.Add(new AAPrism(new Vector3(i, j, k), new Vector3(VoxelSize, VoxelSize, VoxelSize), navlayer));
		//grow all static objects
		//IEnumerable<Collider> objs = FindObjectsOfType<Collider>().Where(x => x.gameObject.isStatic);
		//foreach(var item in objs)
		//	if(!item.isTrigger && item.gameObject.GetComponent<MeshFilter>() != null)
		//		staticObjects.Add(item.gameObject);
		//Debug.Log("Found " + staticObjects.Count + " static objects");
		//foreach(var item in staticObjects) {
		//	if(item.tag == "Plane")
		//		continue;
		//	//Transform parent = item.transform.parent;
		//	//item.transform.SetParent(empty.transform, false);
		//	Transform orig = item.transform;
		//	item.transform.position = Vector3.zero;
		//	Vector3 size = item.GetComponent<MeshFilter>().mesh.bounds.size;
		//	item.transform.localScale += new Vector3((AgentSize/size.x),
		//											(AgentSize/size.y),
		//											(AgentSize/size.z));
		//	//item.transform.SetParent(parent, false);
		//	item.transform.position = orig.position;
		//	//use bounds!!
		//	//no, use expanding cells to check	
		//}
		//if the seeds intersect an object or overlap a boundary we remove them
		seeds.RemoveAll(item => item.Intersects);
		seeds.RemoveAll(item => OutOfBounds(item));
		StartCoroutine(Knit());
	}

	Vector3 AbsoluteScale(GameObject x) {
		Vector3 scale = Vector3.one;
		Transform t = x.transform;
		while(t != null) {
			scale = new Vector3(scale.x*t.localScale.x, scale.y*t.localScale.y, scale.z*t.localScale.z);
			t = t.parent;
		}
		return scale;
	}

	bool OutOfBounds(AAPrism current) {
		bool returnval = false;
		current.Contract(VoxelSize*.1f);
		foreach(var point in current.GetCorners())
			if(!bounds.Contains(point))
				returnval = true;
		current.Expand(VoxelSize*.1f);
		return returnval;
	}
	
	void Update () {
		foreach(var item in a) {
			item.Update();
		}
		foreach(var item in navNodes) {
			item.Update();
		}
		//navNodes[58].DisplayNeighbours(0);

		if(knitDone) {
		//int num = 0;
		//a[num].Display(Color.white);
		//foreach(var item in a[num].neighbours) {
		//	item.prism.Display(Color.green);
		//}
			AStarPath(start.transform.position, finish.transform.position); //pathfind from start to finish
		}
	}

	IEnumerator Knit() {
		//choose voxel and start growing
		//when edge hit, stop that edge
		Debug.Log("Beginning knit");
		while(seeds.Count > 0) {
			Debug.Log("Seeds Remaining: " + seeds.Count);
			bool[] growing = new[] { true, true, true, true, true, true }; //+x, +y +z -x, -y -z
			AAPrism current = seeds[rand.Next(0, seeds.Count - 1)];
			seeds.Remove(current);
			a.Add(current);
			current.isSeed = false;
			if(delay != 0)
				foreach(var seed in seeds)
					seed.Display(delay*10f);

			while(growing[0] || growing[1] || growing[2] || growing[3] || growing[4] || growing[5]) {
				if(growing[0]) { //step x
					growing[0] = KnitStep(current, Axis.x, 1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
				if(growing[1]) { //step y
					growing[1] = KnitStep(current, Axis.y, 1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
				if(growing[2]) { //step z
					growing[2] = KnitStep(current, Axis.z, 1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
				if(growing[3]) { //step -x
					growing[3] = KnitStep(current, Axis.x, -1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
				if(growing[4]) { //step -y
					growing[4] = KnitStep(current, Axis.y, -1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
				if(growing[5]) { //step -z
					growing[5] = KnitStep(current, Axis.z, -1);
					if(delay != 0) yield return new WaitForSeconds(delay);
				}
			}
			if(delay != 0) yield return new WaitForSeconds(delay);
		}
		//set up navnodes
		foreach(var prism in a) {
			prism.drawn = false;
			List<NavNode> nodes = new List<NavNode>();
			foreach(var point in prism.GetAllPoints())
				nodes.Add(new NavNode(point));
			for(int i = 0; i < nodes.Count - 1; i++)
				for(int j = i + 1; j < nodes.Count; j++) {
					nodes[i].AddNeighbour(nodes[j]);
					nodes[j].AddNeighbour(nodes[i]);
				}
			foreach(var item in nodes) {
				//if the node is already in the main list, just add neighbours
				if(navNodes.Any((x) => item.position == x.position)) {
					int index = navNodes.FindIndex((x) => item.position == x.position);
					foreach(var neighbour in item.neighbours)
						navNodes[index].AddNeighbour(neighbour);
				}
				else
					navNodes.Add(item); //otherwise add the node
			}
		}
		knitDone = true;
	}

	bool KnitStep(AAPrism current, Axis axis, float posneg) {
		current.Expand(axis, posneg*VoxelSize);
		current.Contract(VoxelSize*.1f); //This is a little hack to prevent coplanar hits along non-expanding sides
		//we have to make sure to expand again before exiting
		foreach(var point in current.GetCorners()) //check if we hit the outside bounds //TODO only do max and min?
			if(!bounds.Contains(point)) {
				current.Expand(VoxelSize*.1f);
				current.Contract(axis, posneg*VoxelSize);
				return false;
			}
		foreach(var item in a) { //check if we hit another prism
			if(item.Equals(current)) continue;
			if(item.bounds.Intersects(current.bounds)) {
				current.Expand(VoxelSize*.1f);
				current.Contract(axis, posneg*VoxelSize);
				//calculate points of intersection and log the neighbour relation
				List<Vector3> points = current.DetermineIntersectionPoints(item);
				current.AddNeighbour(item, points);
				item.AddNeighbour(current, points);
				return false;
			}
		}
		current.Expand(VoxelSize*.1f);

		if(current.Intersects) { //check if we hit an object
			current.Contract(axis, posneg*VoxelSize);
			return false;
		}
		else { //no intersections so growth continues, remove all covered seeds
			Bounds b = current.bounds;
			seeds.RemoveAll(item => b.Contains(item.centre));
			return true;
		}
	}


	void AStarPath() {
		AAPrism s = null, f = null;

		foreach(var item in a) {
			item.H = (item.centre - finish.transform.position).sqrMagnitude;
			item.parent = null;
			item.state = NodeState.Untested;
			if(item.bounds.Contains(start.transform.position)) {
				item.Display(Color.green);
				s = item;
			}
			if(item.bounds.Contains(finish.transform.position)) {
				item.Display(Color.red);
				f = item;
			}
		}
		if(PathSearch(s, f)) {
			ColourPath(f);
		}
		else
			Debug.Log("Failed to find path!");
	}

	List<Vector3> AStarPath(Vector3 s, Vector3 f) {
		AAPrism startPrism = null, endPrism = null;
		List<NavNode> list = new List<NavNode>(navNodes); //copy the list so we can modify it non-destructively
		foreach(var prism in a) { //find the containing prism for start and finish
			if(prism.bounds.Contains(s)) {
				prism.Display(Color.green);
				startPrism = prism;
			}
			if(prism.bounds.Contains(f)) {
				prism.Display(Color.red);
				endPrism = prism;
			}
		}
		//TODO add nullchecking on prisms to see if off grid
		NavNode startNode = new NavNode(s); //add start and finish to list
		NavNode endNode = new NavNode(f);
		list.Add(startNode);
		list.Add(endNode);
		foreach(var location in startPrism.GetAllPoints()) {//update neighbours
			NavNode node = getNodeAt(list, location);
			node.AddNeighbour(startNode);
			startNode.AddNeighbour(node);
		}
		foreach(var location in endPrism.GetAllPoints()) {
			NavNode node = getNodeAt(list, location);
			node.AddNeighbour(endNode);
			endNode.AddNeighbour(node);
		}
		foreach(var item in list) {//prepare to pathfind
			item.H = (item.position - endNode.position).sqrMagnitude;
			item.parent = null;
			item.state = NodeState.Untested;
		}
		List<Vector3> path = new List<Vector3>();
		if(PathSearch(startNode, endNode)) {
			Debug.Log("Path found!");
			path = ReturnPath(endNode);
			for(int i = 0; i < path.Count - 1; i++) {
				//DebugExtension.DebugPoint(path[i], Color.magenta, .5f);
				Debug.DrawLine(path[i], path[i + 1], Color.cyan);
			}
			SmoothPath(path);
			for(int i = 0; i < path.Count - 1; i++) {
				//DebugExtension.DebugPoint(path[i], Color.magenta, .5f);
				Debug.DrawLine(path[i], path[i + 1], Color.yellow);
			}
		}
		else
			Debug.Log("Failed to find path!");
		return path;
	}

	List<Vector3> ReturnPath(NavNode f) {
		NavNode currentNode = f;
		List<Vector3> list = new List<Vector3> { currentNode.position };
		for(int i = 0; i < 100; i++) {
			if(currentNode.parent == null)
				break;
			currentNode = currentNode.parent;
			list.Add(currentNode.position);
		}
		list.Reverse();
		return list;
	}

	void SmoothPath(List<Vector3> path) {
		//Raycast from current point to each other in turn (starting from end)
		//If path is clear remove intervening points
		//repeat
		//TODO use funnel algo instead
		for(int i = 0; i < path.Count - 2; i++) {
			for(int j = path.Count - 1; j > i + 1; j--) {
				Vector3 dir = path[j] - path[i];
				Ray ray = new Ray(path[i], dir);
				float dist = dir.magnitude;
				if(!Physics.Raycast(ray, dist)) {
					path.RemoveRange(i + 1, j - i - 1); //remove all points between i and j
					j = path.Count - 1; //reset j to the new last point
				}
			}
		}
	}


	bool PathSearch(NavNode current, NavNode finish) {
		current.state = NodeState.Closed;
		if(current.Equals(finish))
			return true;
		foreach(var item in current.neighbours) {
			if(item.node.state == NodeState.Closed)
				continue;
			if(item.node.state == NodeState.Open) {
				if(item.distance < item.node.G) { //if the new g is smaller then the old one
					item.node.parent = current;
					item.node.G = item.distance;
				}
			}
			else {
				item.node.state = NodeState.Open;
				item.node.parent = current;
				item.node.G = item.distance;
			}
		}
		current.neighbours.Sort((x, y) => x.node.F.CompareTo(y.node.F));
		foreach(var node in current.neighbours.Where(x => x.node.state != NodeState.Closed)) {
			if(PathSearch(node.node, finish))
				return true;
		}
		return false;
	}

	bool PathSearch(AAPrism current, AAPrism finish) {
		current.state = NodeState.Closed;
		if(current.Equals(finish))
			return true;
		foreach(var item in current.neighbours) {
			if(item.prism.state == NodeState.Closed)
				continue;
			if(item.prism.state == NodeState.Open) {
				if(item.distance < item.prism.G) { //if the new g is smaller then the old one
					item.prism.parent = current;
					item.prism.G = item.distance;
				}
			}
			else {
				item.prism.state = NodeState.Open;
				item.prism.parent = current;
				item.prism.G = item.distance;
			}
		}
		current.neighbours.Sort((x, y) => x.prism.F.CompareTo(y.prism.F));
		foreach(var node in current.neighbours.Where(x => x.prism.state != NodeState.Closed)) {
			if(PathSearch(node.prism, finish))
				return true;
		}
		return false;
	}

	void ColourPath(AAPrism s) {
		if(s.parent == null) return;
		s.parent.Display(Color.magenta);
		ColourPath(s.parent);
	}

	public enum NodeState { Untested, Open, Closed }

	public static NavNode getNodeAt(List<NavNode> list, Vector3 location) {
		foreach(var node in list)
			if(node.position == location)
				return node;
		Debug.Log("No node found at position " + location);
		return null;
	}

	public class NavNode {
		public Vector3 position;
		public List<NavNodeNeighbour> neighbours = new List<NavNodeNeighbour>();
		public bool drawn = true;

		//pathfinding stuff
		public NodeState state = NodeState.Untested;
		public float G;
		public float H;
		public float F { get { return G + H; } }
		public NavNode parent;

		public NavNode(Vector3 position) {
			this.position = position;
		}

		public NavNode(NavNode original) {
			position = original.position;
			neighbours = original.neighbours;
		}

		public void AddNeighbour(NavNodeNeighbour node) {
			if(!neighbours.Contains(node))
				neighbours.Add(node);
		}

		public void AddNeighbour(NavNode node) {
			AddNeighbour(new NavNodeNeighbour(this, node));
		}

		public void Update() {
			if(drawn)
				Display();
		}

		public void Display() {
			Display(0);
		}

		public void Display(float duration) {
			DebugExtension.DebugPoint(position, Color.magenta, 0.5f, duration);
		}

		public void Display(Color colour) {
			DebugExtension.DebugPoint(position, colour, 0.5f);
		}

		public void DisplayNeighbours(float duration) {
			foreach(var item in neighbours) {
				Debug.DrawLine(position, item.node.position, Color.red, duration);
			}
		}

		public struct NavNodeNeighbour
		{
			public NavNode node;
			public float distance;

			public NavNodeNeighbour(NavNode original, NavNode neighbour) {
				node = neighbour;
				distance = (original.position - node.position).sqrMagnitude;
			}

			public override bool Equals(object obj) {
				if(obj == null) return false;
				return obj is NavNodeNeighbour && node == ((NavNodeNeighbour)obj).node;
			}

			public override int GetHashCode() {
				return node.GetHashCode();
			}
		}
	}


	class AAPrism {
		public Vector3 centre, Size; //full size of edge
		public bool drawn = true, isSeed = true;
		public bool Intersects { get { return Physics.CheckBox(centre, Size/2f, Quaternion.identity, layer); } }
		public Bounds bounds { get { return new Bounds(centre, Size); } }
		int layer;

		//pathfinding stuff
		public NodeState state = NodeState.Untested;
		public float G;
		public float H;
		public float F { get { return G + H; } }
		public AAPrism parent;

		public List<NeighbourInfo> neighbours = new List<NeighbourInfo>();

		public AAPrism(Vector3 loc, Vector3 dims, int navlayer) {
			centre = loc;
			Size = dims;
			layer = 1 << navlayer;
		}

		public void Expand(Axis a, float amount) {
			if(amount == 0) return;
			if(a == Axis.x) {
				centre.x += amount/2f;
				Size.x += Mathf.Abs(amount);
			}
			else if(a == Axis.y) {
				centre.y += amount/2f;
				Size.y += Mathf.Abs(amount);
			}
			else if(a == Axis.z) {
				centre.z += amount/2f;
				Size.z += Mathf.Abs(amount);
			}
		}

		public void Expand(float amount) {
			foreach(Axis item in Enum.GetValues(typeof(Axis)))
				foreach(float x in new[] { 1, -1 })
					Expand(item, x*amount);
		}

		public void Contract(Axis a, float amount) {
			if(amount == 0) return;
			if(a == Axis.x) {
				centre.x -= amount/2f;
				Size.x -= Mathf.Abs(amount);
			}
			else if(a == Axis.y) {
				centre.y -= amount/2f;
				Size.y -= Mathf.Abs(amount);
			}
			else if(a == Axis.z) {
				centre.z -= amount/2f;
				Size.z -= Mathf.Abs(amount);
			}
		}

		public void Contract(float amount) {
			foreach(Axis item in Enum.GetValues(typeof(Axis)))
				foreach(float x in new[] { 1, -1 })
					Contract(item, x*amount);
		}

		public void Update() {
			if(drawn)
				Display();
		}

		public void Display() {
			Display(0);
		}

		public void Display(float duration) {
			if(isSeed) { //If still a seed show a purple centrepoint instead
				DebugExtension.DebugPoint(centre, Color.magenta, .3f, duration: duration);
				return;
			}
			Color colour = Intersects ? Color.red : Color.white;
			Vector3[] locations = GetCorners(); //get the 8 points		
			foreach(int i in new[] { 0, 3, 5, 6 }) { //draw lines between the eight points
				Debug.DrawLine(locations[i], locations[i % 2 == 0 ? i + 1 : i - 1], colour, duration);
				Debug.DrawLine(locations[i], locations[i < 4 ? i + 4 : i - 4], colour, duration);
				Debug.DrawLine(locations[i], locations[i % 5 == 0 ? i + 2 : i - 2], colour, duration);
			}
		}

		public void Display(Color colour) {
			Vector3[] locations = GetCorners(); //get the 8 points		
			foreach(int i in new[] { 0, 3, 5, 6 }) { //draw lines between the eight points
				Debug.DrawLine(locations[i], locations[i % 2 == 0 ? i + 1 : i - 1], colour, 0);
				Debug.DrawLine(locations[i], locations[i < 4 ? i + 4 : i - 4], colour, 0);
				Debug.DrawLine(locations[i], locations[i % 5 == 0 ? i + 2 : i - 2], colour, 0);
			}
		}

		public Vector3[] GetCorners() {
			float x = Size.x/2, y = Size.y/2, z = Size.z/2;
			Vector3 location = new Vector3();
			Vector3[] locations = new Vector3[8];

			for(int i = 0; i < 8; i++) {
				location.x = i % 2 == 0 ? -x : x;
				location.y = i >= 4 ? y : -y;
				location.z = i < 2 || i > 3 && i < 6 ? z : -z;
				locations[i] = centre + location;
			}
			return locations;
		}

		public List<Vector3> GetAllPoints() {
			List<Vector3> points = GetCorners().ToList();
			foreach(var item in neighbours)
				foreach(var point in item.intersectionPoints)
					points.Add(point);
			return points.Distinct().ToList();
		}

		public List<Vector3> DetermineIntersectionPoints(AAPrism other) {
			List<Vector3> points = new List<Vector3>();
			Vector3 min = new Vector3(Mathf.Max(bounds.min.x, other.bounds.min.x),
									  Mathf.Max(bounds.min.y, other.bounds.min.y),
									  Mathf.Max(bounds.min.z, other.bounds.min.z));
			Vector3 max = new Vector3(Mathf.Min(bounds.max.x, other.bounds.max.x),
									  Mathf.Min(bounds.max.y, other.bounds.max.y),
									  Mathf.Min(bounds.max.z, other.bounds.max.z));
			//TODO check for no intersection case
			Vector3 diff = max - min;
			//merge points on the zero dist. axes
			float[] xvals = Mathf.Abs(diff.x) < 1e-5 ? new[] { min.x } : new[] { min.x, max.x };
			float[] yvals = Mathf.Abs(diff.y) < 1e-5 ? new[] { min.y } : new[] { min.y, max.y };
			float[] zvals = Mathf.Abs(diff.z) < 1e-5 ? new[] { min.z } : new[] { min.z, max.z };
			foreach(var x in xvals)
				foreach(var y in yvals)
					foreach(var z in zvals)
						points.Add(new Vector3(x, y, z));
			
			if(points.Count > 4) { //something is wrong, intersection is > 2 dimensional
				Debug.Log("Prism intersection is box!");
			}

			return points;
		}

		public void AddNeighbour(AAPrism neighbour, List<Vector3> points) {
			Vector3 average = Vector3.zero;
			foreach(var item in points)
				average += item;
			average /= points.Count;
			float d1 = (centre - average).sqrMagnitude, d2 = (neighbour.centre - average).sqrMagnitude;
			NeighbourInfo n = new NeighbourInfo(neighbour, d1 + d2, points);
			if(neighbours.Contains(n)) return;
			neighbours.Add(n);
		}
	}


	struct NeighbourInfo
	{
		public AAPrism prism;
		public float distance;
		public List<Vector3> intersectionPoints;

		public NeighbourInfo(AAPrism item, float dist, List<Vector3> points) {
			prism = item;
			distance = dist;
			intersectionPoints = points;
		}

		public override bool Equals(object obj) {
			if(obj == null) return false;
			if(obj.GetType() == typeof(NeighbourInfo))
				return ((NeighbourInfo)obj).prism.Equals(prism) && ((NeighbourInfo)obj).distance == distance;
			else
				return false;
		}
	}


	public enum Axis { x, y, z }
}
