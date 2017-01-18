using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public GameObject DinnerGame;
	public static TableManager Table;
	public TableManager _TableManager;
	public static UIManager UI;
	public UIManager _UIManager;

	public static WorldResources WorldRes;
	public WorldResources _WorldResources;

	public static Transform GetCanvas()
	{
		if(UI != null) return UI.Canvas.transform;
		else return GameObject.Find("Canvas").transform;
	}
	public static UIObj GetFaceParent()
	{
		if(UI != null) return UI.FaceParent;
		else return GameObject.Find("FaceParent").GetComponent<UIObj>();
	}

	public static GameData Data;

	public InputController _Input;
	public GreatGrand [] GG;

	public static int GG_num = 8;

	public Generator GGGen;
	public VectorObject2D GrumpLine;

	public bool loadDinner = false;

	public bool Resolved
	{
		get{
			for(int i = 0; i < GG.Length; i++)
			{
				if(GG[i]==null) continue;
				if(!GG[i].IsHappy) return false;
			}
			return true;
		}
	}

	public int NumberHappy
	{
		get{
			int num = 0;
			for(int i = 0; i < GG.Length; i++)
			{
				if(GG[i].IsHappy) num++;
			}
			return num;
		}
	}

	public bool AllSeated
	{
		get{
			for(int i = 0; i < GG.Length; i++)
			{
				if(!GG[i].isSeated) return false;
			}
			return true;
		}
	}

	void Awake(){
		instance = this;
		Table = _TableManager;
		UI = _UIManager;
		
	}

	bool gameStart = false;
	// Use this for initialization
	void Start () {
		Data = this.GetComponent<GameData>();
		WorldRes = _WorldResources;
		WorldRes.Init();
		Table.Init();
		_Input.Init();
		GGGen.LoadElements();
		UI.Init();

		if(loadDinner) StartCoroutine(LoadModule("dinner"));
		else StartCoroutine(LoadModule("menu"));
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.W)) UI.Quote("Hi!", "Carer");
	}


	public void Clear()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			GG[i].Destroy();
		}
	}

	public IEnumerator LoadModule(string n)
	{
		Clear();
		n = n.ToLower();
		switch(n)
		{
			case "dinner":
			yield return new WaitForSeconds(0.5F);
			yield return StartCoroutine(UI.Module(UI.Module_Dinner));
			//yield return StartCoroutine(
			//	UI.SetModule(UI.Module_Dinner);
			//	);
			yield return new WaitForSeconds(0.6F);
			 CreateDinnerGame();
			 DinnerGame.SetActive(true);
			break;
			case "menu":
			yield return new WaitForSeconds(0.5F);
			yield return StartCoroutine(UI.Module(UI.Module_Menu));
			//yield return new WaitForSeconds(0.25F);
				
			
			break;
		}
		yield return null;
	}

	public void ExitMinigame()
	{
		StartCoroutine(UI.Module(UI.Module_Menu));
	}


	public void CreateDinnerGame()
	{
		gameStart = false;
		GG = new GreatGrand[GG_num];
		for(int i = 0; i < GG_num; i++)
		{
			GG[i] = GGGen.Generate(i);//(GreatGrand) Instantiate(GGObj);
			GGGen.GenerateFace(GG[i]);
			GG[i].transform.SetParent(this.transform);
			//GG[i].Face.SetActive(false);
		}


		for(int i = 0; i < GG_num; i+=2)
		{
			MaritalStatus m = Random.value > 0.55F ? MaritalStatus.Married : (Random.value < 0.9F ? MaritalStatus.Divorced : MaritalStatus.Donor);
			GG[i].Info.MStat = m;
			GG[i+1].Info.MStat = m;
			GG[i].Relation = GG[i+1];
			GG[i+1].Relation = GG[i];
		}

		for(int i = 0; i < GG_num; i++)
		{
			//GG[i].Generate(i);
			GG[i].SitImmediate(Table.Seat[i]);
		}

		for(int i = 0; i < GG_num; i++)
		{
			//GenerateGrumpsPrimitive(GG[i], 2);
			GenerateGrumpsReal(GG[i], 1);

		}
		int [] ind = new int[0];
		while(NumberHappy > 3) ind = ShuffleGG();

		StartCoroutine(StartDinnerGame(ind));

		
	}

	IEnumerator StartDinnerGame(int [] index)
	{
		UI.WinMenu.SetActive(false);
		for(int i = 0; i < GG.Length; i++)
		{
			GG[i].Face.transform.position = Table.Door.position;
		}

		for(int i = GG.Length-1; i >= 0; i--)
		{
			StartCoroutine(Table.DoorToSeat(GG[i], index[i], 0.7F));
		}

		while(!AllSeated) yield return null;

		gameStart = true;
		
		yield return null;
	}



	public static void OnTouch()
	{

	}

	public static void OnRelease()
	{
		Table.Reset();
	}

	VectorLine TLine;
	[SerializeField]
	private Color TLineColor;
	public void TargetLine(Vector3 from, Vector3 to)
	{
		if(TLine == null)
		{
			TLine = new VectorLine("Targeter", new List<Vector3>(), 7.0F, LineType.Continuous);
			TLine.points3.Add(from);
			TLine.points3.Add(to);
			TLine.SetColor(TLineColor);
		}
		TLine.points3[0] = from;
		TLine.points3[1] = to;
		TLine.Draw();
	}

	public void CompleteDinner()
	{
		if(!AllSeated || !gameStart) return;
		StartCoroutine(CompleteGame());
	}

	IEnumerator CompleteGame()
	{
		UI.WinMenu.TweenActive(true);
		UI.WinMenu[0].SetActive(false);
		UI.WinMenu[1].SetActive(false);

		int rep = 0;
		int grumptotal = 0;
		UIObj total = UI.WinMenu[1];
		UI.WinMenu.Txt[0].text = "Let's Eat!";
		UI.WinMenu.Txt[1].text = "";
		total.Txt[0].text = grumptotal + "";

		yield return new WaitForSeconds(0.6F);

		UI.WinMenu.Txt[1].text = "HAPPY GRANDS";
		Tweens.Bounce(UI.WinMenu.Txt[1].transform);
		UI.WinMenu[1].TweenActive(true);
		yield return new WaitForSeconds(0.2F);

		for(int i = 0; i < Table.Seat.Length; i++)
		{
			if(Table.Seat[i].Target == null) continue;

			yield return StartCoroutine(Table.Seat[i].Target.EmotionRoutine(false));

			int init = grumptotal;
			int g = Table.Seat[i].Target.GrumpMeter;
			if(g > 0) grumptotal = Mathf.Clamp(grumptotal + g, 0, 15);
			
			total.Txt[0].text = grumptotal + "";

			//scale the UI based on difference between previous total and current total. bigger for a high score, lower for small
			float scalediff = 0.05F;
			int totaldiff = grumptotal - init;
			float scaleactual = scalediff * totaldiff;
			if(totaldiff > 0) Tweens.Bounce(total.transform);

			yield return new WaitForSeconds(Time.deltaTime  * 14);
		}

		rep = grumptotal * 10;
		UI.WinMenu.Txt[1].text = "VILLAGE REP";
		Tweens.Bounce(UI.WinMenu.Txt[1].transform);
		total.Txt[0].text = rep + "";
		float repscale = 1.05F + ((float)rep / 100);
		repscale = Mathf.Clamp(repscale, 1.05F, 1.4F);

		Tweens.Bounce(total.transform, Vector3.one * repscale);

		yield return StartCoroutine(UI.ResourceAlert(WorldRes.Rep, rep));
		
		EndGame();
	}

	public void EndGame()
	{
		gameStart = false;
		UI.ShowEndGame();
	}

	public void DestroyGame()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			Destroy(GG[i].gameObject);
		}
		Clear();
		Table.Clear();
		CreateDinnerGame();
	}

	public void GenerateGrumpsPrimitive(GreatGrand g, int num)
	{
		g.Grumps = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			GrumpObj targ = GetRandomGG(g);
			_Grump newg = new _Grump(false, g, targ);
			g.Grumps[i] = newg;
		}
	}

	public void GenerateGrumpsReal(GreatGrand g, int num)
	{
		_Grump [] final = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			bool has_targ = false;

			GrumpObj targ = null;

			while(!has_targ)
			{
				targ = GetRandomGG(g);
				has_targ = true;

				for(int x = 0; x < i; x++)
				{
					if(final[x].Target == targ) has_targ = false;
				}
			}
			int dist = Table.SeatDistance(targ as GreatGrand, g);
			bool like = false;
			if(dist == 1) like = true;
			

			final[i] = new _Grump(like, g, targ);	
		}
		g.Grumps = final;	
	}

	public int [] ShuffleGG()
	{	
		int [] fin = new int[Table.Seat.Length];
		List<_Seat> finalpos = new List<_Seat>();
		finalpos.AddRange(Table.Seat);

		for(int i = 0; i < GG_num; i++)
		{
			int num = Random.Range(0, finalpos.Count);
			_Seat point = finalpos[num];
			if(point == GG[i].Seat)
			{
				num = Random.Range(0, finalpos.Count);
				point = finalpos[num];
			}

			fin[i] = finalpos[num].Index;
			GG[i].SitImmediate(point);
			finalpos.RemoveAt(num);

		}

		return fin;
	}

	public GreatGrand GetRandomGG(GreatGrand g)
	{
		GreatGrand final = GG[Random.Range(0, GG.Length)];
		while(final == g) final = GG[Random.Range(0, GG.Length)];
		return final;
	}

	public GreatGrand GetNonNeighbourGG(GreatGrand g)
	{
		return Table.GetNonNeighbourSeat(g.Seat).Target;
	}

	public List<_Grump> GetRelatedGrumps(GreatGrand g)
	{
		List<_Grump> fin = new List<_Grump>();
		for(int i = 0; i < GG.Length; i++)
		{
			if(GG[i] == g) continue;
			for(int x = 0; x < GG[i].Grumps.Length; x++)
			{
				if(GG[i].Grumps[x].Target == g) fin.Add(GG[i].Grumps[x]);
			}
		}
		return fin;
	}

	public void FocusOn(InputTarget t)
	{
		if(t is GreatGrand) UI.SetGrandUI(t as GreatGrand);
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

	public void TraceLine(float r)
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
	}

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

