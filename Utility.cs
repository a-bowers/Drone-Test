using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

public static class Utility
{
	public const int NavLayer = 1 << 10;

	public static void AddSorted<T>(this List<T> list, T value) {
		int x = list.BinarySearch(value);
		list.Insert((x >= 0) ? x : ~x, value);
	}
}


namespace UnityEngine
{
	public struct Vector3d
	{
		public const float kEpsilon = 1E-05f;
		public double x;
		public double y;
		public double z;

		public double this[int index] {
			get {
				switch(index) {
					case 0:
					return this.x;
					case 1:
					return this.y;
					case 2:
					return this.z;
					default:
					throw new IndexOutOfRangeException("Invalid index!");
				}
			}
			set {
				switch(index) {
					case 0:
					this.x = value;
					break;
					case 1:
					this.y = value;
					break;
					case 2:
					this.z = value;
					break;
					default:
					throw new IndexOutOfRangeException("Invalid Vector3d index!");
				}
			}
		}

		public Vector3d normalized {
			get {
				return Vector3d.Normalize(this);
			}
		}

		public double magnitude {
			get {
				return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
			}
		}

		public double sqrMagnitude {
			get {
				return this.x * this.x + this.y * this.y + this.z * this.z;
			}
		}

		public static Vector3d zero {
			get {
				return new Vector3d(0d, 0d, 0d);
			}
		}

		public static Vector3d one {
			get {
				return new Vector3d(1d, 1d, 1d);
			}
		}

		public static Vector3d forward {
			get {
				return new Vector3d(0d, 0d, 1d);
			}
		}

		public static Vector3d back {
			get {
				return new Vector3d(0d, 0d, -1d);
			}
		}

		public static Vector3d up {
			get {
				return new Vector3d(0d, 1d, 0d);
			}
		}

		public static Vector3d down {
			get {
				return new Vector3d(0d, -1d, 0d);
			}
		}

		public static Vector3d left {
			get {
				return new Vector3d(-1d, 0d, 0d);
			}
		}

		public static Vector3d right {
			get {
				return new Vector3d(1d, 0d, 0d);
			}
		}

		[Obsolete("Use Vector3d.forward instead.")]
		public static Vector3d fwd {
			get {
				return new Vector3d(0d, 0d, 1d);
			}
		}

