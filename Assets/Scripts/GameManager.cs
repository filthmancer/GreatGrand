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
	public Module Menu, Dinner;
	public Module [] AllModules
	{
		get{
			return new Module[] {Menu, Dinner};
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

	public static Generator GetGenerator()
	{
		if(_generator == null)
		{
			_generator = GameObject.Find("Generator").GetComponent<Generator>();
		}
		return _generator;
	}

	private static Generator _generator;


	public static GameData Data;

	public InputController _Input;
	public GrandData [] Grands
	{
		get{return Data.Grands.ToArray();}
	}

	public Generator Generator;
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
		Data = this.GetComponent<GameData>();
		Data.Init();
		_Input.Init();
		UI.Init();
		for(int i = 0; i < AllModules.Length; i++) AllModules[i].Init();

		Generator.LoadElements();

		FirstTimeInitialise = PlayerPrefs.GetInt("FirstTime") == 0;
		if(FirstTimeInitialise)
		{
			//PlayerPrefs.SetInt("FirstTime", 1);

			WorldRes.VillageName = "Tall Trees";
			WorldRes[0].Name = "Rep";
			//WorldRes[0].Col = Color.red;
			WorldRes[0].Set(0);
			(WorldRes[0] as Stat).SetLevel(1);

			WorldRes[1].Name = "Funds";
			//WorldRes[1].Col = Color.green;
			WorldRes[1].Set(0);

			WorldRes[2].Name = "Meds";
			//WorldRes[2].Col = Color.blue;
			WorldRes[2].Set(0);

			//StartCoroutine(UI.ResourceAlert(WorldRes.Meds, 25));
			//StartCoroutine(UI.ResourceAlert(WorldRes.Funds, 100));
			//StartCoroutine(UI.ResourceAlert(WorldRes.Rep, 0));	

			Data.Grands = new List<GrandData>();

			for(int i = 0; i < WorldRes.Population; i++)
			{
				AddGrand(Generator.GenerateGrand());
			}
		}
		
		if(Data.Grands.Count == 0)
		{
			for(int i = 0; i < WorldRes.Population; i++)
			{
				AddGrand(Generator.GenerateGrand());
			}
		}
		else
		{
			for(int i = 0; i < Data.Grands.Count; i++)
			{
				Data.Grands[i].GrandObj = Generator.Generate(Data.Grands[i]);
			}
		}
		
		if(StartingModule == string.Empty) StartingModule = "menu";
		StartCoroutine(LoadModule(StartingModule));
	}

	
	// Update is called once per frame
	void Update () {
		if(CurrentModule != null) CurrentModule.ControlledUpdate();
		if(Input.GetKeyDown(KeyCode.F1)) PlayerPrefs.SetInt("FirstTime", 0);
		if(Input.GetKeyDown(KeyCode.F2)) StartCoroutine(UI.ResourceAlert(WorldRes.Rep, 50));
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

	public static Vector3 InputPos;
	public static GreatGrand TargetGrand;
	public void SetTargetGrand(GreatGrand g)
	{
		if(g == null) TargetGrand.Release(InputPos);
		else g.Tap(InputPos);
		UI.SetGrandUI(g);
		TargetGrand = g;
	}
}



