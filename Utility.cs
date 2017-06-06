using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Utility
{

	//USE Mathf.Clamp instead
	public static float Restrict(float number, float maxLimit, float minLimit = float.NaN) {
		if(number > maxLimit)
			return maxLimit;
		if(!float.IsNaN(minLimit) && number < minLimit)
			return minLimit;
		return number;
	}

	//public static float Restrict(float number, float maxLimit, float minLimit = float.NaN) {
	//	return number > maxLimit ? maxLimit : (!float.IsNaN(minLimit) && number < minLimit) ? minLimit : number;
	//}

	public static int Restrict(int number, float maxLimit, float minLimit = float.NaN) {
		if(number > maxLimit)
			return (int)maxLimit;
		if(!float.IsNaN(minLimit) && number < minLimit)
			return (int)minLimit;
		return number;
	}
}
