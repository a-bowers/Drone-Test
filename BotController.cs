using System;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour, IDroneInput {

	public FrameInput CurrentInput { get { return currentInput; } }
	FrameInput currentInput;
	public PlayerController drone;
	public List<Vector3> waypointList;
	int waypointIndex;
	//Vector3 nextWaypoint { get { return waypointList[waypointIndex]; } }
	Vector3 nextWaypoint { get { return waypoint.transform.position; } } //For testing
	[SerializeField] GameObject waypoint;

	Dictionary<PIDs, PID> pids = new Dictionary<PIDs, PID>();
	float tolerance = 0.2f; //waypoint tolerance in m
	public bool tracking;

	void Start () {
		foreach(PIDs item in Enum.GetValues(typeof(PIDs)))
			pids.Add(item, new PID());

		pids[PIDs.PITCH].kP = 1f;
		pids[PIDs.ROLL].kP = 1f;
		pids[PIDs.THRUST].kP = .1f;
	}
	
	void Update () {
		if(!tracking) return;
		if(WaypointReached())
			if(++waypointIndex > waypointList.Count - 1)
				waypointIndex = 0; //reset the index if it reaches the end of the list

		currentInput = new FrameInput();
		currentInput.Reset(); //change to STAB mode and baromode = true

		//TODO FIXME yaw turning causes altitude gain
		//If we do not face the target, turn
		Vector3 proj = Vector3.ProjectOnPlane(drone.transform.position - nextWaypoint, Vector3.up);
		float angle = Vector3.Angle(proj, Vector3.right);
		drone.setYaw = Vector3.Angle(proj, Vector3.back) < 90 ? angle : -angle;

		//If we are not on the same height as the target, adjust throttle
		currentInput.Throttle = Mathf.Clamp(pids[PIDs.THRUST].GetPID(nextWaypoint.y - drone.setAltitude), -1, 1);

		//pitch and roll, depending on yaw

	}

	bool WaypointReached() {
		return (drone.transform.position - nextWaypoint).sqrMagnitude < tolerance*tolerance;
	}

	float AngleDifference180(float a, float b) {
		a = b - a;
		a += a > 180 ? -360 : a < -180 ? 360 : 0;
		return a;
	}

	enum PIDs
	{
		PITCH,
		ROLL,
		YAW,
		THRUST
	}
}
