using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TableManager : MonoBehaviour {
	public Transform TableObj;
	public _Seat [] Seat;
	public _Food [] Food;

	public void Init()
	{
		for(int i = 0; i < Seat.Length; i++)
		{
			Seat[i].Index = i;
		}
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
		get{return Object.transform.position + Vector3.up*3;}
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
