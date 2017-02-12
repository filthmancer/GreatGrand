using UnityEngine;
using System.Collections;
using System;

public class Utility : MonoBehaviour {

	public static int RandomInt(int a)
	{
		return (int) UnityEngine.Random.Range(0, a);
	}

	public static int RandomInt(float a){
		return (int) UnityEngine.Random.Range(0, a);
	}
	public static int RandomIntInclusive(float a){
		return (int) UnityEngine.Random.Range(-a, a);
	}

	public static Vector3 RandomVector(float x = 0.0F, float y = 0.0F, float z = 0.0F)
	{
		return new Vector3(UnityEngine.Random.Range(0, x), UnityEngine.Random.Range(0, y), UnityEngine.Random.Range(0,z));
	}

	public static Vector2 RandomVector2(float x = 0.0F, float y = 0.0F)
	{
		return new Vector2(UnityEngine.Random.Range(0, x), UnityEngine.Random.Range(0, y));
	}

	public static Vector2 RandomMatrixPoint(Simple2x2 s)
	{
		return new Vector2(UnityEngine.Random.Range(s.min.x, s.max.x),
							UnityEngine.Random.Range(s.min.y, s.max.y));
	}

	public static Vector3 RandomVectorInclusive(float x = 0.0F, float y = 0.0F, float z = 0.0F)
	{
		return new Vector3(UnityEngine.Random.Range(-x, x), UnityEngine.Random.Range(-y, y), UnityEngine.Random.Range(-z,z));
	}

	public static int [] IntNormal(int [] x, int [] y)
	{
		int a = y[0] - x[0];
		int b = y[1] - x[1];
		a = a >= 1 ? 1 : (a <= -1 ? -1 : 0);
		b = b >= 1 ? 1 : (b <= -1 ? -1 : 0);
		return new int [] 
		{
			a,
			b
		};
	}

	public static void Flog(params object [] s)
	{
		string final = "";
		for(int i = 0; i < s.Length; i++)
		{
			final += s[i].ToString();
			if(i < s.Length-1) final += " : ";
		}
		Debug.Log(final);
	}

}

[System.Serializable]
public class IntVector
{
	//public static IntVector zero = new IntVector(0,0);
	public int x, y;
	public int this[int v]
	{
		get
		{
			if(v == 0) return x;
			else if(v == 1) return y;
			else return 0;
		}
		
	}
	public IntVector(int a, int b)
	{
		x = a;
		y = b;
	}

	public IntVector(float a, float b)
	{
		x = (int) a;
		y = (int) b;
	}
	public IntVector(IntVector a)
	{
		x = a.x;
		y = a.y;
	}

	public IntVector(int a)
	{
		x = a;
		y = a;
	}

	public IntVector(Vector2 v)
	{
		x = (int) v.x;
		y = (int) v.y;
	}

	public static IntVector operator -(IntVector v)
	{
	    return new IntVector(-v.x, -v.y);
	}

	public string ToString() {return x + ":" + y;}

	public static IntVector operator + (IntVector a, IntVector b)
	{
		return new IntVector(b.x+a.x, b.y+a.y);
	}

	public bool Equals(IntVector b) {return x == b.x && y == b.y;}

	public Vector2 ToVector2
	{
		get{
			return new Vector2(x, y);
		}
	}

	public int [] ToInt
	{
		get{
			return new int[] {x,y};
		}
	}

	public void Add(IntVector a)
	{
		x += a.x;
		y += a.y;
	}

	public void Sub(IntVector a)
	{
		x -= a.x;
		y -= a.y;
	}



	public void Mult(float m)
	{
		x = (int)((float)x*m);
		y = (int)((float)y*m);
	}
}

public class Simple2x2
{
	public static Simple2x2 zero
	{get{return new Simple2x2();}}
	public Vector2 min, max;
	public Simple2x2(float _x = 0.0F, float _y = 0.0F, float _w = 0.0F, float _z = 0.0F)
	{
		min = new Vector2(_x, _y);
		max = new Vector2(_w, _z);
	}

	public Rect ToRect()
	{
		return new Rect(min.x, min.y, max.x, max.y);
	}

	public bool ContainsX(float x){return x > min.x && x < max.x;}
	public bool ContainsY(float y){return y > min.y && y < max.y;}
	public bool Contains(Vector2 m) {return ContainsX(m.x) && ContainsY(m.y);}

	public float RandX() {return UnityEngine.Random.Range(min.x, max.x);}
	public float RandY() {return UnityEngine.Random.Range(min.y, max.y);}
}

[System.Serializable]
public class ToggleContainer
{
	public bool Active;
	public string Name;
	public ToggleContainer(string n, bool a)
	{
		Name = n;
		Active = a;
	}
}

[System.Serializable]
public class ToggleList
{
	public ToggleContainer [] Togg;
	public ToggleContainer this[string n]
	{
		get{
			for(int i = 0; i < Togg.Length; i++){
				if(string.Equals(Togg[i].Name, n)) 
					return Togg[i];
			}
			return null;
		}
	}
	public ToggleContainer this[int i] {get{return Togg[i];}}
	public int Length{get{return Togg.Length;}}

	public ToggleList(params string [] s)
	{
		Togg = new ToggleContainer[s.Length];
		for(int i =0 ; i < Togg.Length; i++)
		{
			Togg[i] = new ToggleContainer(s[i], false);
		}
	}
}