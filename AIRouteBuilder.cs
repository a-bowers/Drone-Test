using System.Collections.Generic;
using UnityEngine;

public class AIRouteBuilder : MonoBehaviour {

	[SerializeField] RaceMagager manager;
	[SerializeField] GameObject indicator;
	List<Vector3> GatePoints = new List<Vector3>();
	//http://www.gdcvault.com/play/1022016/Getting-off-the-NavMesh-Navigating

	void Start () {
		foreach(Gate gate in manager.GateList) {
			Vector3 pos = gate.transform.position;
			pos.y += gate.GetComponentInChildren<Collider>().bounds.size.y/2;
			GatePoints.Add(pos);
		}
		foreach(Vector3 location in GatePoints) {
			Instantiate(indicator, location, new Quaternion());
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
