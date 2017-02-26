using UnityEngine;
using System.Collections;
using Filthworks;

public class InputController : MonoBehaviour {

	public Transform Target;
	public FOBJ TObj;

	private Ray InputRay;
	private RaycastHit InputRay_hit;
	private Vector3 InputPos;
	private Plane baseplane;
	public void Init()
	{
		Target = null;
	}

	public Vector2 Position;
	private Vector2 Position_last;
	public Vector2 Vector, Vector_nonnormal;

	public Vector3 WorldPosition;
	public Vector3 WorldVector;

	public bool HasInput;

	void Update()
	{
		Position_last = Position;
		Position = Input.mousePosition;
		Vector_nonnormal = (Position - Position_last);
		Vector = Vector_nonnormal.normalized;
		UpdateScroll();
		if(GameManager.Paused || GameManager.IgnoreInput) return;
		CheckInput();
	}

	public void CheckInput()
	{
		if(Input.GetMouseButtonUp(0))
		{

			HasInput = false;
			GameManager.OnRelease();
			if(TObj != null) TObj.OnUp();
			TObj = null;
			Target = null;
		}

		if(Input.GetMouseButton(0))
		{
			HasInput = true;
			InputRay = Camera.main.ScreenPointToRay(Position);
			TObj = CheckCollision(InputRay);
			if(TObj != null) TObj.OnStay();
		}

		if(Input.GetMouseButtonDown(0))
		{

			HasInput = true;
			InputRay = Camera.main.ScreenPointToRay(Position);
			TObj = CheckCollision(InputRay);
			if(TObj != null) TObj.OnDown();
			GameManager.OnTouch();
		}	
	}

	public FOBJ CheckCollision(Ray inp)
	{
		baseplane = new Plane(-Vector3.forward, Vector3.zero);
		float d;
		baseplane.Raycast(InputRay, out d);
		GameManager.InputPos = InputRay.GetPoint(d);
		Debug.DrawRay(InputRay.origin, InputRay.direction * 500);
		if(Physics.Raycast(InputRay, out InputRay_hit, Mathf.Infinity))
		{
			WorldPosition = InputRay_hit.point;
			if(Target != InputRay_hit.transform)
			{
				Target = InputRay_hit.transform;
				FOBJ fin = InputRay_hit.transform.GetComponent<FOBJ>();

				Transform p = Target;
				while(fin == null && p.parent != null)
				{
					p = p.parent;
 					fin = p.GetComponent<FOBJ>();
				}
				return fin;
			}
			else return TObj;
		}
		return null;
	}

	private Vector2 ScrollPosition, ScrollVector, ScrollActual;
	private bool Scrolling;
	private float 	ScrollSpeed = 0.0F,
					ScrollSpeed_init = 1.0F,
					ScrollDecay = 0.94F;

	private float   ScrollTimeThreshold = 0.3F,
					ScrollTimeCurrent = 0.0F;

	public void UpdateScroll()
	{
		if(Input.GetMouseButtonDown(0))
		{
			ScrollPosition = Position;
			ScrollVector = Vector2.zero;
			ScrollActual = ScrollVector;
		}
		else if(HasInput)
		{
			if(Position != ScrollPosition)
			{
				ScrollPosition = Position;
				ScrollVector = Vector_nonnormal;
				ScrollActual = ScrollVector;

				ScrollTimeCurrent = 0.0F;
				ScrollSpeed = ScrollSpeed_init;
			}
			else 
			{
				ScrollTimeCurrent += Time.deltaTime;
				ScrollVector = Vector2.zero;
				ScrollActual = Vector2.zero;
			}
		}
		else 
		{
			if(ScrollTimeCurrent >= ScrollTimeThreshold)
			{
				ScrollVector = Vector2.zero;
				ScrollActual = Vector2.zero;
				ScrollSpeed = 0.0F;
			}
			else
			{
				ScrollActual = ScrollVector * ScrollSpeed;
				ScrollSpeed *= ScrollDecay;
				if(ScrollSpeed < 0.05F) 
				{
					ScrollSpeed = 0.0F;
					ScrollVector = Vector2.zero;
				}
			}
		}
	}

	public Vector2 GetScroll()	{return ScrollActual;}
}
