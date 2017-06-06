using UnityEngine;

[ExecuteInEditMode]
public class GateSizer : MonoBehaviour {

	[SerializeField] float Width = 1, Height = 1;
	float prevWidth = 0, prevHeight = 0;
	[SerializeField] GameObject LeftPost = null, RightPost = null, FrontCollider = null, BackCollider = null;

	void Update () {
		if(Width == prevWidth && Height == prevHeight)
			return;

		Vector3 scale = new Vector3(Width, Height, 1);

		FrontCollider.transform.localScale = Vector3.Scale(FrontCollider.transform.localScale, scale);
		BackCollider.transform.localScale = Vector3.Scale(BackCollider.transform.localScale, scale);

		scale = new Vector3(Width/2, 1, 1);

		LeftPost.transform.position = Vector3.Scale(LeftPost.transform.position, scale);
		//TODO finish

		prevWidth = Width;
		prevHeight = Height;
	}
}
