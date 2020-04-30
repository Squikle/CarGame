using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	[SerializeField] Transform target;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = target.position;
		Quaternion targetRotation = Quaternion.Euler(0,target.rotation.eulerAngles.y,0);
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
		transform.Translate(new Vector3(0,8,-25));
	}
}
