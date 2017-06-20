public interface IDroneInput
{
	FrameInput CurrentInput { get; }
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

