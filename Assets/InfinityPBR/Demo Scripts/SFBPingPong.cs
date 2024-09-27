using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFBPingPong : MonoBehaviour {

	public float distance = 5f;
	public float speed = 10.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(-distance + Mathf.PingPong(Time.time * speed, distance * 2), transform.position.y, transform.position.z);
	}
}
