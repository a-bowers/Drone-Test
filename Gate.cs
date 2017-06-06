using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
	[SerializeField] RaceMagager manager;
	[SerializeField] GateCollider frontCollider;

	List<int> passedFirst = new List<int>();

	public void OnTrigger(GateCollider collider, int id) {
		if(collider.Equals(frontCollider)) {
			if(!passedFirst.Contains(id)) {
				Debug.Log("Passed 1");
				passedFirst.Add(id);
				StartCoroutine(RemoveID(id, 1));
			}
		}
		else { //second collider
			if(passedFirst.Contains(id)) {
				Debug.Log("Passed 2");
				manager.OnGatePassed(this, id);
				passedFirst.Remove(id);
			}
		}
	}

	IEnumerator RemoveID(int id, float delay = 0) {
		yield return new WaitForSeconds(delay);
		if(passedFirst.Contains(id))
			passedFirst.Remove(id);
	}
}