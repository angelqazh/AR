using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class statusEmocion : MonoBehaviour {

	public Text lblEmocion;
	public Slider slider;
	public Image header;

	void Start(){
		if(informationScene.header.Equals("disable")){
			header.gameObject.SetActive (false);
		}
	}

	//Put the patient's status
	public void onChange(){
		if(slider.value<=0.2){
			lblEmocion.text = "Muy mal";
		}else if(slider.value>0.2 && slider.value<=0.4){
			lblEmocion.text = "Mal";
		}else if(slider.value>0.4 && slider.value<=0.6){
			lblEmocion.text = "Normal";
		}else if(slider.value>0.6 && slider.value<=0.8){
			lblEmocion.text = "Muy bien";
		}else if(slider.value>0.8){
			lblEmocion.text = "Excelente";
		}

	}

	public void goToHome(){
		SceneManager.LoadScene(0);
	}

	public void sendStatus(){
		if(informationScene.send.Equals("before")){
			//Store the data of the previous state in variables
			informationScene.beforeStatus = lblEmocion.text;
			informationScene.numberBeforeS = (slider.value==0 || slider.value==1)?slider.value.ToString():slider.value.ToString().Remove(6);
			SceneManager.LoadScene(2);
		}else if(informationScene.send.Equals("after")){
			//Store the patient status in the DB
			StartCoroutine(status());
		}
	}

	private IEnumerator status() { 
		// Data that will be sent to the php file
		WWWForm form = new WWWForm();
		form.AddField("Id_Patient", 1);
		form.AddField("before_State", informationScene.beforeStatus);
		form.AddField("after_State", lblEmocion.text);
		form.AddField("before_StateN", informationScene.numberBeforeS);
		form.AddField("after_StateN", (slider.value==0 || slider.value==1)?slider.value.ToString():slider.value.ToString().Remove(6));

		// php file path
		UnityWebRequest www = UnityWebRequest.Post("https://emotionvr.000webhostapp.com/SummerApp/setStatus.php", form);

		yield return www.SendWebRequest();

		// if there was a network or http error
		if (www.isNetworkError || www.isHttpError) {
			Debug.Log("Error when connecting to the server");
		}
		// if the response is different from empty 
		else if (www.downloadHandler.text.Length > 0) { 
			// Obtaining the response
			if(www.downloadHandler.text.Equals("good")){
				Debug.Log("Status send successfully");
				SceneManager.LoadScene(0);
			}else if(www.downloadHandler.text.Equals("error")){
				Debug.Log("Error storing status");
			}
		}

	}

}
