using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	//http://blog.owenson.me/build-your-own-quadcopter-flight-controller/
	//http://sal.aalto.fi/publications/pdf-files/eluu11_public.pdf
	//http://andrew.gibiansky.com/blog/physics/quadcopter-dynamics/

	//[SerializeField] IDroneInput inputManager; //control input source
	[SerializeField] BotController inputManager;
	//[SerializeField] InputManager inputManager;
	FrameInput input; //The input for the frame

	[SerializeField] Propeller[] props = new Propeller[4];
	[SerializeField] float baseThrottlePercent = .02f, maxThrottlePercent = .9f;
	float baseThrottle, maxThrottle;
	public Text throttleText, mousePosText, velocityText, orientationText, flightModeText, debugText;
	
	Rigidbody rb;
	Dictionary<PIDs, PID> pids = new Dictionary<PIDs, PID>();
	float throttle = 0;
	ControlMode flightMode = ControlMode.STAB;
	bool baroMode = true;
	public float setAltitude = 1;
	public float setYaw {
		get { return _setYaw; }
		set { _setYaw = value > 180 ? value - 360 : value < -180 ? value + 360 : value; }
	}
	float _setYaw = 0;

	float hoverThrottle {
		get {
			float angleMod = Mathf.Cos(Mathf.Deg2Rad*Vector3.Angle(transform.up, -Physics.gravity));//Angle b/w up vector and world up
			float force = rb.mass*Mathf.Abs(Physics.gravity.y)/angleMod; //The force/prop required to maintain altitude
			return force/props[0].GetComponent<Propeller>().thrustMultiplier/props.Length; //transformed into rpm per prop
		}
	}

	//TODO These have limits as they always return the smallest angle (never reflex)
	public float Pitch { get { return -(Vector3.Angle(transform.right, Physics.gravity) - 90); } }
	public float Roll { get { return -(Vector3.Angle(transform.forward, Physics.gravity) - 90); } }
	public float Yaw { get {
			Vector3 proj = Vector3.ProjectOnPlane(transform.right, Vector3.up);
			float angle = Vector3.Angle(proj, Vector3.right);
			return Vector3.Angle(proj, Vector3.back) < 90 ? angle : -angle;
		}
	}

	public float Altitude { get { return transform.position.y; } } //TODO Modify this to dist over terrain?

	//mode angular rates in deg/s
	float ACRO_PITCH_RATE = 70;
	float ACRO_ROLL_RATE = 70;
	float ACRO_YAW_RATE = 200;
	float STAB_YAW_RATE = 400;

	//STAB mode maximum angles in deg
	float STAB_PITCH_MAX = 60;
	float STAB_ROLL_MAX = 60;

	float BARO_RATE = 7f; //Rate of change in m/s of altitude at full throttle in baro mode

	void Start() {
		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = 50;

		baseThrottle = props[0].GetComponent<Propeller>().MaxSpeed*baseThrottlePercent;
		maxThrottle = props[0].GetComponent<Propeller>().MaxSpeed*maxThrottlePercent;

		foreach(PIDs item in Enum.GetValues(typeof(PIDs))) {
			pids.Add(item, new PID());
		}
		//TODO adjust these
		pids[PIDs.PITCH_RATE].kP = 3.0f;
		pids[PIDs.PITCH_RATE].kI = 0.00f;
		pids[PIDs.PITCH_RATE].imax = 10;

		pids[PIDs.ROLL_RATE].kP = 3.0f;
		pids[PIDs.ROLL_RATE].kI = 0.00f;
		pids[PIDs.ROLL_RATE].imax = 10;

		pids[PIDs.YAW_RATE].kP = 2.5f;
		pids[PIDs.YAW_RATE].kI = 0.00f;
		pids[PIDs.YAW_RATE].imax = 50;

		pids[PIDs.PITCH_STAB].kP = 5f;
		pids[PIDs.PITCH_STAB].kD = .5f;

		pids[PIDs.ROLL_STAB].kP = 5f;
		pids[PIDs.ROLL_STAB].kD = .5f;

		pids[PIDs.YAW_STAB].kP = 50f;
		pids[PIDs.YAW_STAB].kD = 20f;

		pids[PIDs.BARO].kP = .5f;
		pids[PIDs.BARO].kI = .0f;
		pids[PIDs.BARO].kD = 1f;
	}

	void Update () {
		input = inputManager.CurrentInput;
		if(flightMode != input.FlightMode) {
			if(input.FlightMode == ControlMode.STAB)
				setYaw = Yaw; //if turning stab mode on, set yaw to current
			flightMode = input.FlightMode;
		}
		flightModeText.text = input.FlightMode.ToString();
		if(baroMode != input.BaroMode) {
			setAltitude = Altitude; //if turning baro mode on, set altitude to current
			baroMode = input.BaroMode;
		}

		Vector3 currentGyro = Mathf.Rad2Deg*transform.InverseTransformDirection(rb.angularVelocity);
		currentGyro.z = -currentGyro.z;
		velocityText.text = "(" + rb.velocity.x + ", " + rb.velocity.z + ")\nVert.: " + rb.velocity.y;
		orientationText.text =  "Actual: " + Altitude + "\nSet: "+ setAltitude + "\nPitch: "
			+ Pitch + "\nRoll: " + Roll + "\nYaw: " + Yaw + "\nSet: " + setYaw;

		if(baroMode) {
			setAltitude += input.Throttle*BARO_RATE*Time.deltaTime;
			float pid = pids[PIDs.BARO].GetPID(setAltitude - Altitude , maxThrottle/20);
			throttle = Mathf.Clamp(hoverThrottle + pid, baseThrottle, maxThrottle);
		}
		else
			throttle = input.Throttle*maxThrottle*1;
		//throttleText.text = " Throttle: " + throttle;

		//calculate output of PIDs
		float pitchOut = 0, rollOut = 0, yawOut = 0;

		if(flightMode == ControlMode.STAB) {
			//FIXME get these values working so that they can wrap with the pids //Not necessary except for exceptional cases
			float pitch_stab_output = pids[PIDs.PITCH_STAB].GetPID(input.Pitch*STAB_PITCH_MAX - Pitch, 1f);
			float roll_stab_output = pids[PIDs.ROLL_STAB].GetPID(input.Roll*STAB_ROLL_MAX - Roll, 1f);
			float yaw_stab_output;
			if(input.Yaw != 0) {
				yaw_stab_output = input.Yaw*STAB_YAW_RATE;
				setYaw = Yaw;
			} else {
				yaw_stab_output = pids[PIDs.YAW_STAB].GetPID(AngleDifference180(Yaw, setYaw), 1f);
			}
			debugText.text = "X: " + input.Roll + "\nY: " + input.Pitch;

			pitchOut = pids[PIDs.PITCH_RATE].GetPID(currentGyro.z - pitch_stab_output, 1f);
			rollOut = pids[PIDs.ROLL_RATE].GetPID(currentGyro.x - roll_stab_output, 1f);
			yawOut = pids[PIDs.YAW_RATE].GetPID(currentGyro.y - yaw_stab_output, 1f);
		}
		else if(flightMode == ControlMode.ACRO) {
			pitchOut = pids[PIDs.PITCH_RATE].GetPID(currentGyro.z - input.Pitch*ACRO_PITCH_RATE, 1f);
			rollOut = pids[PIDs.ROLL_RATE].GetPID(currentGyro.x - input.Roll*ACRO_ROLL_RATE, 1f);
			yawOut = pids[PIDs.YAW_RATE].GetPID(currentGyro.y - input.Yaw*ACRO_YAW_RATE, 1f);
		}

		throttleText.text = "Throttle: " + throttle + " PitchOut: " + pitchOut + " RollOut: " + rollOut + " YawOut: " + yawOut;

		//	  ^
		//	  |
		//	2	1
		//	  X
		//	3	0
		//

		float[] thrust = new float[4];
		//y tilts forewards and back, x left and right

		if(throttle > baseThrottle) {
			thrust[0] = throttle + rollOut + pitchOut - yawOut;
			thrust[1] = throttle + rollOut - pitchOut + yawOut;
			thrust[2] = throttle - rollOut - pitchOut - yawOut;
			thrust[3] = throttle - rollOut + pitchOut + yawOut;
			mousePosText.text = thrust[2] + "\t\t" + thrust[1] + "\n" + thrust[3] + "\t\t" + thrust[0];
		}
		else {
			thrust[0] = thrust[1] = thrust[2] = thrust[3] = 0;
		}

		for(int i = 0; i < props.Length; i++) { //Set RPM of each propeller
			props[i].SetSpeed(thrust[i]);
		}
	}

	float AngleDifference180(float a, float b) {
		a = b - a;
		a += a > 180 ? -360 : a < -180 ? 360 : 0;
		return a;
	}

	enum PIDs {
		PITCH_RATE,
		ROLL_RATE,
		YAW_RATE,
		PITCH_STAB,
		ROLL_STAB,
		YAW_STAB,
		BARO
	}
}
