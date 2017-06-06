using UnityEngine;

public class GateCollider : MonoBehaviour {

	[SerializeField]
	Gate gate;

	private void OnTriggerEnter(Collider other) {
		//if(other.CompareTag("Player")) {
			Debug.Log("Contact with Collider");
			//TODO get id of the player
			gate.OnTrigger(this, 1); //Send id to parent gate script
		//}
	}
}
