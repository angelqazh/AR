/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
///     A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class Imagen : MonoBehaviour, ITrackableEventHandler
{
	#region PRIVATE_MEMBER_VARIABLES

	protected TrackableBehaviour mTrackableBehaviour;
	public TrackableBehaviour video;
	public TrackableBehaviour sound;
	public Text detalles;
	public RawImage img;
	private string[] Imagenes;
	private int countImg=-2;
	private int numberOfImagenes;
	private string statusRed="";
	private string msjImagen="";

	#endregion // PRIVATE_MEMBER_VARIABLES

	#region UNTIY_MONOBEHAVIOUR_METHODS

	protected virtual void Start()
	{
		StartCoroutine (getImagenes());
		mTrackableBehaviour = GetComponent<TrackableBehaviour>();
		if (mTrackableBehaviour)
			mTrackableBehaviour.RegisterTrackableEventHandler(this);
	}


	#endregion // UNTIY_MONOBEHAVIOUR_METHODS

	#region PUBLIC_METHODS

	/// <summary>
	///     Implementation of the ITrackableEventHandler function called when the
	///     tracking state changes.
	/// </summary>
	public void OnTrackableStateChanged(
		TrackableBehaviour.Status previousStatus,
		TrackableBehaviour.Status newStatus)
	{
		if (newStatus == TrackableBehaviour.Status.DETECTED ||
			newStatus == TrackableBehaviour.Status.TRACKED ||
			newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
		{
			//            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
			OnTrackingFound();
		}
		else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
			newStatus == TrackableBehaviour.Status.NOT_FOUND)
		{
			//            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
			OnTrackingLost();
		}
		else
		{
			// For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
			// Vuforia is starting, but tracking has not been lost or found yet
			// Call OnTrackingLost() to hide the augmentations
			OnTrackingLost();
		}
	}

	#endregion // PUBLIC_METHODS

	#region PRIVATE_METHODS

	protected virtual void OnTrackingFound()
	{
		//Disabled audio and video playback and enable image playback
		mTrackableBehaviour.gameObject.SetActive (true);
		video.gameObject.SetActive (false);
		sound.gameObject.SetActive (false);

		var rendererComponents = GetComponentsInChildren<Renderer>(true);
		var colliderComponents = GetComponentsInChildren<Collider>(true);
		var canvasComponents = GetComponentsInChildren<Canvas>(true);

		// Enable rendering:
		foreach (var component in rendererComponents)
			component.enabled = true;

		// Enable colliders:
		foreach (var component in colliderComponents)
			component.enabled = true;

		// Enable canvas':
		foreach (var component in canvasComponents)
			component.enabled = true;
		
		if (Imagenes!=null) {
			msjImagen = "";
			//To advance image
			if (countImg == 2) {
				CancelInvoke ();
				Advance ();
			}

			if (img.IsActive () == false) {
				img.gameObject.SetActive (true);
				Invoke ("Advance", 15);
			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countImg==1)?countImg.ToString()+" vez":countImg.ToString()+" veces")+"\n"+msjImagen;
		} else {
			msjImagen = "Sin imágenes para mostrar";
			detalles.text = statusRed+"\nMarcador perdido: "+((countImg==1)?countImg.ToString()+" vez":countImg.ToString()+" veces")+"\n"+msjImagen;
			countImg = -1;
			StartCoroutine (getImagenes());

		}

	}

	//To advance image
	private void Advance(){
		if (mTrackableBehaviour.isActiveAndEnabled) {
			if (numberOfImagenes >= 1) {
				numberOfImagenes--;
				countImg = 0;
				StartCoroutine (showImages (Imagenes [numberOfImagenes]));

			} else {
				countImg = 0;
				numberOfImagenes = Imagenes.Length - 1;
				StartCoroutine (showImages (Imagenes [numberOfImagenes]));

			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countImg==1)?countImg.ToString()+" vez":countImg.ToString()+" veces")+"\n"+msjImagen;
			Invoke ("Advance", 15);
		} else {
			countImg = 0;
			CancelInvoke ();
			StopAllCoroutines ();
		}
	}

	protected virtual void OnTrackingLost()
	{
		var rendererComponents = GetComponentsInChildren<Renderer>(true);
		var colliderComponents = GetComponentsInChildren<Collider>(true);
		var canvasComponents = GetComponentsInChildren<Canvas>(true);

		// Disable rendering:
		foreach (var component in rendererComponents)
			component.enabled = false;

		// Disable colliders:
		foreach (var component in colliderComponents)
			component.enabled = false;

		// Disable canvas':
		foreach (var component in canvasComponents)
			component.enabled = false;

		countImg++;
		Debug.Log (countImg);
		detalles.text = statusRed+"\nMarcador perdido: "+((countImg==1)?countImg.ToString()+" vez":countImg.ToString()+" veces")+"\n"+msjImagen;
	}


	private IEnumerator getImagenes() { 
		// data that will be sent to the php file
		WWWForm form = new WWWForm();
		form.AddField("Id_Patient", 1);

		// php file path
		UnityWebRequest www = UnityWebRequest.Post("https://emotionvr.000webhostapp.com/SummerApp/getImage.php", form);

		yield return www.SendWebRequest();

		// if there was a network or http error
		if (www.isNetworkError || www.isHttpError) {
			Debug.Log("Error when connecting to the server");
			statusRed = "Error al conectarse al servidor";
		}
		// if the response is different from empty 
		else if (www.downloadHandler.text.Length > 0) { 
			// Obtaining the response
			statusRed="";
			Imagenes = www.downloadHandler.text.Split(',');
			numberOfImagenes = Imagenes.Length-1;
			StartCoroutine (showImages(Imagenes [numberOfImagenes]));
			//			Debug.Log (www.downloadHandler.text);
		}


	}
		

	//Load the image of the server
	private IEnumerator showImages(string imagen)
	{
		UnityWebRequest wr = new UnityWebRequest("https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + imagen);
		DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
		wr.downloadHandler = texDl;
		yield return wr.SendWebRequest();
		if (!(wr.isNetworkError || wr.isHttpError)) {
			img.texture = texDl.texture;
			statusRed="";
		} else {
			Debug.Log("Error when connecting to the server");
			statusRed = "Error al conectarse al servidor";
			CancelInvoke ();
			StopAllCoroutines ();
			numberOfImagenes++;
			Advance ();
		}
	}





	#endregion // PRIVATE_METHODS
}
