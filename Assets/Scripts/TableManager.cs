using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class TableManager : MonoBehaviour {
	public UIObj [] TableObjects;

	public UIObj TableUI;
	public GameObject TableObj;
	public _Seat [] Seat;
	public _Food [] Food;
	public Transform EntryDoor, ExitDoor;

	public VectorLine Movement;

	int mvmt_segments = 100;

	public void Init()
	{
		List<Vector2> splinepoints = new List<Vector2>();
		for(int i = 0; i < Seat.Length; i++)
		{
			Seat[i].Index = i;
			Vector3 t = Seat[i].transform.position + Seat[i].transform.forward;
			splinepoints.Add(new Vector2(t.x, t.z));
		}

		//Movement = new VectorLine("Movement Path", new List<Vector2>(mvmt_segments+1), 4.0F, LineType.Continuous);

		//Movement.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, true);
		//Movement.Draw();
	}

	public void SetupTable(int n)
	{
		if(TableUI == TableObjects[n]) return;

		for(int i = 0; i < TableObjects.Length; i++) 
		{
			if(i != n)	TableObjects[i].TweenActive(false);
		}
		TableUI = TableObjects[n];//(UIObj) Instantiate(t);
		//TableUI.SetParent(GameManager.Module.MUI[7]);
		//TableUI.ResetRect();

		TableObj = TableUI.Child[0].transform.gameObject;
		TableObj.transform.SetParent(GameManager.Module.transform);
	 	Seat = new _Seat[TableUI.Child.Length-1];
	 	List<Vector2> splinepoints = new List<Vector2>();
	 	for(int i = 0; i < Seat.Length; i++)
	 	{
	 		Seat[i] = new _Seat();
	 		Seat[i].Index = i;
	 		Seat[i].Object = TableUI.Child[i+1].transform.gameObject;
	 		Vector3 p = TableUI.Child[i+1].transform.position + TableUI.Child[i+1].transform.forward * 1.7F;
			splinepoints.Add(new Vector2(p.x, p.y));
	 	}
	 	VectorLine.Destroy(ref Movement);
	 	
	 	Movement = new VectorLine("Movement Path", new List<Vector2>(mvmt_segments+1), 4.0F, LineType.Continuous);
		Movement.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, true);
		//Movement.Draw();
	}

	public Vector3 GetMovementPoint(float ratio)
	{
		int index = 0;
		Vector2 a = Movement.GetPoint01(ratio, out index);
		Vector3 fin = new Vector3(a.x, a.y, Seat[0].Object.transform.position.z);
		return fin;
	}

	public Vector3 GetMovementPointAtSeat(int s)
	{
		float norm = (float)s/(float)Seat.Length;
		Vector2 mpos = Movement.GetPoint01(norm);
		Vector3 fin = new Vector3(mpos.x, mpos.y, Seat[0].Object.transform.position.z);
		return fin;
	}

	public float GetMovementNormalAtSeat(int s)
	{
		float norm = (float)s/(float)Seat.Length;
		return norm;
	}

	float x_rot = 80;
	public IEnumerator MoveSeat(GreatGrand g, int from, int to, float time)
	{
		Transform t = g.Face.transform;
		g.isSeated = false;
		Vector3 start = GetMovementPointAtSeat(from);
		Vector3 end = GetMovementPointAtSeat(to);

		if(start == end) yield break;

		float start_norm = GetMovementNormalAtSeat(from);
		float end_norm = GetMovementNormalAtSeat(to);

	//Checking to see if clockwise or anticlockwise movement will be faster
		float movenorm = 0.0F, clockwise, anticlock;
		if(end_norm > start_norm) 
		{
			clockwise = end_norm - start_norm;
			anticlock = (1.0F + start_norm) - end_norm;
		}
		else 
		{
			clockwise = start_norm - (end_norm + 1.0F);
			anticlock = end_norm - start_norm;
		}

		if(Mathf.Abs(clockwise) < Mathf.Abs(anticlock)) movenorm = clockwise;
		else movenorm = anticlock;

		float timer = 0.0F;
		float timer_total = Mathf.Clamp(1.1F * Mathf.Abs(movenorm), 0.2F, 0.9F);

		float norm = start_norm;
		float norm_rate = movenorm / timer_total;

		float getup_init = 0.05F;
		float getup_time = 0.0F;
		while((getup_time += Time.deltaTime) < getup_init)
		{
			t.position = Vector3.Lerp(Seat[from].transform.position, start, getup_time/getup_init);
			t.LookAt(TableObj.transform.position, Vector3.forward);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);
			yield return null;
		}

		while((timer += Time.deltaTime) < timer_total)
		{
			t.position = GetMovementPoint(norm);
			t.LookAt(TableObj.transform.position, Vector3.forward);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);

			norm += norm_rate * Time.deltaTime;
			if(norm < 0.0F) norm = 1.0F;
			if(norm > 1.0F) norm = 0.0F;
			yield return null;
		}

		float sitdown_init = 0.05F;
		float sitdown_time = 0.0F;
		while((sitdown_time += Time.deltaTime) < sitdown_init)
		{
			t.position = Vector3.Lerp(end, Seat[to].Position, sitdown_time/sitdown_init);
			t.transform.rotation = Quaternion.Slerp(t.transform.rotation,
													Seat[to].Rotation * Quaternion.Euler(5, 180,180),
													sitdown_time/sitdown_init);
			//t.LookAt(TableObj.transform.position);
			//t.rotation *= Quaternion.Euler(x_rot, 0,180);

			yield return null;
		}
		g.isSeated = true;
		yield return null;
	}

	public IEnumerator DoorToSeat(GreatGrand g, int seat, float time)
	{
		Transform t = g.Face.transform;
		g.isSeated = false;
		Vector3 d = EntryDoor.position;
		Vector3 start = GetMovementPoint(0.0F);
		Vector3 end = GetMovementPointAtSeat(seat);

		float start_norm = 0.0F;
		float end_norm = GetMovementNormalAtSeat(seat);
		float movenorm = end_norm - start_norm;

		float getup_init = 0.3F;
		float getup_time = 0.0F;
		while((getup_time += Time.deltaTime) < getup_init)
		{
			t.position = Vector3.Lerp(d, start, getup_time/getup_init);
			t.LookAt(TableObj.transform.position, Vector3.forward);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);
			yield return null;
		}

		float timer = 0.0F;
		float norm = start_norm;
		float norm_rate = movenorm / time;

		while((timer += Time.deltaTime) < time)
		{
			t.position = GetMovementPoint(norm);
			t.LookAt(TableObj.transform.position, Vector3.forward);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);

			norm += norm_rate * Time.deltaTime;
			if(norm < 0.0F) norm = 1.0F;
			if(norm > 1.0F) norm = 0.0F;
			yield return null;
		}

		float sitdown_init = 0.3F;
		float sitdown_time = 0.0F;
		while((sitdown_time += Time.deltaTime) < sitdown_init)
		{
			t.position = Vector3.Lerp(end, Seat[seat].Position, sitdown_time/sitdown_init);
			t.rotation = Quaternion.Slerp(t.rotation,
										Seat[seat].Rotation * Quaternion.Euler(5, 180,180),
										sitdown_time/sitdown_init);

			yield return null;
		}
		g.isSeated = true;
		yield return null;
	}


	public IEnumerator Exit(GreatGrand g, float time)
	{
		Transform t = g.Face.transform;
		g.isSeated = false;
		Vector3 d = ExitDoor.position;
		Vector3 start = GetMovementPointAtSeat(g.Seat.Index); 
		Vector3 end = GetMovementPoint(0.5F);

		float start_norm = GetMovementNormalAtSeat(g.Seat.Index);
		float end_norm = 0.5F;
		float movenorm = end_norm - start_norm;

		/*float getup_init = 0.3F;
		float getup_time = 0.0F;
		while((getup_time += Time.deltaTime) < getup_init)
		{
			t.position = Vector3.Lerp(d, start, getup_time/getup_init);
			t.LookAt(TableObj.transform.position);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);
			yield return null;
		}*/

		float timer = 0.0F;
		float norm = start_norm;
		float norm_rate = movenorm / time;

		while((timer += Time.deltaTime) < time)
		{
			t.position = GetMovementPoint(norm);
			t.LookAt(ExitDoor.transform.position, Vector3.forward);
			t.rotation *= Quaternion.Euler(x_rot, 0,180);
			norm += norm_rate * Time.deltaTime;
			if(norm < 0.0F) norm = 1.0F;
			if(norm > 1.0F) norm = 0.0F;
			yield return null;
		}

		float sitdown_init = 0.3F;
		float sitdown_time = 0.0F;
		while((sitdown_time += Time.deltaTime) < sitdown_init)
		{
			t.position = Vector3.Lerp(end, d, sitdown_time/sitdown_init);
			//t.rotation = Quaternion.Slerp(t.rotation,
			//							Seat[seat].Rotation * Quaternion.Euler(5, 180,180),
			//							sitdown_time/sitdown_init);
			yield return null;
		}
		//g.isSeated = true;
		yield return null;
	}


	public _Seat GetNonNeighbourSeat(_Seat s)
	{
		int min = s.Index - 2;
		int max = s.Index + 2;

		List<int> range = new List<int>();
		
		for(int l = 0; l < min; l++)
		{
			range.Add(l);
		}
		for(int h = GameManager.Module.Grands.Length; h > max; h--)
		{
			range.Add(h);
		}

		return Seat[range[Random.Range(0, range.Count)]];
	}

	public int SeatDistance(GreatGrand a, GrumpObj b)
	{
		if(b is GreatGrand) return SeatDistance(a, b as GreatGrand);
		return 0;
	}

	public int SeatDistance(GreatGrand a, GreatGrand b)
	{
		if(a.Seat == null || b.Seat== null) return 0;
		int ai = a.Seat.Index;
		int bi = b.Seat.Index;
		if(ai > bi + Seat.Length/2) bi += Seat.Length;
		if(bi > ai + Seat.Length/2) ai += Seat.Length;
		return Mathf.Abs(ai - bi);
	}

	/*public _Seat NearestSeat(Vector3 pos)
	{
		float dist = 1000;
		_Seat nearest = null;
		for(int i = 0; i < Seat.Length; i++)
		{
			float d = Vector3.Distance(Seat[i].transform.position, pos);
			if(d < dist)
			{
				dist = d;
				nearest = Seat[i];
			}
		}
		return nearest;
	}*/

	public _Seat NearestSeat(Vector2 pos)
	{
		float dist = 1000;
		_Seat nearest = null;
		for(int i = 0; i < Seat.Length; i++)
		{
			float d = Vector2.Distance(Seat[i].Position, pos);
			if(d < dist)
			{
				dist = d;
				nearest = Seat[i];
			}
		}
		return nearest;
	}

	public void Reset()
	{
		for(int i = 0; i < Seat.Length; i++)
		{
			Seat[i].Reset();
		}
	}

	public void Clear()
	{
		for(int i = 0; i < Seat.Length; i++)
		{
			Seat[i].Target = null;
		}
	}
}

[System.Serializable]
public class _Seat
{
	public int Index;
	public GameObject Object;
	public GreatGrand Target;
	public bool CanBeSwapped = true;

	public Vector3 Position
	{
		get{return Object.transform.position;}
	}
	public Quaternion Rotation
	{
		get{return Object.transform.rotation;}
	}

	public Transform transform {get{
		return Object.transform;}}

	private bool highlighted = false;

	public void SetTarget(GreatGrand g)
	{
		Target = g;
	}

	public bool CanSeat(GreatGrand g)
	{
		return CanBeSwapped;
	}

	public void Highlight(bool? active = null)
	{
		bool actual = active ?? !highlighted;
		if(highlighted == actual) return;
		highlighted = actual;

		Object.transform.localScale = Vector3.one * (actual ? 1.0F : 0.8F);
	}

	public void Reset()
	{
		if(Object != null) Object.transform.localScale = Vector3.one * 0.8F;
	}
}

[System.Serializable]
public class _Food
{
	public GameObject Object;
	public FoodType Type;
}

public enum FoodType
{
	Lobster, Pizza, Curry, Fruit
}
