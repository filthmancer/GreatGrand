using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class TableManager : MonoBehaviour {
	public Transform TableObj;
	public _Seat [] Seat;
	public _Food [] Food;

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


		Movement = new VectorLine("Movement Path", new List<Vector2>(mvmt_segments+1), 4.0F, LineType.Continuous);

		Movement.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, true);
		Movement.Draw();
	}

	public Vector3 GetMovementPoint(float ratio)
	{
		int index = 0;
		Vector2 a = Movement.GetPoint01(ratio, out index);
		Vector3 fin = new Vector3(a.x, 0.0F, a.y);
		return fin;
	}

	public Vector3 GetMovementPointAtSeat(int s)
	{
		float norm = (float)s/(float)Seat.Length;
		Vector2 mpos = Movement.GetPoint01(norm);
		Vector3 fin = new Vector3(mpos.x, 0.0F, mpos.y);
		return fin;
	}

	public float GetMovementNormalAtSeat(int s)
	{
		float norm = (float)s/(float)Seat.Length;
		return norm;
	}

	public IEnumerator MoveSeat(Transform t, int from, int to, float time)
	{
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
		float norm = start_norm;
		float norm_rate = movenorm / time;

		float getup_init = 0.3F;
		float getup_time = 0.0F;
		while((getup_time += Time.deltaTime) < getup_init)
		{
			t.position = Vector3.Lerp(Seat[from].transform.position, start, getup_time/getup_init);
			t.LookAt(TableObj.transform.position);
			t.rotation *= Quaternion.Euler(65, 0,180);
			yield return null;
		}

		while((timer += Time.deltaTime) < time)
		{
			t.position = GetMovementPoint(norm);
			t.LookAt(TableObj.transform.position);
			t.rotation *= Quaternion.Euler(65, 0,180);

			norm += norm_rate * Time.deltaTime;
			if(norm < 0.0F) norm = 1.0F;
			if(norm > 1.0F) norm = 0.0F;
			yield return null;
		}

		float sitdown_init = 0.3F;
		float sitdown_time = 0.0F;
		while((sitdown_time += Time.deltaTime) < sitdown_init)
		{
			t.position = Vector3.Lerp(end, Seat[to].transform.position, sitdown_time/sitdown_init);
			t.LookAt(TableObj.transform.position);
			t.rotation *= Quaternion.Euler(65, 0,180);

			yield return null;
		}

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
		for(int h = GameManager.GG_num-1; h > max; h--)
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
		if(ai > bi + 4) bi += 8;
		if(bi > ai + 4) ai += 8;
		return Mathf.Abs(ai - bi);
	}

	public _Seat NearestSeat(Vector3 pos)
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
		get{return Object.transform.position + Vector3.up*0.5F;}
	}
	public Quaternion Rotation
	{
		get{return Object.transform.rotation;}
	}

	public Transform transform {get{return Object.transform;}}

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
		Object.transform.localScale = Vector3.one * 0.8F;
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
