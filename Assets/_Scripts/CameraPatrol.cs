using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * CameraPatrol
 * A simple script to make the cameras rotate at different speeds
 */
public class CameraPatrol : MonoBehaviour {

	Transform curPos;
	float speed;

	// Use this for initialization
	void Start () {
		curPos = gameObject.transform;	
		// the rotation speed is randomly generated for the sake of experimental diversity
		speed = Random.Range(10f, 90f);
	}
	
	// Update is called once per frame
	void Update () {
		// Rotates the gameObject on the y-axis by (Time.deltaTime * speed)
		curPos.RotateAround(curPos.position, curPos.up, Time.deltaTime * speed); 
	}
}
