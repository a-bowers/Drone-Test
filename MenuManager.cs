using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
	//https://www.youtube.com/watch?v=QxRAIjXdfFU&t=241

	public Menu currentMenu;
	public Transform mainMenu, optionsMenu;

	public void Start() {
		ShowMenu(currentMenu);
	}

	public void ShowMenu(Menu menu) {
		if(currentMenu != null)
			currentMenu.IsOpen = false;

		currentMenu = menu;
		currentMenu.IsOpen = true;
	}

	public void LoadScene(string scene) {
		UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
	}

	public void ExitGame() {
		Application.Quit();
	}
}
