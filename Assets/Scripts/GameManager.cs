using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static TableManager Table;
	public TableManager _TableManager;
	public static UIManager UI;
	public UIManager _UIManager;

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
	public LineRenderer GrumpLine;

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

	void Awake(){
		instance = this;
		Table = _TableManager;
		UI = _UIManager;
	}

	bool gameStart = false;
	// Use this for initialization
	void Start () {
		Data = this.GetComponent<GameData>();
		Table.Init();
		_Input.Init();
		GGGen.LoadElements();
		CreateGame();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.W)) GGGen.GenerateFace(GG[0]);
	}


	public void CreateGame()
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
			GG[i].SitAt(Table.Seat[i]);
		}

		for(int i = 0; i < GG_num; i++)
		{
			//GenerateGrumpsPrimitive(GG[i], 2);
			GenerateGrumpsReal(GG[i], 1);

		}

		while(NumberHappy > 3) ShuffleGG();

		for(int i = 0; i < GG_num; i++)
		{
			GG[i].CreateGrumpLines();
		}

		gameStart = true;
		UI.ShowEndGame(false);
	}

	public void EndGame()
	{
		gameStart = false;
		UI.ShowEndGame(true);
	}

	public void DestroyGame()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			Destroy(GG[i].gameObject);
		}
		Table.Clear();
		CreateGame();

	}

	public static void OnTouch()
	{

	}

	public static void OnRelease()
	{
		Table.Reset();
	}

	public void CheckGrumps()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			if(GG[i] == null) continue;
			GG[i].CheckEmotion();
		}

		if(Resolved && gameStart) EndGame();
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

	public void ShuffleGG()
	{
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

			GG[i].SitAt(point);
			finalpos.RemoveAt(num);
		}
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

	public _Grump(bool like, GreatGrand p,  GrumpObj t= null)
	{
		Parent = p;
		Target = t;
		LikesIt = like;
	}

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

