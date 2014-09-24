using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {
	public bool Spin_Clockwise = true;
	public float Rotate_Speed = 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		int s;
		if(Spin_Clockwise)
			s = 1;
		else
			s = -1;
		transform.RotateAround(transform.position, transform.up, s * Rotate_Speed * Time.deltaTime);
	}
}
