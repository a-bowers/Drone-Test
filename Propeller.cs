using UnityEngine;

public class Propeller : MonoBehaviour {

	[SerializeField] public float thrustMultiplier, torqueMultiplier;
	float rpm;
	[SerializeField] public float MaxSpeed;
	[SerializeField] bool CCW;

	[SerializeField] Rigidbody rb;

	public void SetSpeed(float rpm) {
		this.rpm = Mathf.Clamp(rpm, 0, MaxSpeed);
	}

	public void SetSpeedPercent(float percent) {
		SetSpeed(percent*MaxSpeed);
	}

	public void ChangeSpeed(float delta) {
		SetSpeed(rpm + delta);
	}

	void FixedUpdate() {
		Quaternion angle = GetComponent<Transform>().rotation;
		Vector3 force = new Vector3(0, thrustMultiplier*rpm, 0);

		rb.AddForceAtPosition(angle*force, transform.position);
		rb.AddRelativeTorque(0, rpm*torqueMultiplier*(CCW ? 1 : -1), 0);
	}
}
