using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	//http://blog.owenson.me/build-your-own-quadcopter-flight-controller/
	//http://sal.aalto.fi/publications/pdf-files/eluu11_public.pdf
	//http://andrew.gibiansky.com/blog/physics/quadcopter-dynamics/

	//[SerializeField] IDroneInput inputManager; //control input source
	//[SerializeField] BotController inputManager;
	[SerializeField] InputManager inputManager;
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
			return force/props[0].thrustMultiplier/props.Length/props[0].MaxSpeed; //transformed into %MaxRPM per prop
		}
	}

	public bool Inverted { get { return transform.up.y < 0; } }

	public float Pitch { get { return -Mathf.Asin(transform.right.y)*Mathf.Rad2Deg; } }
	public float Roll { 
		get { 
			float angle = -Mathf.Asin(transform.forward.y)*Mathf.Rad2Deg;
			return Inverted ? Mathf.Sign(angle)*180 - angle : angle; 
		}
	}
	public float Yaw { 
		get {
			Vector3 proj = Vector3.ProjectOnPlane(transform.right, Vector3.up);
			float angle = Vector3.Angle(proj, Vector3.right);
			return Vector3.Angle(proj, Vector3.back) < 90 ? angle : -angle;
		}
	}

	public float Altitude { get { return transform.position.y; } }
	public Vector3 Velocity { get { return rb.velocity; } }

	//mode angular rates in deg/s
	float ACRO_PITCH_RATE = 150f;
	float ACRO_ROLL_RATE = 150f;
	float ACRO_YAW_RATE = 100f;
	float STAB_YAW_RATE = 100f;

	//STAB mode maximum angles in deg
	float STAB_PITCH_MAX = 60;
	float STAB_ROLL_MAX = 60;

	float BARO_RATE = 10f; //Rate of change in m/s of altitude at full throttle in baro mode

	void Start() {
		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = 50;

		setYaw = Yaw;

		baseThrottle = props[0].MaxSpeed*baseThrottlePercent;
		maxThrottle = props[0].MaxSpeed*maxThrottlePercent;

		foreach(PIDs item in Enum.GetValues(typeof(PIDs))) {
			pids.Add(item, new PID());
		}
		//TODO adjust these
		pids[PIDs.PITCH_RATE].kP = 0.0003f;
		pids[PIDs.PITCH_RATE].kI = 0.00f;
		pids[PIDs.PITCH_RATE].imax = 10;

		pids[PIDs.ROLL_RATE].kP = 0.0003f;
		pids[PIDs.ROLL_RATE].kI = 0.00f;
		pids[PIDs.ROLL_RATE].imax = 10;

		pids[PIDs.YAW_RATE].kP = 0.001f;
		pids[PIDs.YAW_RATE].kI = 0.00f;
		//pids[PIDs.YAW_RATE].kD = 1.00f;
		pids[PIDs.YAW_RATE].imax = 10;

		pids[PIDs.PITCH_STAB].kP = 4f;
		pids[PIDs.PITCH_STAB].kD = .5f;

		pids[PIDs.ROLL_STAB].kP = 4f;
		pids[PIDs.ROLL_STAB].kD = .5f;

		pids[PIDs.YAW_STAB].kP = 40f;
		pids[PIDs.YAW_STAB].kD = 20f;

		pids[PIDs.BARO].kP = 0.09f;
		pids[PIDs.BARO].kI = 0.00f;
		pids[PIDs.BARO].kD = 0.08f;
	}

	void Update () {
		input = inputManager.CurrentInput;

		//Update flight modes
		if(flightMode != input.FlightMode) {
			if(input.FlightMode == ControlMode.STAB)
				setYaw = Yaw; //if turning stab mode on, set yaw to current
			flightMode = input.FlightMode;
		}
		if(baroMode != input.BaroMode) {
			setAltitude = Altitude; //if turning baro mode on, set altitude to current
			baroMode = input.BaroMode;
		}

		flightModeText.text = string.Format("{0}{1}", baroMode ? "BARO\n" : "", flightMode.ToString());

		//Gyro information
		Vector3 currentGyro = Mathf.Rad2Deg*transform.InverseTransformDirection(rb.angularVelocity);
		currentGyro.z = -currentGyro.z;

		velocityText.text = string.Format("({0:F3}, {1:F3})\nVert.: {2:f3}", Velocity.x, Velocity.z, Velocity.y);
		orientationText.text =  string.Format("Actual: {0:F3}\nSet: {1:F3}\nPitch: {2:F5}\nRoll: {3:F5}\nYaw: {4:F5}\nSet: {5:F5}",
												Altitude, setAltitude, Pitch, Roll, Yaw, setYaw);

		//Throttle
		if(baroMode) {
			setAltitude += input.Throttle*BARO_RATE*Time.deltaTime;
			if(Inverted)
				throttle = 0; //No base throttle if flipped
			else {
				float pid = pids[PIDs.BARO].GetPID(setAltitude - Altitude);
				throttle = Mathf.Clamp(hoverThrottle + pid, 0, 1);
			}
		}
		else
			throttle = Mathf.Clamp(input.Throttle, 0, 1);

		//Pitch, roll, and yaw PIDs
		float pitchOut = 0, rollOut = 0, yawOut = 0;

		//TODO FIXME yaw turning (reversing direction specifically) causes altitude gain in stab mode
		if(flightMode == ControlMode.STAB) {
			//FIXME get these values working so that they can wrap with the pids //Not necessary except for exceptional cases
			//TODO Autolevel if upside-down in stab mode
			float pitch_stab_output = pids[PIDs.PITCH_STAB].GetPID(input.Pitch*STAB_PITCH_MAX - Pitch);
			float roll_stab_output = pids[PIDs.ROLL_STAB].GetPID(input.Roll*STAB_ROLL_MAX - Roll);
			float yaw_stab_output;
			if(input.Yaw != 0) {
				yaw_stab_output = input.Yaw*STAB_YAW_RATE;
				setYaw = Yaw; //TODO change this to be ahead by an amount depending on the turn rate to remove slingshotting
			} else {
				yaw_stab_output = pids[PIDs.YAW_STAB].GetPID(AngleDifference180(Yaw, setYaw));
			}

			pitchOut = pids[PIDs.PITCH_RATE].GetPID(currentGyro.z - pitch_stab_output);
			rollOut = pids[PIDs.ROLL_RATE].GetPID(currentGyro.x - roll_stab_output);
			yawOut = pids[PIDs.YAW_RATE].GetPID(currentGyro.y - yaw_stab_output);
		}
		else if(flightMode == ControlMode.ACRO) {
			pitchOut = pids[PIDs.PITCH_RATE].GetPID(currentGyro.z - input.Pitch*ACRO_PITCH_RATE);
			rollOut = pids[PIDs.ROLL_RATE].GetPID(currentGyro.x - input.Roll*ACRO_ROLL_RATE);
			yawOut = pids[PIDs.YAW_RATE].GetPID(currentGyro.y - input.Yaw*ACRO_YAW_RATE);
		}

		debugText.text = string.Format("X: {0:F3}\nY: {1:F3}", input.Roll, input.Pitch);
		throttleText.text = string.Format("Throttle: {0:F0}\tPitchOut: {1:F5}\tRollOut: {2:F5}\tYawOut:{3:F5}", throttle, pitchOut, rollOut, yawOut);

		//Mixer

		//	  ^
		//	  |
		//	2	1
		//	  X
		//	3	0
		//

		float[][] mixTable = {
			new float[] { 1.00f, 1.00f, 1.00f, -1.00f },
			new float[] { 1.00f, 1.00f, -1.00f, 1.00f },
			new float[] { 1.00f, -1.00f, -1.00f, -1.00f },
			new float[] { 1.00f, -1.00f, 1.00f, 1.00f },
		};

		float[] thrust = new float[4];
		//y tilts forewards and back, x left and right

		for(int i = 0; i < 4; i++) {
			float[] mix = mixTable[i];
			thrust[i] = Mathf.Clamp(mix[0]*throttle + mix[1]*rollOut + mix[2]*pitchOut + mix[3]*yawOut, 0, 1);
		}

		mousePosText.text = string.Format("{0:F3}\t\t{1:F3}\n{0:F3}\t\t{1:F3}",
											thrust[2], thrust[1],
											thrust[3], thrust[0]);

		for(int i = 0; i < props.Length; i++) { //Set speed of each propeller
			props[i].SetSpeedPercent(thrust[i]);
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
