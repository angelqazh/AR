/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using UnityEngine.Networking;

/// <summary>
///     A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class Audio : MonoBehaviour, ITrackableEventHandler
{
	#region PRIVATE_MEMBER_VARIABLES

	protected TrackableBehaviour mTrackableBehaviour;
	public TrackableBehaviour video;
	public TrackableBehaviour imagen;
	public RawImage img;
	public Text detalles;
	public AudioSource As;
	private string[] Audios;
	private int countAudio=-2;
	private int numberOfAudios;
	private string statusRed="";
	private string msjAudio="";
//	public Vuforia.VuforiaConfiguration.VideoBackgroundConfiguration vuforia;

	#endregion // PRIVATE_MEMBER_VARIABLES

	#region UNTIY_MONOBEHAVIOUR_METHODS

	protected virtual void Start()
	{
//		vuforia.VideoBackgroundEnabled=true;
		StartCoroutine (getAudios());
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
		//Disabled image and video playback and enable audio playback
		mTrackableBehaviour.gameObject.SetActive (true);
		video.gameObject.SetActive (false);
		imagen.gameObject.SetActive (false);
		img.gameObject.SetActive(false);

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
		if (Audios!=null) {
			msjAudio = "";
			//To advance audio
			if(countAudio==2){
				CancelInvoke();
				Advance();
			}
			if (!As.isPlaying) {
				As.Play ();
				Invoke ("Advance", 20);
			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countAudio==1)?countAudio.ToString()+" vez":countAudio.ToString()+" veces")+"\n"+msjAudio;
		} else {
			msjAudio = "Sin audios para reproducir";
			detalles.text = statusRed+"\nMarcador perdido: "+((countAudio==1)?countAudio.ToString()+" vez":countAudio.ToString()+" veces")+"\n"+msjAudio;
			countAudio = -1;
			StartCoroutine (getAudios());

		}


	}

	//To advance audio
	private void Advance(){
		if (mTrackableBehaviour.isActiveAndEnabled) {
			if (numberOfAudios >= 1) {
				numberOfAudios--;
				countAudio = 0;
				StartCoroutine (playAudioClips (Audios [numberOfAudios], true));

			} else {
				countAudio = 0;
				numberOfAudios = Audios.Length - 1;
				StartCoroutine (playAudioClips (Audios [numberOfAudios], true));

			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countAudio==1)?countAudio.ToString()+" vez":countAudio.ToString()+" veces")+"\n"+msjAudio;
			Invoke ("cambiar", 20);
		}else {
			countAudio = 0;
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

		countAudio++;
		Debug.Log (countAudio);
		detalles.text = statusRed+"\nMarcador perdido: "+((countAudio==1)?countAudio.ToString()+" vez":countAudio.ToString()+" veces")+"\n"+msjAudio;
	}


	private IEnumerator getAudios() { 
		// data that will be sent to the php file
		WWWForm form = new WWWForm();
		form.AddField("Id_Patient", 1);

		// php file path
		UnityWebRequest www = UnityWebRequest.Post("https://emotionvr.000webhostapp.com/SummerApp/getAudio.php", form);

		yield return www.SendWebRequest();

		// if there was a network or http error
		if (www.isNetworkError || www.isHttpError) {
			Debug.Log("Error when connecting to the server");
			statusRed = "Error al conectarse al servidor";
		}
		// if the response is different from empty 
		else if (www.downloadHandler.text.Length > 0) { 
			// Obtaining the response
			statusRed = "";
			Audios = www.downloadHandler.text.Split(',');
			numberOfAudios = Audios.Length-1;
			StartCoroutine (playAudioClips(Audios [numberOfAudios],false));
//			Debug.Log (www.downloadHandler.text);
		}


	}

	//Get the songs of the server
	private IEnumerator playAudioClips(string clip,bool reproducir) {
		using (var uwr = UnityWebRequestMultimedia.GetAudioClip("https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + clip, AudioType.MPEG)) {
			yield return uwr.SendWebRequest();
			if (uwr.isNetworkError || uwr.isHttpError) {
				Debug.Log("Error when connecting to the server");
				statusRed = "Error al conectarse al servidor";
				CancelInvoke ();
				StopAllCoroutines ();
				numberOfAudios++;
				Advance ();
//				yield break;
			} else {
				statusRed = "";
				As.clip = DownloadHandlerAudioClip.GetContent (uwr);
				if(reproducir==true){
					As.Play ();	
				}
			}
		}
	}



	#endregion // PRIVATE_METHODS
}
