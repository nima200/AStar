using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPatrol : MonoBehaviour {

	Transform curPos;
	float speed;

	// Use this for initialization
	void Start () {
		curPos = gameObject.transform;	
		speed = Random.Range(10f, 90f);
	}
	
	// Update is called once per frame
	void Update () {
		curPos.RotateAround(curPos.position, curPos.up, Time.deltaTime * speed); 
	}
}