		public Vector3d(double x, double y, double z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3d(float x, float y, float z) {
			this.x = (double)x;
			this.y = (double)y;
			this.z = (double)z;
		}

		public Vector3d(Vector3 v3) {
			this.x = (double)v3.x;
			this.y = (double)v3.y;
			this.z = (double)v3.z;
		}

		public Vector3d(double x, double y) {
			this.x = x;
			this.y = y;
			this.z = 0d;
		}

		public static Vector3d operator +(Vector3d a, Vector3d b) {
			return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3d operator -(Vector3d a, Vector3d b) {
			return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3d operator -(Vector3d a) {
			return new Vector3d(-a.x, -a.y, -a.z);
		}

		public static Vector3d operator *(Vector3d a, double d) {
			return new Vector3d(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3d operator *(double d, Vector3d a) {
			return new Vector3d(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3d operator /(Vector3d a, double d) {
			return new Vector3d(a.x / d, a.y / d, a.z / d);
		}

		public static bool operator ==(Vector3d lhs, Vector3d rhs) {
			return (double)Vector3d.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
		}

		public static bool operator !=(Vector3d lhs, Vector3d rhs) {
			return (double)Vector3d.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
		}

		public static explicit operator Vector3(Vector3d vector3d) {
			return new Vector3((float)vector3d.x, (float)vector3d.y, (float)vector3d.z);
		}

		public static Vector3d Lerp(Vector3d from, Vector3d to, double t) {
			t = Mathd.Clamp01(t);
			return new Vector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
		}

		public static Vector3d Slerp(Vector3d from, Vector3d to, double t) {
			Vector3 v3 = Vector3.Slerp((Vector3)from, (Vector3)to, (float)t);
			return new Vector3d(v3);
		}

		public static void OrthoNormalize(ref Vector3d normal, ref Vector3d tangent) {
			Vector3 v3normal = new Vector3();
			Vector3 v3tangent = new Vector3();
			v3normal = (Vector3)normal;
			v3tangent = (Vector3)tangent;
			Vector3.OrthoNormalize(ref v3normal, ref v3tangent);
			normal = new Vector3d(v3normal);
			tangent = new Vector3d(v3tangent);
		}

		public static void OrthoNormalize(ref Vector3d normal, ref Vector3d tangent, ref Vector3d binormal) {
			Vector3 v3normal = new Vector3();
			Vector3 v3tangent = new Vector3();
			Vector3 v3binormal = new Vector3();
			v3normal = (Vector3)normal;
			v3tangent = (Vector3)tangent;
			v3binormal = (Vector3)binormal;
			Vector3.OrthoNormalize(ref v3normal, ref v3tangent, ref v3binormal);
			normal = new Vector3d(v3normal);
			tangent = new Vector3d(v3tangent);
			binormal = new Vector3d(v3binormal);
		}

		public static Vector3d MoveTowards(Vector3d current, Vector3d target, double maxDistanceDelta) {
			Vector3d vector3 = target - current;
			double magnitude = vector3.magnitude;
			if(magnitude <= maxDistanceDelta || magnitude == 0.0d)
				return target;
			else
				return current + vector3 / magnitude * maxDistanceDelta;
		}

		public static Vector3d RotateTowards(Vector3d current, Vector3d target, double maxRadiansDelta, double maxMagnitudeDelta) {
			Vector3 v3 = Vector3.RotateTowards((Vector3)current, (Vector3)target, (float)maxRadiansDelta, (float)maxMagnitudeDelta);
			return new Vector3d(v3);
		}

		public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime, double maxSpeed) {
			double deltaTime = (double)Time.deltaTime;
			return Vector3d.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime) {
			double deltaTime = (double)Time.deltaTime;
			double maxSpeed = double.PositiveInfinity;
			return Vector3d.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
			smoothTime = Mathd.Max(0.0001d, smoothTime);
			double num1 = 2d / smoothTime;
			double num2 = num1 * deltaTime;
			double num3 = (1.0d / (1.0d + num2 + 0.479999989271164d * num2 * num2 + 0.234999999403954d * num2 * num2 * num2));
			Vector3d vector = current - target;
			Vector3d vector3_1 = target;
			double maxLength = maxSpeed * smoothTime;
			Vector3d vector3_2 = Vector3d.ClampMagnitude(vector, maxLength);
			target = current - vector3_2;
			Vector3d vector3_3 = (currentVelocity + num1 * vector3_2) * deltaTime;
			currentVelocity = (currentVelocity - num1 * vector3_3) * num3;
			Vector3d vector3_4 = target + (vector3_2 + vector3_3) * num3;
			if(Vector3d.Dot(vector3_1 - current, vector3_4 - vector3_1) > 0.0) {
				vector3_4 = vector3_1;
				currentVelocity = (vector3_4 - vector3_1) / deltaTime;
			}
			return vector3_4;
		}

		public void Set(double new_x, double new_y, double new_z) {
			this.x = new_x;
			this.y = new_y;
			this.z = new_z;
		}

		public static Vector3d Scale(Vector3d a, Vector3d b) {
			return new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public void Scale(Vector3d scale) {
			this.x *= scale.x;
			this.y *= scale.y;
			this.z *= scale.z;
		}

		public static Vector3d Cross(Vector3d lhs, Vector3d rhs) {
			return new Vector3d(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
		}

		public override int GetHashCode() {
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
		}

		public override bool Equals(object other) {
			if(!(other is Vector3d))
				return false;
			Vector3d vector3d = (Vector3d)other;
			if(this.x.Equals(vector3d.x) && this.y.Equals(vector3d.y))
				return this.z.Equals(vector3d.z);
			else
				return false;
		}

		public static Vector3d Reflect(Vector3d inDirection, Vector3d inNormal) {
			return -2d * Vector3d.Dot(inNormal, inDirection) * inNormal + inDirection;
		}

		public static Vector3d Normalize(Vector3d value) {
			double num = Vector3d.Magnitude(value);
			if(num > 9.99999974737875E-06)
				return value / num;
			else
				return Vector3d.zero;
		}

		public void Normalize() {
			double num = Vector3d.Magnitude(this);
			if(num > 9.99999974737875E-06)
				this = this / num;
			else
				this = Vector3d.zero;
		}
		// TODO : fix formatting
		public override string ToString() {
			return "(" + this.x + " - " + this.y + " - " + this.z + ")";
		}

		public static double Dot(Vector3d lhs, Vector3d rhs) {
			return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
		}

		public static Vector3d Project(Vector3d vector, Vector3d onNormal) {
			double num = Vector3d.Dot(onNormal, onNormal);
			if(num < 1.40129846432482E-45d)
				return Vector3d.zero;
			else
				return onNormal * Vector3d.Dot(vector, onNormal) / num;
		}

		public static Vector3d Exclude(Vector3d excludeThis, Vector3d fromThat) {
			return fromThat - Vector3d.Project(fromThat, excludeThis);
		}

		public static double Angle(Vector3d from, Vector3d to) {
			return Mathd.Acos(Mathd.Clamp(Vector3d.Dot(from.normalized, to.normalized), -1d, 1d)) * 57.29578d;
		}

		public static double Distance(Vector3d a, Vector3d b) {
			Vector3d vector3d = new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
			return Math.Sqrt(vector3d.x * vector3d.x + vector3d.y * vector3d.y + vector3d.z * vector3d.z);
		}

		public static Vector3d ClampMagnitude(Vector3d vector, double maxLength) {
			if(vector.sqrMagnitude > maxLength * maxLength)
				return vector.normalized * maxLength;
			else
				return vector;
		}

		public static double Magnitude(Vector3d a) {
			return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
		}

		public static double SqrMagnitude(Vector3d a) {
			return a.x * a.x + a.y * a.y + a.z * a.z;
		}

		public static Vector3d Min(Vector3d lhs, Vector3d rhs) {
			return new Vector3d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y), Mathd.Min(lhs.z, rhs.z));
		}

		public static Vector3d Max(Vector3d lhs, Vector3d rhs) {
			return new Vector3d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y), Mathd.Max(lhs.z, rhs.z));
		}

		[Obsolete("Use Vector3d.Angle instead. AngleBetween uses radians instead of degrees and was deprecated for this reason")]
		public static double AngleBetween(Vector3d from, Vector3d to) {
			return Mathd.Acos(Mathd.Clamp(Vector3d.Dot(from.normalized, to.normalized), -1d, 1d));
		}
	}


	public struct Mathd
	{
		public const double PI = 3.141593d;
		public const double Infinity = double.PositiveInfinity;
		public const double NegativeInfinity = double.NegativeInfinity;
		public const double Deg2Rad = 0.01745329d;
		public const double Rad2Deg = 57.29578d;
		public const double Epsilon = 1.401298E-45d;

		public static double Sin(double d) {
			return Math.Sin(d);
		}

		public static double Cos(double d) {
			return Math.Cos(d);
		}

		public static double Tan(double d) {
			return Math.Tan(d);
		}

		public static double Asin(double d) {
			return Math.Asin(d);
		}

		public static double Acos(double d) {
			return Math.Acos(d);
		}

		public static double Atan(double d) {
			return Math.Atan(d);
		}

		public static double Atan2(double y, double x) {
			return Math.Atan2(y, x);
		}

		public static double Sqrt(double d) {
			return Math.Sqrt(d);
		}

		public static double Abs(double d) {
			return Math.Abs(d);
		}

		public static int Abs(int value) {
			return Math.Abs(value);
		}

		public static double Min(double a, double b) {
			if(a < b)
				return a;
			else
				return b;
		}

		public static double Min(params double[] values) {
			int length = values.Length;
			if(length == 0)
				return 0.0d;
			double num = values[0];
			for(int index = 1; index < length; ++index) {
				if(values[index] < num)
					num = values[index];
			}
			return num;
		}

		public static int Min(int a, int b) {
			if(a < b)
				return a;
			else
				return b;
		}

		public static int Min(params int[] values) {
			int length = values.Length;
			if(length == 0)
				return 0;
			int num = values[0];
			for(int index = 1; index < length; ++index) {
				if(values[index] < num)
					num = values[index];
			}
			return num;
		}

		public static double Max(double a, double b) {
			if(a > b)
				return a;
			else
				return b;
		}

		public static double Max(params double[] values) {
			int length = values.Length;
			if(length == 0)
				return 0d;
			double num = values[0];
			for(int index = 1; index < length; ++index) {
				if((double)values[index] > (double)num)
					num = values[index];
			}
			return num;
		}

		public static int Max(int a, int b) {
			if(a > b)
				return a;
			else
				return b;
		}

		public static int Max(params int[] values) {
			int length = values.Length;
			if(length == 0)
				return 0;
			int num = values[0];
			for(int index = 1; index < length; ++index) {
				if(values[index] > num)
					num = values[index];
			}
			return num;
		}

		public static double Pow(double d, double p) {
			return Math.Pow(d, p);
		}

		public static double Exp(double power) {
			return Math.Exp(power);
		}

		public static double Log(double d, double p) {
			return Math.Log(d, p);
		}

		public static double Log(double d) {
			return Math.Log(d);
		}

		public static double Log10(double d) {
			return Math.Log10(d);
		}

		public static double Ceil(double d) {
			return Math.Ceiling(d);
		}

		public static double Floor(double d) {
			return Math.Floor(d);
		}

		public static double Round(double d) {
			return Math.Round(d);
		}

		public static int CeilToInt(double d) {
			return (int)Math.Ceiling(d);
		}

		public static int FloorToInt(double d) {
			return (int)Math.Floor(d);
		}

		public static int RoundToInt(double d) {
			return (int)Math.Round(d);
		}

		public static double Sign(double d) {
			return d >= 0.0 ? 1d : -1d;
		}

		public static double Clamp(double value, double min, double max) {
			if(value < min)
				value = min;
			else if(value > max)
				value = max;
			return value;
		}

		public static int Clamp(int value, int min, int max) {
			if(value < min)
				value = min;
			else if(value > max)
				value = max;
			return value;
		}

		public static double Clamp01(double value) {
			if(value < 0.0)
				return 0.0d;
			if(value > 1.0)
				return 1d;
			else
				return value;
		}

		public static double Lerp(double from, double to, double t) {
			return from + (to - from) * Mathd.Clamp01(t);
		}

		public static double LerpAngle(double a, double b, double t) {
			double num = Mathd.Repeat(b - a, 360d);
			if(num > 180.0d)
				num -= 360d;
			return a + num * Mathd.Clamp01(t);
		}

		public static double MoveTowards(double current, double target, double maxDelta) {
			if(Mathd.Abs(target - current) <= maxDelta)
				return target;
			else
				return current + Mathd.Sign(target - current) * maxDelta;
		}

		public static double MoveTowardsAngle(double current, double target, double maxDelta) {
			target = current + Mathd.DeltaAngle(current, target);
			return Mathd.MoveTowards(current, target, maxDelta);
		}

		public static double SmoothStep(double from, double to, double t) {
			t = Mathd.Clamp01(t);
			t = (-2.0 * t * t * t + 3.0 * t * t);
			return to * t + from * (1.0 - t);
		}

		public static double Gamma(double value, double absmax, double gamma) {
			bool flag = false;
			if(value < 0.0)
				flag = true;
			double num1 = Mathd.Abs(value);
			if(num1 > absmax) {
				if(flag)
					return -num1;
				else
					return num1;
			}
			else {
				double num2 = Mathd.Pow(num1 / absmax, gamma) * absmax;
				if(flag)
					return -num2;
				else
					return num2;
			}
		}

		public static bool Approximately(double a, double b) {
			return Mathd.Abs(b - a) < Mathd.Max(1E-06d * Mathd.Max(Mathd.Abs(a), Mathd.Abs(b)), 1.121039E-44d);
		}

		public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed) {
			double deltaTime = (double)Time.deltaTime;
			return Mathd.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime) {
			double deltaTime = Time.deltaTime;
			double maxSpeed = double.PositiveInfinity;
			return Mathd.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
			smoothTime = Mathd.Max(0.0001d, smoothTime);
			double num1 = 2d / smoothTime;
			double num2 = num1 * deltaTime;
			double num3 = (1.0d / (1.0d + num2 + 0.479999989271164d * num2 * num2 + 0.234999999403954d * num2 * num2 * num2));
			double num4 = current - target;
			double num5 = target;
			double max = maxSpeed * smoothTime;
			double num6 = Mathd.Clamp(num4, -max, max);
			target = current - num6;
			double num7 = (currentVelocity + num1 * num6) * deltaTime;
			currentVelocity = (currentVelocity - num1 * num7) * num3;
			double num8 = target + (num6 + num7) * num3;
			if(num5 - current > 0.0 == num8 > num5) {
				num8 = num5;
				currentVelocity = (num8 - num5) / deltaTime;
			}
			return num8;
		}

		public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed) {
			double deltaTime = (double)Time.deltaTime;
			return Mathd.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime) {
			double deltaTime = (double)Time.deltaTime;
			double maxSpeed = double.PositiveInfinity;
			return Mathd.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
			target = current + Mathd.DeltaAngle(current, target);
			return Mathd.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		public static double Repeat(double t, double length) {
			return t - Mathd.Floor(t / length) * length;
		}

		public static double PingPong(double t, double length) {
			t = Mathd.Repeat(t, length * 2d);
			return length - Mathd.Abs(t - length);
		}

		public static double InverseLerp(double from, double to, double value) {
			if(from < to) {
				if(value < from)
					return 0d;
				if(value > to)
					return 1d;
				value -= from;
				value /= to - from;
				return value;
			}
			else {
				if(from <= to)
					return 0d;
				if(value < to)
					return 1d;
				if(value > from)
					return 0d;
				else
					return (1.0d - (value - to) / (from - to));
			}
		}

		public static double DeltaAngle(double current, double target) {
			double num = Mathd.Repeat(target - current, 360d);
			if(num > 180.0d)
				num -= 360d;
			return num;
		}

		internal static bool LineIntersection(Vector2d p1, Vector2d p2, Vector2d p3, Vector2d p4, ref Vector2d result) {
			double num1 = p2.x - p1.x;
			double num2 = p2.y - p1.y;
			double num3 = p4.x - p3.x;
			double num4 = p4.y - p3.y;
			double num5 = num1 * num4 - num2 * num3;
			if(num5 == 0.0d)
				return false;
			double num6 = p3.x - p1.x;
			double num7 = p3.y - p1.y;
			double num8 = (num6 * num4 - num7 * num3) / num5;
			result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
			return true;
		}

		internal static bool LineSegmentIntersection(Vector2d p1, Vector2d p2, Vector2d p3, Vector2d p4, ref Vector2d result) {
			double num1 = p2.x - p1.x;
			double num2 = p2.y - p1.y;
			double num3 = p4.x - p3.x;
			double num4 = p4.y - p3.y;
			double num5 = (num1 * num4 - num2 * num3);
			if(num5 == 0.0d)
				return false;
			double num6 = p3.x - p1.x;
			double num7 = p3.y - p1.y;
			double num8 = (num6 * num4 - num7 * num3) / num5;
			if(num8 < 0.0d || num8 > 1.0d)
				return false;
			double num9 = (num6 * num2 - num7 * num1) / num5;
			if(num9 < 0.0d || num9 > 1.0d)
				return false;
			result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
			return true;
		}
	}

	public struct Vector2d
	{
		public const double kEpsilon = 1E-05d;
		public double x;
		public double y;

		public double this[int index] {
			get {
				switch(index) {
					case 0:
					return this.x;
					case 1:
					return this.y;
					default:
					throw new IndexOutOfRangeException("Invalid Vector2d index!");
				}
			}
			set {
				switch(index) {
					case 0:
					this.x = value;
					break;
					case 1:
					this.y = value;
					break;
					default:
					throw new IndexOutOfRangeException("Invalid Vector2d index!");
				}
			}
		}

		public Vector2d normalized {
			get {
				Vector2d vector2d = new Vector2d(this.x, this.y);
				vector2d.Normalize();
				return vector2d;
			}
		}

		public double magnitude {
			get {
				return Mathd.Sqrt(this.x * this.x + this.y * this.y);
			}
		}

		public double sqrMagnitude {
			get {
				return this.x * this.x + this.y * this.y;
			}
		}

		public static Vector2d zero {
			get {
				return new Vector2d(0.0d, 0.0d);
			}
		}

		public static Vector2d one {
			get {
				return new Vector2d(1d, 1d);
			}
		}

		public static Vector2d up {
			get {
				return new Vector2d(0.0d, 1d);
			}
		}

		public static Vector2d right {
			get {
				return new Vector2d(1d, 0.0d);
			}
		}

		public Vector2d(double x, double y) {
			this.x = x;
			this.y = y;
		}

		public static implicit operator Vector2d(Vector3d v) {
			return new Vector2d(v.x, v.y);
		}

		public static implicit operator Vector3d(Vector2d v) {
			return new Vector3d(v.x, v.y, 0.0d);
		}

		public static Vector2d operator +(Vector2d a, Vector2d b) {
			return new Vector2d(a.x + b.x, a.y + b.y);
		}

		public static Vector2d operator -(Vector2d a, Vector2d b) {
			return new Vector2d(a.x - b.x, a.y - b.y);
		}

		public static Vector2d operator -(Vector2d a) {
			return new Vector2d(-a.x, -a.y);
		}

		public static Vector2d operator *(Vector2d a, double d) {
			return new Vector2d(a.x * d, a.y * d);
		}

		public static Vector2d operator *(float d, Vector2d a) {
			return new Vector2d(a.x * d, a.y * d);
		}

		public static Vector2d operator /(Vector2d a, double d) {
			return new Vector2d(a.x / d, a.y / d);
		}

		public static bool operator ==(Vector2d lhs, Vector2d rhs) {
			return Vector2d.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
		}

		public static bool operator !=(Vector2d lhs, Vector2d rhs) {
			return (double)Vector2d.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
		}

		public void Set(double new_x, double new_y) {
			this.x = new_x;
			this.y = new_y;
		}

		public static Vector2d Lerp(Vector2d from, Vector2d to, double t) {
			t = Mathd.Clamp01(t);
			return new Vector2d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
		}

		public static Vector2d MoveTowards(Vector2d current, Vector2d target, double maxDistanceDelta) {
			Vector2d vector2 = target - current;
			double magnitude = vector2.magnitude;
			if(magnitude <= maxDistanceDelta || magnitude == 0.0d)
				return target;
			else
				return current + vector2 / magnitude * maxDistanceDelta;
		}

		public static Vector2d Scale(Vector2d a, Vector2d b) {
			return new Vector2d(a.x * b.x, a.y * b.y);
		}

		public void Scale(Vector2d scale) {
			this.x *= scale.x;
			this.y *= scale.y;
		}

		public void Normalize() {
			double magnitude = this.magnitude;
			if(magnitude > 9.99999974737875E-06)
				this = this / magnitude;
			else
				this = Vector2d.zero;
		}

		public override string ToString() {
			/*
      string fmt = "({0:D1}, {1:D1})";
      object[] objArray = new object[2];
      int index1 = 0;
      // ISSUE: variable of a boxed type
      __Boxed<double> local1 = (ValueType) this.x;
      objArray[index1] = (object) local1;
      int index2 = 1;
      // ISSUE: variable of a boxed type
      __Boxed<double> local2 = (ValueType) this.y;
      objArray[index2] = (object) local2;
      */
			return "not implemented";
		}

		public string ToString(string format) {
			/* TODO:
      string fmt = "({0}, {1})";
      object[] objArray = new object[2];
      int index1 = 0;
      string str1 = this.x.ToString(format);
      objArray[index1] = (object) str1;
      int index2 = 1;
      string str2 = this.y.ToString(format);
      objArray[index2] = (object) str2;
      */
			return "not implemented";
		}

		public override int GetHashCode() {
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
		}

		public override bool Equals(object other) {
			if(!(other is Vector2d))
				return false;
			Vector2d vector2d = (Vector2d)other;
			if(this.x.Equals(vector2d.x))
				return this.y.Equals(vector2d.y);
			else
				return false;
		}

		public static double Dot(Vector2d lhs, Vector2d rhs) {
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static double Angle(Vector2d from, Vector2d to) {
			return Mathd.Acos(Mathd.Clamp(Vector2d.Dot(from.normalized, to.normalized), -1d, 1d)) * 57.29578d;
		}

		public static double Distance(Vector2d a, Vector2d b) {
			return (a - b).magnitude;
		}

		public static Vector2d ClampMagnitude(Vector2d vector, double maxLength) {
			if(vector.sqrMagnitude > maxLength * maxLength)
				return vector.normalized * maxLength;
			else
				return vector;
		}

		public static double SqrMagnitude(Vector2d a) {
			return (a.x * a.x + a.y * a.y);
		}

		public double SqrMagnitude() {
			return (this.x * this.x + this.y * this.y);
		}

		public static Vector2d Min(Vector2d lhs, Vector2d rhs) {
			return new Vector2d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y));
		}

		public static Vector2d Max(Vector2d lhs, Vector2d rhs) {
			return new Vector2d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y));
		}
	}
}
