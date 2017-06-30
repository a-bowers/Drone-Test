using System;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour, IDroneInput {

	public FrameInput CurrentInput { get { return currentInput; } }
	FrameInput currentInput;
	public PlayerController drone;
	public List<GameObject> waypointList;
	int waypointIndex; //the index of the current target waypoint
	Vector3 NextWaypoint { get { return waypointList[waypointIndex].transform.position; } }
	Vector3 SecondNextWaypoint {
		get {
			if(waypointIndex >= waypointList.Count - 1)
				return waypointList[0].transform.position;
			return waypointList[waypointIndex + 1].transform.position;
		}
	}
	//Vector3 nextWaypoint { get { return waypoint.transform.position; } } //For testing
	[SerializeField] GameObject waypoint;

	Dictionary<PIDs, PID> pids = new Dictionary<PIDs, PID>();
	float tolerance = 1f; //waypoint tolerance in m
	public bool tracking;
	public float clampLimit = 1;

	void Start () {
		foreach(PIDs item in Enum.GetValues(typeof(PIDs)))
			pids.Add(item, new PID());

		pids[PIDs.PITCH].kP = 1.0f;
		pids[PIDs.ROLL].kP = 1.0f;
		pids[PIDs.THRUST].kP = .1f;

		pids[PIDs.PITCH].kD = 0.1f;
		pids[PIDs.ROLL].kD = 0.1f;
		pids[PIDs.THRUST].kD = 0;

		pids[PIDs.PITCH].kI = 5.5f;
		pids[PIDs.ROLL].kI = 1.5f;
		pids[PIDs.THRUST].kI = 0;
	}
	
	void Update () {
		if(!tracking) return;
		if(WaypointReached())
			if(++waypointIndex > waypointList.Count - 1)
				waypointIndex = 0; //reset the index if it reaches the end of the list

		currentInput = new FrameInput();
		currentInput.Reset(); //change to STAB mode and baromode = true

		//If we do not face the target, turn
		Vector3 proj = Vector3.ProjectOnPlane(NextWaypoint - drone.transform.position, Vector3.up);
		float angle = Vector3.Angle(proj, -Vector3.right);
		drone.setYaw = Vector3.Angle(proj, Vector3.back) < 90 ? -angle : angle;

		//If we are not on the same height as the target, adjust throttle
		currentInput.Throttle = Mathf.Clamp(pids[PIDs.THRUST].GetPID(NextWaypoint.y - drone.setAltitude), -1, 1);

		//pitch and roll to adjust the horizontal velocity vector
		//Vector3 addedVector = Vector3.ClampMagnitude(SecondNextWaypoint - NextWaypoint, 2.5f)*Mathf.Clamp01(1 - proj.magnitude/4);
		//Vector3 diff = (proj + 2*proj.normalized + addedVector);

		//Vector3 addedVector = (SecondNextWaypoint - NextWaypoint).normalized*Mathf.Clamp01(1 - proj.magnitude/4);
		Vector3 diff = proj.normalized*4;
		diff -= Vector3.ProjectOnPlane(drone.Velocity, Vector3.up);
		diff = drone.transform.InverseTransformDirection(diff);
		//clampLimit = Mathf.Clamp(10/diff.magnitude + .3f, -1, 1);
		currentInput.Pitch = Mathf.Clamp(pids[PIDs.PITCH].GetPID(diff.x), -clampLimit, clampLimit);
		currentInput.Roll = Mathf.Clamp(pids[PIDs.ROLL].GetPID(diff.z), -clampLimit, clampLimit);
		DebugExtension.DebugArrow(drone.transform.position, proj, Color.blue);
		DebugExtension.DebugArrow(drone.transform.position, drone.Velocity, Color.red);

	}

	bool WaypointReached() {
		return (drone.transform.position - NextWaypoint).sqrMagnitude < tolerance*tolerance;
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
