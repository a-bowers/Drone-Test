using UnityEngine;

public class Menu : MonoBehaviour {

	Animator animator;
	CanvasGroup canvas;
	
	public bool IsOpen {
		get { return animator.GetBool("IsOpen"); }
		set { animator.SetBool("IsOpen", value); }
	}

	public void Awake() {
		animator = GetComponent<Animator>();
		canvas = GetComponent<CanvasGroup>();

		var rect = GetComponent<RectTransform>();
		rect.offsetMax = rect.offsetMin = Vector2.zero;
	}

	public void Update() {
		canvas.blocksRaycasts = canvas.interactable = animator.GetCurrentAnimatorStateInfo(0).IsName("Open");
	}
}
