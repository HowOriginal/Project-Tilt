using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
	public bool Is_Moving;
	public GameObject Waddler;
	public float Waddle_Current_Angle = 0;
	public float Waddle_Target_Angle = 0;
	private float Waddle_Speed = 10;

	public GameObject Pickup_Effect;
	public GameObject Drop_Effect;

	private bool [] Movement_Direction = new bool[4]; //[up] [down] [left] [right]
	private Vector3 Up_Direction;
	public float Hit_Check_Dist;
	private bool Build;
	public GameObject TargetBuildable;
	public GameObject Buildable;
	public GameObject[] Buildables = new GameObject[2];
	public GameObject[] Build_Views = new GameObject[2];
	public int Buildables_Selected;
	public int Build_Views_Selected;
	public GameObject MainCamera;

	public int Camera_Max_Turn;
	public float x_Offset;
	public float y_Offset;
	public float Camera_Turn_Speed = 1;

	public bool Freeze;
	private float Freeze_Timer;

	public int Speed = 10;
	public int Slow_Speed = 10;
	public int Fast_Speed = 50;

	public bool Paused = false;

	void Start()
	{
		Buildables_Selected = 0;
		Build_Views_Selected = 0;
		Freeze = false;
		//TargetBuildable.renderer.enabled = false;
		Build = false;
		Up_Direction = new Vector3(0,1,0);
		this.rigidbody.drag = 3;
		this.rigidbody.angularDrag = 5;
	}

	void FixedUpdate () 
	{
		if(transform.rigidbody.velocity.magnitude > 0.5f) 
		{
			Is_Moving = true;
			if(audio.isPlaying == false)
			{
				audio.Play();
			}
		} 
		else
		{
			Is_Moving = false;
			audio.Stop();
		}
		WalkAnimate ();

		if(Time.time - Freeze_Timer >= 1)
		{
			Freeze = false;
		}

		// Key presses
		// Movement
		for(int i = 0; i < 4; i++)
		{
			Movement_Direction[i] = false;
		}
		if(Input.GetKey("w") || Input.GetKey("up"))
		{
			Movement_Direction[0] = true;
		}
		if(Input.GetKey("s") || Input.GetKey("down"))
		{
			Movement_Direction[1] = true;
		}
		if(Input.GetKey("a") || Input.GetKey("left"))
		{
			Movement_Direction[2] = true;
		}
		if(Input.GetKey("d") || Input.GetKey("right"))
		{
			Movement_Direction[3] = true;
		}
		if(Input.GetKey("left shift"))
		{
			Speed = Fast_Speed;
			Waddle_Speed = 20;
			audio.pitch = 2;
		}
		else
		{
			Speed = Slow_Speed;
			Waddle_Speed = 10;
			audio.pitch = 1;
		}

		if(!Freeze)
		{
			Movement ();
			Balance();
		}
		Gravity();

		// Camera Movement
		CameraMovement();

		// Building
		if (Input.GetKey ("1")) 
		{
			Buildables_Selected = 0;
			Build_Views_Selected = 0;
		}
		if (Input.GetKey ("2")) 
		{
			Buildables_Selected = 1;
			Build_Views_Selected = 1;
		}
		if (Input.GetKey ("3")) 
		{
			Buildables_Selected = 2;
			Build_Views_Selected = 2;
		}
		if (Input.GetKey ("4")) 
		{
			Buildables_Selected = 3;
			Build_Views_Selected = 3;
		}
		if(Input.GetKey ("space"))
		{
			Build = true;
		}
		else
		{
			Build = false;
		}
		Building();

		// Deconstructing
		if(Input.GetKeyDown("r"))
		{
			Remove();
		}


	}

	void Update()
	{
		// Pause
		if (Input.GetKeyDown ("p")) 
		{
			if(Paused)
			{
				Paused = false;
			}
			else
			{
				Paused = true;
			}
		}
		if (Paused) 
		{
			Time.timeScale = 0;
		} else 
		{
			Time.timeScale = 1;
		}
	}

	void WalkAnimate()
	{
		float Waddle_Max_Angle = 15;




		// Bounds check

		if (Is_Moving == true) 
		{
			if(Waddle_Target_Angle == 0)
			{
				Waddle_Target_Angle = -15;
			}
			if(Waddle_Current_Angle >= 15)
			{
				Waddle_Target_Angle = -15;
			}
			else if(Waddle_Current_Angle <= -15)
			{
				Waddle_Target_Angle = 15;
			}
		}
		else
		{
			Waddle_Target_Angle = 0;
		}

		if (Waddle_Current_Angle > Waddle_Target_Angle)
		{
			float angle = 15 * Time.deltaTime * -Waddle_Speed;
			if(angle > Waddle_Current_Angle - Waddle_Target_Angle)
			{
				angle = Waddle_Current_Angle - Waddle_Target_Angle;
			}
			Waddler.transform.RotateAround(transform.position, transform.up, angle);
			Waddle_Current_Angle += angle;
		}
		else if (Waddle_Current_Angle < Waddle_Target_Angle)
		{
			float angle = 15 * Time.deltaTime * Waddle_Speed;
			if(angle > Waddle_Target_Angle - Waddle_Current_Angle)
			{
				angle = Waddle_Target_Angle - Waddle_Current_Angle;
			}
			Waddler.transform.RotateAround(transform.position, transform.up, angle);
			Waddle_Current_Angle += angle;
		}
	}

	void Movement()
	{
		if( Movement_Direction[0] && Movement_Direction[1])
		{
			Movement_Direction[0] = Movement_Direction[1] = false;
		}
		if( Movement_Direction[2] && Movement_Direction[3])
		{
			Movement_Direction[2] = Movement_Direction[3] = false;
		}

		// Forward
		if(Movement_Direction[0])
		{
			this.rigidbody.AddRelativeForce(0,0,Speed);
		}
		// Backward
		else if(Movement_Direction[1])
		{
			this.rigidbody.AddRelativeForce(0,0,-Speed);
		}
		// Turn left
		if(Movement_Direction[2])
		{
			this.rigidbody.AddRelativeTorque(Vector3.down * 2);
		}
		// Turn right
		else if(Movement_Direction[3])
		{
			this.rigidbody.AddRelativeTorque(Vector3.up * 2);
		}
	}

	void Balance()
	{
		// Debug Rays

		Debug.DrawRay(transform.TransformPoint(Vector3.forward * Hit_Check_Dist), -transform.up + (-transform.forward * 0.25f), Color.green);
		Debug.DrawRay(transform.TransformPoint(Vector3.back * Hit_Check_Dist), -transform.up + (transform.forward * 0.25f), Color.green);
		Debug.DrawRay(transform.TransformPoint(Vector3.left * Hit_Check_Dist), -transform.up + (transform.right * 0.25f), Color.green);
		Debug.DrawRay(transform.TransformPoint(Vector3.right * Hit_Check_Dist), -transform.up + (-transform.right * 0.25f), Color.green);

		RaycastHit Forward_Hit, Back_Hit, Left_Hit, Right_Hit;
		Physics.Raycast(transform.TransformPoint(Vector3.forward * Hit_Check_Dist), -transform.up + (-transform.forward * 0.25f), out Forward_Hit);
		Physics.Raycast(transform.TransformPoint(Vector3.back * Hit_Check_Dist), -transform.up + (transform.forward * 0.25f), out Back_Hit);
		Physics.Raycast(transform.TransformPoint(Vector3.left * Hit_Check_Dist), -transform.up + (transform.right * 0.25f), out Left_Hit);
		Physics.Raycast(transform.TransformPoint(Vector3.right * Hit_Check_Dist), -transform.up + (-transform.right * 0.25f), out Right_Hit);
		/*
		Debug.DrawRay(transform.TransformPoint(Vector3.left * Hit_Check_Dist), -transform.up * 0.5f, Color.green);
		if(Forward_Hit.distance > 1)
		{
			Forward_Hit.point = transform.TransformPoint(Vector3.forward * Hit_Check_Dist) + -transform.up * 0.5f;
		}
		if(Back_Hit.distance > 1)
		{
			Back_Hit.point = transform.TransformPoint(Vector3.back * Hit_Check_Dist) + -transform.up * 0.5f;
		}
		if(Left_Hit.distance > 1)
		{
			Left_Hit.point = transform.TransformPoint(Vector3.left * Hit_Check_Dist) + -transform.up * 0.5f;
		}
		if(Right_Hit.distance > 1)
		{
			Right_Hit.point = transform.TransformPoint(Vector3.right * Hit_Check_Dist) -transform.up * 0.5f;
		}
*/
		Vector3 Balance_Axis;
		float Balance_Angle;

		if(Forward_Hit.distance < 1 && Back_Hit.distance < 1 && Left_Hit.distance < 1 && Right_Hit.distance < 1)
		{
			Up_Direction = (Vector3.Cross (Forward_Hit.point, Right_Hit.point) +
			                Vector3.Cross (Right_Hit.point, Back_Hit.point) +
			                Vector3.Cross(Back_Hit.point, Left_Hit.point) +
			                Vector3.Cross(Left_Hit.point, Forward_Hit.point)).normalized;
			Balance_Axis = Vector3.Cross (transform.up, Up_Direction).normalized;
			Balance_Angle = Vector3.Angle(transform.up, Up_Direction);
			transform.RotateAround(transform.position,Balance_Axis, Balance_Angle * 0.5f);
		}


		Debug.DrawRay (transform.position, Up_Direction, Color.blue);
	}

	void Gravity()
	{
		RaycastHit hit;
		if(Physics.Raycast(transform.position, -transform.up, out hit)){}
		else
		{// If No hit is detected, apply gravity
			hit.distance = 1;
		}
		if(hit.distance > 0.5f)
			transform.rigidbody.AddRelativeForce(0,-15,0);
	}
	
	void Building()
	{
		if(Build)
		{
			Build_Views[Build_Views_Selected].renderer.enabled = true;
			if(Input.GetMouseButtonDown(0))
			{
				GameObject clone = Instantiate(Buildables[Buildables_Selected], TargetBuildable.transform.position, Quaternion.identity) as GameObject;
				clone.tag = "Terrain";
				Drop_Effect.audio.Play();
			}
		}
		else
		{
			foreach (GameObject g in Build_Views)
			{
				g.renderer.enabled = false;
			}
		}
	}

	void Remove()
	{
		Ray ray = MainCamera.camera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		RaycastHit hit;
		Physics.Raycast(MainCamera.transform.position, ray.direction, out hit);
		if(hit.transform.gameObject.tag == "Terrain")
		{
			Debug.Log ("Removal");
			Destroy(hit.transform.gameObject);
			Pickup_Effect.audio.Play ();
		}
	}

	void CameraMovement()
	{
		if(Input.GetMouseButton(1))
		{
			//Value between -1.0 and 1.0
			//float x_Offset = 0;
			//float y_Offset = 0;
			Vector3 Mouse_Position = Input.mousePosition;
			x_Offset = (Mouse_Position.x - Screen.width/2)/(Screen.width/2);
			y_Offset = (Mouse_Position.y - Screen.height/2)/(Screen.height/2);

			Vector3 Target = transform.forward;
			Vector3 Right = transform.right;
			Target = Quaternion.AngleAxis(Camera_Max_Turn * x_Offset, transform.up) * Target;
			Right = Quaternion.AngleAxis(Camera_Max_Turn * x_Offset, transform.up) * Right;
			Target = Quaternion.AngleAxis(0.5f * Camera_Max_Turn * y_Offset, -Right) * Target;

			Target = (Target + MainCamera.transform.forward)/Camera_Turn_Speed;
			Target += MainCamera.transform.position;
			MainCamera.transform.LookAt(Target, transform.up);
		}
		else
		{
			//Vector3 Target = ((MainCamera.transform.position + transform.forward) + (MainCamera.transform.forward))/Camera_Turn_Speed;
			Vector3 Target = MainCamera.transform.position + transform.forward;
			Vector3 Direction = Target - MainCamera.transform.position;
			if(Direction.y != 0)
			{
				Vector3 Right = Vector3.Cross(Direction, new Vector3(Direction.x, 0, Direction.z));
				Vector3 Up = Quaternion.AngleAxis(-90, Right) * Target;
				//MainCamera.transform.LookAt(Target, Up);
			}
			else
			{
				Vector3 Up = transform.up;
				//MainCamera.transform.LookAt(Target, Up);
			}
			//float Omega = Direction.y/(new Vector2(Direction.x, Direction.z).magnitude);
			//float Final_Angle = Mathf.Atan(Mathf.Deg2Rad * Omega) * Mathf.Rad2Deg;
			MainCamera.transform.LookAt(MainCamera.transform.position + transform.forward, transform.up);
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if(collider.gameObject.tag == "Obstacle")
		{
			Debug.Log ("hit");
			Freeze = true;
			Freeze_Timer = Time.time;
			Vector3 Push_Direction = transform.position - collider.gameObject.transform.position;
			Debug.Log ( "(" + Push_Direction.x + ", " + Push_Direction.y + ", " + Push_Direction.z + ")");
			this.rigidbody.AddForce(Push_Direction * 1000);
		}
		else if(collider.gameObject.tag == "Finish")
		{
			Debug.Log ("Victory!");
		}
	}
}
