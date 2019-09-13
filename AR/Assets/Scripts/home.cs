using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class home : MonoBehaviour {

	public void goToTest(){
		informationScene.header = "enable";
		informationScene.send = "before";
		SceneManager.LoadScene(1);
	}
}
