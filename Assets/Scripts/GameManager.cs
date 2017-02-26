using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public GameObject DinnerGame;
	public static TableManager Table;
	public TableManager _TableManager;
	public static UIManager UI;
	public UIManager _UIManager;

	public static Module Module
	{
		get{
			return instance.CurrentModule;
		}
	}
	public Module CurrentModule;
	public Module Menu, Dinner, Bowls;
	public Module [] AllModules
	{
		get{
			return new Module[] {Menu, Dinner, Bowls};
		}
	}
	public string StartingModule = "";

	public static WorldResources WorldRes
	{
		get{return Data.World;}
	}

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

	private static GameObject _worldobjects;
	public static GameObject GetWorldObjects()
	{
		if(_worldobjects == null) _worldobjects = GameObject.Find("WorldObjects");
		return _worldobjects;
	}

	public static Generator GetGenerator()
	{
		if(_generator == null)
		{
			_generator = GameObject.Find("Generator").GetComponent<Generator>();
		}
		return _generator;
	}

	public static Generator Generator
	{
		get
		{
			if(_generator == null)
			{
				_generator = GameObject.Find("Generator").GetComponent<Generator>();
			}
			return _generator;
		}
	}
	private static Generator _generator;
	public static GameData Data;
	public static InputController _Input;

	public GrandData [] Grands
	{
		get{return Data.Grands.ToArray();}
	}

	//public Generator Generator;
	public VectorObject2D GrumpLine;

	public static bool Paused = false;
	public static bool IgnoreInput = false;

	public static bool FirstTimeInitialise = true;

	private bool GivenDefaultResources = false;

	void Awake(){
		instance = this;
		Table = _TableManager;
		UI = _UIManager;
	}

	bool gameStart = false;
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 48;
		Data = this.GetComponent<GameData>();
		Data.Init();
		_Input = Camera.main.GetComponent<InputController>();
		_Input.Init();
		UI.Init();
		for(int i = 0; i < AllModules.Length; i++) AllModules[i].Init();

		Generator.LoadElements();

		CheckForFirstTime();
		
		if(Data.Grands.Count < WorldRes.Population)
		{
			for(int i = Data.Grands.Count; i < WorldRes.Population; i++)
			{
				Data.Grands.Add(Generator.GenerateGrand());
			}
		}

		for(int i = 0; i < Data.Grands.Count; i++)
		{
			Generator.Generate(Data.Grands[i]);
		}

		StartCoroutine(StartGame());
	}

	private int funds_per_hour = 15;
	// Update is called once per frame
	void Update () {
		if(CurrentModule != null) CurrentModule.ControlledUpdate();

		TimeChecks();
		if(Input.GetKeyDown(KeyCode.F1)) PlayerPrefs.SetInt("FirstTime", 0);
		if(Input.GetKeyDown(KeyCode.F2)) 
		{
			Generator.GenerateNewFace(Data.Grands[0]);
		}
	}

	public IEnumerator StartGame()
	{
		if(StartingModule == string.Empty) StartingModule = "menu";
		yield return StartCoroutine(LoadModule(StartingModule));
		yield return null;
	}

	public void InitTimeChecks()
	{
		Alerts = new List<GrandAlert>();
		for(int i = 0; i < Data.Grands.Count; i++) 
			Alerts.AddRange(Data.Grands[i].CheckTime(System.DateTime.Now));
	}

	float check = 0.0F;
	public void TimeChecks()
	{
		for(int i = 0; i < Data.Grands.Count; i++) 
			Data.Grands[i].CheckTime(System.DateTime.Now);

		if(Data.FundsHourly.Claim(() =>
		{
			if(FundsAlert == null) FundsAlert = new RewardCon("INCOMING FUNDS", "", new int [] {0,Data.Grands.Count * funds_per_hour, 0});
			else FundsAlert.Funds += Data.Grands.Count * funds_per_hour;
		}));

		if(Data.RepLast != WorldRes.Rep.Level)
		{
			RepAlert = new RewardCon(WorldRes.VillageName + " Rep Up!", "Lvl. " + WorldRes.Rep.Level, 
									new int [] {0, WorldRes.Rep.Level * 50, 1 + WorldRes.Rep.Level /5});
			Data.RepLast = WorldRes.Rep.Level;
		}

		Data.TimeLast = System.DateTime.Now;	
	}

	public List<GrandAlert> Alerts = new List<GrandAlert>();
	public RewardCon FundsAlert, RepAlert;
	public List<RewardCon> Rewards
	{
		get
		{
			List<RewardCon> fin = new List<RewardCon>();
			if(FundsAlert != null) fin.Add(FundsAlert);
			if(RepAlert != null) fin.Add(RepAlert);
			return fin;
		}
	}
	public int AlertsTotal{get{return Alerts.Count + Rewards.Count;}}

	public IEnumerator ShowAlerts()
	{
		List<GrandAlert> hunger = new List<GrandAlert>();
		List<GrandAlert> fitness = new List<GrandAlert>();
		List<GrandAlert> ageup = new List<GrandAlert>();
		//List<GrandAlert> repup = new List<GrandAlert>();

		for(int a = 0; a < Alerts.Count; a++)
		{
			switch(Alerts[a].Type)
			{
				case AlertType.Hungry: hunger.Add(Alerts[a]); break;
				case AlertType.Fitness: fitness.Add(Alerts[a]); break;
				case AlertType.Ageup: ageup.Add(Alerts[a]); break;
				//case AlertType.Repup: repup.Add(Alerts[a]); break;
			}
		}
	
		if(hunger.Count > 0) 
		{
			//for(int i = 0; i < hunger.Count ; i++)
			//{
				yield return StartCoroutine(UI.ShowGrandAlert(hunger));
			//}
		}
		if(fitness.Count > 0) 
		{
			//for(int i = 0; i < fitness.Count ; i++)
			//{
				yield return StartCoroutine(UI.ShowGrandAlert(fitness));
			//}
		}
		if(ageup.Count > 0) 
		{
			//for(int i = 0; i < ageup.Count ; i++)
			//{
				yield return StartCoroutine(UI.ShowGrandAlert(ageup));
			//}
		}

		Alerts.Clear();

		for(int i = 0; i < Rewards.Count; i++)
		{
			yield return StartCoroutine(UI.RepAlert(Rewards[i]));
		}

		RepAlert = null;
		FundsAlert = null;

	}

	public IEnumerator LoadModule(string n)
	{
		bool entry = false;
		Module target = null;

		Clear();
		n = n.ToLower();
		
		switch(n)
		{
			case "dinner":
			target = Dinner;
			break;
			case "menu":
			target = Menu;
			break;
			case "bowls":
			target = Bowls;
			break;
		}

		Module temp = CurrentModule;
		CurrentModule = target;	

		IntVector velocity = new IntVector(1,0);
		if(temp != null) velocity = new IntVector(target.Index - temp.Index,0);
		if(temp != target) 
		{
			if(temp != null) StartCoroutine(temp.Exit(-velocity));
			yield return StartCoroutine(CurrentModule.Enter(true, velocity));
		}

		yield return null;

	}

	public void CheckPopulation()
	{
		if(Data.Grands.Count < WorldRes.Population)
		{
			for(int i = Data.Grands.Count; i < WorldRes.Population; i++)
			{
				Data.Grands.Add(Generator.GenerateGrand());
			}
		}
	}

	public static void OnTouch()
	{

	}

	public static void OnRelease()
	{
		Table.Reset();
	}
	
	public void FocusOn(InputTarget t)
	{
		if(t is GreatGrand) UI.SetGrandUI(t as GreatGrand);
	}

	public static Vector2 InputPos_Screen
	{
		get{
			return Input.mousePosition;
		}
	}
	public static Vector3 InputPos;
	public static GreatGrand TargetGrand;
	public void SetTargetGrand(GreatGrand g)
	{
		if(g!= null)
		{	
			if(TargetGrand != null) TargetGrand.Release(InputPos);
			else g.Tap(InputPos);
		}
		
		UI.SetGrandUI(g);
		TargetGrand = g;
	}

	public void OnApplicationQuit()
	{
		Data.Save();
	}


	public void Clear()
	{

	}

	public void AddGrand(GrandData g)
	{
		Data.Grands.Add(g);
	}

	private void CheckForFirstTime()
	{
		FirstTimeInitialise = PlayerPrefs.GetInt("FirstTime") == 0;
		if(FirstTimeInitialise)
		{
			PlayerPrefs.SetInt("FirstTime", 1);
			for(int i = 0; i < AllModules.Length; i++)
			{
				AllModules[i].SetIntro(false);
			}

			StartCoroutine(UI.ResourceAlert(WorldRes.Meds, 10));
			StartCoroutine(UI.ResourceAlert(WorldRes.Funds, 400));
			StartCoroutine(UI.ResourceAlert(WorldRes.Rep, 0));	

			Data.SetupData();

		}
	}
}



