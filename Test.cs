using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Test : MonoBehaviour
{
	[SerializeField] GameObject point;
	[SerializeField] GameObject obj;
	List<NavMeshGenerator.Surface> surfaces = new List<NavMeshGenerator.Surface>();
	List<GameObject> markers = new List<GameObject>();

	void Start() {
		Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
		Vector3[] verts = mesh.vertices;
		int[] triangles = mesh.triangles;

		for(int i = 0; i < triangles.Length;) {
			Vector3 P1 = verts[triangles[i++]];
			Vector3 P2 = verts[triangles[i++]];
			Vector3 P3 = verts[triangles[i++]];
			surfaces.Add(new NavMeshGenerator.Surface(obj, new[] { P1, P2, P3 }));
		}
	}

	void Update() {
		for(int i = 0; i < surfaces.Count; i++) {
			DebugExtension.DebugPoint(surfaces[i].ClosestPoint(point.transform.position), Color.green, .1f);
			//DebugExtension.DebugArrow(surfaces[i].Center, surfaces[i].Normal*.5f);
		}
		//int j = 0;
		//DebugExtension.DebugPoint(surfaces[j].P[0], Color.red, .1f);
		//DebugExtension.DebugPoint(surfaces[j].P[1], Color.yellow, .1f);
		//DebugExtension.DebugPoint(surfaces[j].P[2], Color.blue, .1f);
		//DebugExtension.DebugPoint(surfaces[j].ClosestPoint(point.transform.position), Color.red, .1f);
		DebugExtension.DebugLocalCube(obj.transform, new Vector3(1.0f, 1.0f, 1.0f), Color.cyan);
	}
}
