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
public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;
	public TrackableBehaviour sound;
	public TrackableBehaviour imagen;
	public RawImage img;
	public Text detalles;
	public UnityEngine.Video.VideoPlayer vp;
	private string[] videos;
	private int countVideo=-2;
	private int numberOfVideos;
	private string statusRed="";
	private string msjVideo="";

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS

    protected virtual void Start()
    {
		StartCoroutine (getVideos());
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
    }

	protected virtual void Update()
	{
		//To advance video
		if(vp.time>=20 || vp.time>=(vp.frameCount/vp.frameRate)){
			if (numberOfVideos >= 1) {
				countVideo = 0;
				numberOfVideos--;
				vp.url = "https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + videos [numberOfVideos];
				vp.Play ();
			} else {
				countVideo = 0;
				numberOfVideos = videos.Length-1;
				vp.url = "https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + videos [numberOfVideos];
				vp.Play ();
			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countVideo==1)?countVideo.ToString()+" vez":countVideo.ToString()+" veces")+"\n"+msjVideo;
		}
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
		//Disabled audio and image playback and enable video playback
		mTrackableBehaviour.gameObject.SetActive (true);
		sound.gameObject.SetActive (false);
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

		if(videos!=null){
			msjVideo = "";
			//To advance video
			if(countVideo==2){
				if (numberOfVideos >= 1) {
					numberOfVideos--;
					vp.url = "https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + videos [numberOfVideos];
					countVideo = 0;
				} else {
					countVideo = 0;
					numberOfVideos = videos.Length-1;
					vp.url = "https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/" + videos [numberOfVideos];
				}
				detalles.text = statusRed+"\nMarcador perdido: "+((countVideo==1)?countVideo.ToString()+" vez":countVideo.ToString()+" veces")+"\n"+msjVideo;
			}
			detalles.text = statusRed+"\nMarcador perdido: "+((countVideo==1)?countVideo.ToString()+" vez":countVideo.ToString()+" veces")+"\n"+msjVideo;
			vp.Play ();
		} else {
			msjVideo = "Sin videos para reproducir";
			detalles.text = statusRed+"\nMarcador perdido: "+((countVideo==1)?countVideo.ToString()+" vez":countVideo.ToString()+" veces")+"\n"+msjVideo;
			countVideo = -1;
			StartCoroutine (getVideos());

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

		countVideo++;
		detalles.text = statusRed+"\nMarcador perdido: "+((countVideo==1)?countVideo.ToString()+" vez":countVideo.ToString()+" veces")+"\n"+msjVideo;
    }


	private IEnumerator getVideos() { 
		// data that will be sent to the php file
		WWWForm form = new WWWForm();
		form.AddField("Id_Patient", 1);

		// php file path
		UnityWebRequest www = UnityWebRequest.Post("https://emotionvr.000webhostapp.com/SummerApp/getVideo.php", form);

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
			videos = www.downloadHandler.text.Split(',');
			numberOfVideos = videos.Length-1;
			vp.url = "https://emotionvr.000webhostapp.com/SummerApp/multimedia/1/"+videos[numberOfVideos];
//			Debug.Log (www.downloadHandler.text);
		}


	}

    #endregion // PRIVATE_METHODS
}
