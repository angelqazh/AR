using Vuforia;
using UnityEngine;

public class disableAR : MonoBehaviour {

	void Start () {
		//Disable Vuforia
		VuforiaBehaviour.Instance.enabled = false;
		UnityEngine.XR.XRSettings.enabled=false;
	}
}
