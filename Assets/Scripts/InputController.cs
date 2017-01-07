using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

	public InputTarget Target;

	private Ray InputRay;
	private RaycastHit InputRay_hit;
	private Vector3 InputPos;
	private Plane baseplane;
	public void Init()
	{
		Target = null;
	}

	void Update()
	{
		if(Input.GetMouseButton(0))
		{
			InputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			baseplane = new Plane(Vector3.up, Vector3.zero);
			float d;
			baseplane.Raycast(InputRay, out d);
			InputPos = InputRay.GetPoint(d);
		}
		if(Input.GetMouseButtonDown(0))
		{
			if(Physics.Raycast(InputRay, out InputRay_hit, Mathf.Infinity))
			{
				Transform hit = InputRay_hit.transform;
				if(hit.tag == "TouchObj")
				{
					Target = hit.GetComponent<InputTarget>();
				}
			}
			GameManager.OnTouch();
		}

		if(Target != null)
		{
			if(Input.GetMouseButton(0))
			{
				Target.Drag(InputPos);
			}
			else if(Input.GetMouseButtonUp(0))
			{
				Target.Release(InputPos);
				Target = null;

				GameManager.OnRelease();
			}
		}

		
	}
}
