using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavMeshGenerator : MonoBehaviour {

	List<GameObject> list = new List<GameObject>();
	List<Surface> surfaces = new List<Surface>();
	//public Vector3 point;
	[SerializeField] GameObject point; //to move around for debugging


	void Start () {
		Collider[] x = FindObjectsOfType<Collider>();
		foreach(var item in x)
			if(!item.isTrigger && item.gameObject.GetComponent<MeshFilter>() != null)
				list.Add(item.gameObject);
		Debug.Log("Found " + list.Count + " Objects");
		GenerateMesh();
	}

	private void Update() {
		foreach(var item in surfaces)
			//Debug.DrawLine(item.Center, item.Center + item.Normal);
			DebugExtension.DebugPoint(item.ClosestPoint(point.transform.position), Color.green, .1f);
	}


	void GenerateMesh() {
		Debug.Log("Grabbing Meshes");
		foreach(var item in list) {
			Collider c = item.GetComponent<Collider>();
			Mesh mesh = item.GetComponent<MeshFilter>().mesh;
			//TODO simpify mesh https://www.cs.mtu.edu/~shene/COURSES/cs3621/SLIDES/Simplification.pdf
			Vector3[] verts = mesh.vertices;
			int[] triangles = mesh.triangles;

			for(int i = 0; i < triangles.Length;) {
				Vector3 P1 = verts[triangles[i++]];
				Vector3 P2 = verts[triangles[i++]];
				Vector3 P3 = verts[triangles[i++]];
				surfaces.Add(new Surface(item, new[] { P1, P2, P3 }));
			}
		}

		//Remove all normals that point away from the seed point
		foreach(var item in surfaces.Where(x => Vector3.Dot(x.Normal, point.transform.position - x.Center) <= 0).ToList()) {
			surfaces.Remove(item);
		}

		//Sort by relative spatial position
		//Lists are {+y, -y, +x, -x, +z, -z}
		List<KeyValuePair<Surface, float>>[] relativeSurfaces = new List<KeyValuePair<Surface, float>>[6];
		foreach(var item in surfaces) { //TODO check and see if this really works fine based only on centers
			Vector3 p = Vector3.Scale(item.Center, item.Center); //might have to do each vertex instead
			float dist = item.ClosestDistance(point.transform.position);
			KeyValuePair<Surface, float> kvp = new KeyValuePair<Surface, float>(item, dist);
			if(p.y >= p.z && p.y >= p.x) {
				if(item.Center.y >= 0)
					relativeSurfaces[0].Add(kvp);
				else
					relativeSurfaces[1].Add(kvp);
			} else if(p.x >= p.z) {
				if(item.Center.x >= 0)
					relativeSurfaces[2].Add(kvp);
				else
					relativeSurfaces[3].Add(kvp);
			} else {
				if(item.Center.z >= 0)
					relativeSurfaces[4].Add(kvp);
				else
					relativeSurfaces[5].Add(kvp);
			}
		}

		//sort by distance
		foreach(var item in relativeSurfaces)
			//item.Sort((x, y) => Surface.CompareDistance(x, y, point.transform.position));
			item.Sort((x, y) => x.Value.CompareTo(y.Value));

		//categorize the event points
		//if surface is parallel, flat edge
		//if surface normal points away from the midpoint of the expanding edge, 
		//the first point hit is a vertex, we stop there (intruding vertex)
		//if it points towards the midpoint, we have a splitting situation
		//we have to store the dimensions of the surface to know the future dimensions of the new surface
		//

		//add any world boundaries to the event list
		//sort event points by distance

	}

	//http://search.proquest.com/openview/2df72c5cc6338149feb6a5b3a3208ecb/1?pq-origsite=gscholar&cbl=18750&diss=y
	//1. get all the GameObjects with MeshFilter Components (simplify meshes?)
	//2. Get the normals from mesh (collapse normals from same face?)
	//3. remove all normals that point away from the seed point
	//5. categorize the event points and sort by distance
	//6. Examine how we need to grow to get to the first event point
	//remove it from the list and grow to it
	//7. permentantly stop growing an edge if it collides
	//8. continue until all edges have stopped, or events list is empty
	//place a new seed and repeat 

	//INVERSE MESH KNITTING?
	//voxel knitting?

	//Check if custom simplified mesh is present
	//Check if collider is not mesh
	//
	//if not, try using the AABB, if excess volume is too large, try OOBB instead
	//depending on faces of mesh, 

	public struct EventPoint
	{
		public enum EventPointType
		{
			Parallel,
			Intruding,
			Split
		}

		public Vector3 Position;
		public EventPointType Type;
		public Surface Surface;
		public float Distance;

		//public EventPoint(Surface surf, Vector3 seed) {
		//	Surface = surf;
		//	//categorize
		//	//get point and distance
		//}
	}

	public struct Surface
	{
		GameObject Parent;
		public Vector3[] P, tP;
		public readonly Vector3 Center, Normal;
		public readonly Transform2D t;

		public Surface(GameObject parent, IEnumerable<Vector3> points) {
			if(points.Count() != 3) {
				throw new UnityException("Could not create surface");
			}
			Parent = parent;
			P = points.ToArray();
			for(int i = 0; i < P.Count(); i++) {
				P[i] = Parent.transform.rotation*P[i];
				P[i] += Parent.transform.position; //transform the points to world coordinates
			}
			Center = Average(P);
			Normal = Vector3.Normalize(Vector3.Cross(P[1] - P[0], P[2] - P[0]));
			t = new Transform2D(P);
			tP = new Vector3[P.Count()]; //store the transformed points
			for(int i = 0; i < P.Count(); i++)
				tP[i] = t.Transform(P[i]);
		}

		public static int CompareDistance(Surface s1, Surface s2, Vector3 point) {
			return s1.ClosestDistance(point).CompareTo(s2.ClosestDistance(point));
		}

		public float ClosestDistance(Vector3 point) {
			return Vector3.Magnitude(point - ClosestPoint(point));
		}

		 public Vector3 ClosestPoint(Vector3 point) {
			//http://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
			//http://stackoverflow.com/questions/14467296/barycentric-coordinate-clamping-on-3d-triangle
			point = t.Transform(point);
			Vector2 p = new Vector2(point.z, point.y);
			Vector2 p0 = new Vector2(tP[0].z, tP[0].y);
			Vector2 p1 = new Vector2(tP[1].z, tP[1].y);
			Vector2 p2 = new Vector2(tP[2].z, tP[2].y);
			float Area = 0.5f *(-p1.y*p2.x + p0.y*(-p1.x + p2.x) + p0.x*(p1.y - p2.y) + p1.x*p2.y);
			float v = 1/(2*Area)*(p0.y*p2.x - p0.x*p2.y + (p2.y - p0.y)*p.x + (p0.x - p2.x)*p.y);
			float w = 1/(2*Area)*(p0.x*p1.y - p0.y*p1.x + (p0.y - p1.y)*p.x + (p1.x - p0.x)*p.y);
			float u = 1 - w - v;

			Vector3 vector = new Vector3(u, v, w);
			if(u < 0) {
				float x = Vector2.Dot(p-p1, p2-p1)/Vector2.Dot(p2-p1, p2-p1);
				x = Mathf.Clamp01(x);
				vector = new Vector3(0.0f, 1.0f-x, x);
			}
			else if(v < 0) {
				float x = Vector2.Dot(p-p2, p0-p2)/Vector2.Dot(p0-p2, p0-p2);
				x = Mathf.Clamp01(x);
				vector = new Vector3(x, 0.0f, 1.0f-x);
			}
			else if(w < 0) {
				float x = Vector2.Dot(p-p0, p1-p0)/Vector2.Dot(p1-p0, p1-p0);
				x = Mathf.Clamp01(x);
				vector = new Vector3(1.0f-x, x, 0.0f);
			}

			Vector2 nearest = p0*vector.x + p1*vector.y + p2*vector.z;
			return t.UnTransform(new Vector3(0, nearest.y, nearest.x));
		}

		static Vector3 Average(IEnumerable<Vector3> list) {
			Vector3 output = new Vector3();
			foreach(var vector in list)
				output += vector;
			return output / list.Count();
		}

		public struct Transform2D
		{
			public readonly Vector3 translation;
			public readonly Quaternion rotation, derotation;

			public Transform2D(Vector3[] p) {
				translation = p[0];
				Vector3 to1 = p[1] - p[0];
				rotation = Quaternion.FromToRotation(Vector3.Cross(to1, p[2] - p[0]), Vector3.right);
				to1 = rotation*to1;
				float angle = Vector3.Angle(to1, Vector3.forward) * (to1.y > 0 ? 1 : -1);
				rotation = Quaternion.Euler(Vector3.right*angle) * rotation;
				derotation = Quaternion.Inverse(rotation);
			}

			public Vector3 Transform(Vector3 p) {
				p -= translation;
				return rotation*p;
			}

			public Vector3 UnTransform(Vector3 p) {
				p = derotation*p;
				return p + translation;
			}
		}
	}

	static class TypeSwitch
	{
		// http://stackoverflow.com/questions/298976/is-there-a-better-alternative-than-this-to-switch-on-type
		public class CaseInfo
		{
			public bool IsDefault { get; set; }
			public Type Target { get; set; }
			public Action<object> Action { get; set; }
		}

		public static void Do(object source, params CaseInfo[] cases) {
			var type = source.GetType();
			foreach(var entry in cases) {
				if(entry.IsDefault || entry.Target.IsAssignableFrom(type)) {
					entry.Action(source);
					break;
				}
			}
		}

		public static CaseInfo Case<T>(Action action) {
			return new CaseInfo() {
				Action = x => action(),
				Target = typeof(T)
			};
		}

		public static CaseInfo Case<T>(Action<T> action) {
			return new CaseInfo() {
				Action = (x) => action((T)x),
				Target = typeof(T)
			};
		}

		public static CaseInfo Default(Action action) {
			return new CaseInfo() {
				Action = x => action(),
				IsDefault = true
			};
		}
	}
}
