using System.Collections.Generic;
using UnityEngine;

public class RaceMagager : MonoBehaviour {

	[SerializeField] public List<Gate> GateList;
	List<int> PlayerList;
	[SerializeField] int Laps = 1;
	int currentlap = 1;
	List<bool> GatesPassed = new List<bool>();

	public int NextGate {
		get {
			return GatesPassed.IndexOf(false); //TODO fix this for last gate
		}
	}

	void Start () {
		foreach(Gate gate in GateList) {
			GatesPassed.Add(false);
		}
	}

	public void OnGatePassed(Gate gate, int id) {
		if(gate.Equals(GateList[NextGate])) { //Check if the gate is the next gate for the player
			Debug.Log("Passed Gate!");
			GatesPassed[NextGate] = true;
			if(NextGate == -1) {
				if(currentlap++ < Laps) { //we have finished the lap but are still racing
					ResetGates();
					Debug.Log("Lap Complete!");
				} else { //Race complete
					Debug.Log("Victory!");
					ResetGates(); //Also just reset the gates for now for testing
				}
			}
		}		
	}

	void ResetGates() {
		for(int i = 0; i < GatesPassed.Count; i++) {
			GatesPassed[i] = false;
		}
	}
}
