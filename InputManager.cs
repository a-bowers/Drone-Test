using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
	public enum InputDevice
	{
		Keyboard,
		Controller
	}

	public struct FrameInput
	{
		public float Pitch, Yaw, Roll, Throttle;
		public ControlMode FlightMode;
		public bool BaroMode;

		public void Reset() {
			Pitch = Yaw = Roll = Throttle = 0;
			FlightMode = ControlMode.STAB;
			BaroMode = true;
		}
	}

	InputDevice inputDevice = InputDevice.Controller;
	public FrameInput currentInput;
	Vector3 mouseOrigin;
	[SerializeField] Image MouseOriginPic;
	[SerializeField] float PitchSensitivity = 1;
	[SerializeField] float RollSensitivity = 1;
	[SerializeField] float YawSensitivity = 1;
	[SerializeField] float ThrottleSensitivity = 1;
	ControlMode flightMode = ControlMode.STAB;
	bool baroMode = true;

	void Start() {
		ResetMouseOrigin();
	}

	void Update() {
		currentInput = new FrameInput();
		//TODO sensitivity values
		//take input for player
		if(Input.GetKey(KeyCode.C)) //when a key is pushed, reset the mouse
			ResetMouseOrigin();
		if(Input.GetButtonUp("FlightMode")) //change from ACRO to STAB mode
			flightMode = flightMode == ControlMode.ACRO ? ControlMode.STAB : ControlMode.ACRO;
		if(Input.GetButtonUp("BaroMode"))//toggles barometric mode
			baroMode = !baroMode;

		currentInput.BaroMode = baroMode;
		currentInput.FlightMode = flightMode;

		//Throttle and Yaw
		currentInput.Throttle = Input.GetAxis("Vertical")*ThrottleSensitivity;
		currentInput.Yaw = Input.GetAxis("Horizontal")*YawSensitivity;

		//Roll and Pitch inputs
		if(inputDevice.Equals(InputDevice.Keyboard)) {
			Vector3 mousePosition = Input.mousePosition; //TODO can be uneven if mouse origin is not very centered
			currentInput.Roll = Mathf.Clamp((mousePosition - mouseOrigin).x/Screen.width*2, -1, 1)*RollSensitivity;
			currentInput.Pitch = Mathf.Clamp((mouseOrigin - mousePosition).y/Screen.height*2, -1, 1)*PitchSensitivity;
		}
		else if(inputDevice.Equals(InputDevice.Controller)) {
			currentInput.Roll = Input.GetAxis("PadRoll")*RollSensitivity;
			currentInput.Pitch = Input.GetAxis("PadPitch")*PitchSensitivity;
		}
	}

	void ResetMouseOrigin() {
		mouseOrigin = Input.mousePosition;
		MouseOriginPic.rectTransform.position = mouseOrigin;
	}
}

public enum ControlMode
{
	ACRO,
	STAB
}

