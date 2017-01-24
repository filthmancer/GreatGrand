using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;

public class GameData : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		//StartCoroutine(UI.QuoteRoutine("Carer", "Hi!", "How are you?"));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


[System.Serializable]
public class WorldResources
{
	public string VillageName;
	public Resource Rep, Funds, Meds;
	public Resource [] AllRes
	{
		get{return new Resource[]{Rep, Funds, Meds};}
	}
	public int Length{get{return AllRes.Length;}}
	public WorldResources()
	{
		Rep = new Resource(0);
		Funds = new Resource(1);
		Meds = new Resource(2);
	}
	public void Init()
	{
		Rep.Index = 0;
		Rep.Name = "Rep";
		Funds.Index = 1;
		Funds.Name = "Funds";
		Meds.Index = 2;
		Meds.Name = "Meds";

		Rep.Current = PlayerPrefs.GetInt("Rep");
		Funds.Current = PlayerPrefs.GetInt("Funds");
		Meds.Current = PlayerPrefs.GetInt("Meds");
	}

	public void Save()
	{
		PlayerPrefs.SetInt("Rep", Rep.Current);
		PlayerPrefs.SetInt("Funds", Funds.Current);
		PlayerPrefs.SetInt("Meds", Meds.Current);
	}

	public Resource this[int v]
	{
		get{return AllRes[v];}
	}
}

[System.Serializable]
public class GrandResources
{
	public Resource Smiles, Grumps;
	public Resource Fit, Slob;
}

[System.Serializable]
public class Resource
{
	public int Value
	{
		get{
			return (int)((float)Current * Multiplier);
		}
	}

	public float Multiplier;
	public int Current;

	public Color Col;
	[HideInInspector]
	public int Index;
	public string Name;
	private int Max = 99999;


	public Resource(int ind)
	{
		Current = 1;
		Multiplier = 1.0F;
		Col = Color.white;
		Index = ind;
	}

	public bool Charge(int n)
	{
		if(Current > n)
		{
			Add(-n);

			return true;
		}
		else return false;
	}

	public void Add(int n)
	{
		Current += n;
		GameManager.UI.CheckResourcesUI();
	}
}

[System.Serializable]
public class _Grump
{
	public bool LikesIt;
	public GreatGrand Parent;
	public GrumpObj Target;
	public VectorLine Line;
	private VectorLine Arrow;
	private float line_time = 0.0F;

	public _Grump(bool like, GreatGrand p,  GrumpObj t= null)
	{
		Parent = p;
		Target = t;
		LikesIt = like;

		Line = new VectorLine("Grump - " + Parent + ":" + Target, new List<Vector3>(), 4.5F, LineType.Discrete, Joins.Weld);
		Vector3 a = Parent.Face.transform.position;
		Vector3 b = Target.transform.position;
		Vector3 vel = b - a;
		vel.Normalize();

		float d = Vector3.Distance(a,b);
		int steps = (int) (d/0.3F);
		
		Line.points3.Add(Vector3.Lerp(a, b, 0.15F));
		Line.points3.Add(Vector3.Lerp(a, b, 0.85F));
		Line.SetColor(new Color(0,0,0,0));
		Line.Draw();

		Arrow = new VectorLine("Arrow - Grump - " + Parent + ":" + Target, new List<Vector3>(), 7.0F, LineType.Continuous, Joins.Weld);
		Vector3 point  = Vector3.Lerp(a, b, 0.85F);
		Arrow.points3.Add(point - (vel*0.5F) + (Vector3.Cross(vel, -vel) * 0.5F));
		Arrow.points3.Add(point);
		Arrow.points3.Add(point - (vel*0.5F) - (Vector3.Cross(vel, -vel) * 0.5F));
		Arrow.SetColor(new Color(0,0,0,0));
		Arrow.Draw();
	}

	public void Update()
	{
		if((line_time -= Time.deltaTime) > -0.1F)
		{
			Color c = (LikesIt ? Color.green : Color.red);
			float a = (line_time > 0.0F) ? line_time * 3 : 0.0F;
			c.a = Mathf.Clamp01(a);
			Line.SetColor(c);
			Line.Draw();
			Arrow.SetColor(c);
			Arrow.Draw();
		}
	}

	public void Trace(float spawn, float fade = 1.3F)
	{
		Color c = (LikesIt ? Color.green : Color.red);
		c.a = 1.0F;

		Line.SetColor(c);

		Vector3 startpos = Parent.Position;
		Vector3 endpos = Target.Position;
		Vector3 vel = endpos - startpos;
		vel.Normalize();

		Vector3 start = Vector3.Lerp(startpos, endpos, 0.0F);
		Vector3 point = Vector3.Lerp(startpos, endpos, 0.85F);

		Line.points3[0] = start;
		Line.points3[1] = start;
		Line.Draw();

		DOTween.To(()=> Line.points3[1], x=>Line.points3[1] = x, point, spawn);

		Vector3 arrowoffset = Vector3.Cross(vel, Vector3.up).normalized * 0.2F;
		Vector3 arrowpushback = point - (vel*0.2F);

		Arrow.points3[0] = start;
		Arrow.points3[1] = start;
		Arrow.points3[2] = start;

		DOTween.To(()=>Arrow.points3[0], x=>Arrow.points3[0] = x, arrowpushback + arrowoffset, spawn);
		DOTween.To(()=>Arrow.points3[2], x=>Arrow.points3[2] = x, arrowpushback - arrowoffset, spawn);
		DOTween.To(()=>Arrow.points3[1], x=>Arrow.points3[1] = x, point, spawn);

		Arrow.SetColor(c);
		Arrow.Draw();

		SetLineTime(fade);
	}

	/*public void TraceLine(float r)
	{
		Color c = (LikesIt ? Color.green : Color.red);
		c.a = 1.0F;

		Line.SetColor(c);

		Vector3 startpos = Parent.Face.transform.position;
		Vector3 endpos = Target.transform.position;
		Vector3 vel = endpos - startpos;
		vel.Normalize();

		float ratio = 0.15F + (r * 0.7F);
		Line.points3[0] = Vector3.Lerp(startpos, endpos, 0.15F);
		Line.points3[1] = Vector3.Lerp(startpos, endpos, ratio);
		Line.Draw();

		Vector3 point  = Vector3.Lerp(startpos, endpos, ratio);
		Arrow.points3[0] = (point -(vel*0.2F) + (Vector3.Cross(vel, Vector3.up).normalized * 0.2F));
		Arrow.points3[1] = point;
		Arrow.points3[2] = (point -(vel*0.2F) - (Vector3.Cross(vel, Vector3.up).normalized * 0.2F));
		Arrow.SetColor(c);
		Arrow.Draw();

		SetLineTime(1.2F);
	}*/

	public void Destroy()
	{
		VectorLine.Destroy(ref Line);
	}

	public void SetLineTime(float t){line_time = t;}

	public bool Resolved
	{
		get
		{
			int dist = GameManager.Table.SeatDistance(Parent,Target);
			if(LikesIt && dist == 1) return true;
			if(!LikesIt && dist > 1) return true;
			return false;
		}
	}
}


public enum ResourceType
{
	Rep, Funds, Meds, Smiles, Grumps, Fit, Slob
}

