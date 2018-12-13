using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

class TreeGenerator : MonoBehaviour
{
	[SerializeField] Vector3 Center;
	[SerializeField] float smallestEdge = .1f, size = 1;
	public OctreeNode Tree;

	[SerializeField] Transform start;
	[SerializeField] List<Transform> testpoints;

	void Start() {
		// List<GameObject> statics = (from item in FindObjectsOfType<GameObject>() 
		// where GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic) select item).ToList();

		Tree = OctreeNode.CreateRoot(Center, size);
		Tree.Visible = true;
		Tree.PopulateTree(smallestEdge);
		Tree.GetNavigationNeighbours(smallestEdge);
	}

	public List<Vector3> Navigate(Vector3 start, Vector3 dest, bool smooth = true) {
		//Find cell of start and end
		OctreeNode startNode = Tree.GetContainingNode(start);
		OctreeNode endNode = Tree.GetContainingNode(dest);

		List<NavNode> OpenList = new List<NavNode> { new NavNode(startNode, start) };
		List<NavNode> ClosedList = new List<NavNode>();

		NavNode current = null;
		Console.WriteLine("starting nav");

		while(OpenList.Count > 0) {
			current = OpenList[0];
			ClosedList.Add(current);
			OpenList.Remove(current);
			if(current.Node == endNode){
				break; //end found
			}
			foreach(OctreeNode neighbour in current.Node.Neighbours) {
				if(ClosedList.Where(x => x.Node == neighbour).Any()) {
					continue;
				}

				NavNode adjacent = new NavNode(neighbour, current, dest);

				NavNode p = OpenList.Where(x => x.Node == neighbour).FirstOrDefault();

				if(p != null) {
					if(adjacent.F < p.F) {
						OpenList.Remove(p);
						OpenList.AddSorted(adjacent);
					}
				} else {
					OpenList.AddSorted(adjacent);
				}
			}
		}

		//TODO check if a path was found
		List<Vector3> waypoints = new List<Vector3> { dest };
		while(current != null) {
			waypoints.Insert(0, current.ClosestPoint);
			current = current.Parent;
		}
		
		return smooth ? SmoothPath(waypoints) : waypoints;
	}

	public List<Vector3> Navigate(Vector3 start, List<Vector3> waypoints, bool smooth = true) {
		List<Vector3> route = new List<Vector3>(Navigate(start, waypoints[0], smooth));
		for(int i = 1; i < waypoints.Count; i++) {
			route.AddRange(Navigate(waypoints[i - 1], waypoints[i], smooth));
		}
		return route;
	}

	public List<Vector3> SmoothPath(List<Vector3> waypoints) {
		var points = new List<Vector3>(waypoints);
		int i = 0;
		while(i < points.Count - 2) {
			Vector3 dirVector = points[i + 2] - points[i];
			RaycastHit hitInfo = new RaycastHit();
			//FIXME watch the size on this, too large and it will miss detections (but can't be too small). 1/4 of smallestEdge might work
			if(!Physics.SphereCast(points[i], smallestEdge/4, dirVector, out hitInfo, dirVector.magnitude, Utility.NavLayer)) {
				points.RemoveAt(i + 1);
				continue;
			} else {
				Console.WriteLine(string.Format("Sphere collided with collider {0} at {1}", hitInfo.collider, hitInfo.point));
			}
			i++;
		}

		return points;
	}

	void Update() {
		Tree.Update();

		if(start != null && testpoints.Count > 0) {
			var points = Navigate(start.position, testpoints.Select(x => x.position).ToList(), false);
			var spoints = Navigate(start.position, testpoints.Select(x => x.position).ToList());
			Console.WriteLine("Nav complete");
			points.Insert(0, start.position);
			for(int i = 0; i < points.Count - 1; i++)
				Debug.DrawLine(points[i], points[i + 1]);
			for(int i = 0; i < spoints.Count - 1; i++)
				Debug.DrawLine(spoints[i], spoints[i + 1], Color.blue);
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.DrawWireCube(Center, new Vector3(size, size, size));
	}

	class NavNode : IComparable {
		public OctreeNode Node { get; private set; }
		public NavNode Parent { get; private set; }

		public Vector3 ParentPoint { get { return Parent.ClosestPoint; } }
		public Vector3 ClosestPoint { get; private set; }
		public float F { get; private set; }

		public NavNode(OctreeNode node, NavNode parent, Vector3 destination) {
			Node = node;
			Parent = parent;
			ClosestPoint = Node.Bounds.ClosestPoint(ParentPoint);
			F = (ClosestPoint - ParentPoint).sqrMagnitude + (destination - ClosestPoint).sqrMagnitude;
		}

		public NavNode(OctreeNode node, Vector3 point) {
			Node = node;
			ClosestPoint = point;
		}

		public int CompareTo(object obj) {
			if(obj == null) return 1;

			NavNode otherNode = obj as NavNode;
			if(otherNode != null) {
				return this.F.CompareTo(otherNode.F);
			} else
				throw new ArgumentException("Object is not a NavNode");
		}
	}
}
