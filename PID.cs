using UnityEngine;

class PID
{
	public float P, I, D;
	public float kP, kI, kD, imax;
	float integrator, lastError, lastDerivative, lastT;
	int fCut; //frequency cutoff for derivative lowpass filter

	public PID(float initialP = 0, float initialI = 0, float initialD = 0, float initialMax = 0) {
		kP = initialP;
		kI = initialI;
		kD = initialD;
		imax = initialMax;
		lastT = 0;
		fCut = 20;
		integrator = 0;
		lastError = lastDerivative = float.NaN;
		P = I = D = 0;
	}

	public float GetPID(float error, float scalar = 1) {
		float dt = Time.time - lastT; //TODO check about using this class for dt calcs

		float output = 0;
		if(float.IsNaN(lastT) || dt > 1) { //in seconds
			dt = 0;
			ResetI();
		}

		lastT = Time.time;

		//P
		P = error*kP;
		output += P;

		//D
		if(Mathf.Abs(kD) > 0 && dt > 0) {
			float derivative;

			if(float.IsNaN(lastDerivative)) {
				derivative = 0;
				lastDerivative = 0;
			}
			else
				derivative = (error - lastError)/dt;


			float RC = 1/(2*Mathf.PI*fCut);
			derivative = lastDerivative + ((dt/(RC + dt))*(derivative - lastDerivative));

			lastError = error;
			lastDerivative = derivative;

			D = kD*derivative;
			output += D;
		}

		output *= scalar;
		P *= scalar;
		D *= scalar;

		//I
		if(Mathf.Abs(kI) > 0 && dt > 0) {
			integrator += (error*kI)*scalar*dt;
			I = Mathf.Clamp(integrator, -imax, imax);
			output += I;
		}

		return output;
	}

	void ResetI() {
		integrator = 0;
		lastDerivative = float.NaN;
		I = 0;
	}
}
