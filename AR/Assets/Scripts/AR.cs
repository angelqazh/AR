using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AR : MonoBehaviour {
	
	public Image header;

	//Hide and show the finish button
	public void showHide(){
		if (header.IsActive ()) {
			header.gameObject.SetActive (false);
		} else {
			header.gameObject.SetActive (true);
		}
	}
		
	public void goToTest(){
		informationScene.header = "disable";
		informationScene.send = "after";
		SceneManager.LoadScene(1);

	}
}
