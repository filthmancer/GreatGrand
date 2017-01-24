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

	public Generator Generator;
	public VectorObject2D GrumpLine;

	public bool loadDinner = false;

	public static bool Paused = false;
	public static bool IgnoreInput = false;

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

		WorldRes = _WorldResources;
		WorldRes.Init();

		WorldRes.VillageName = "Tall Trees";
	
		_Input.Init();
		Generator.LoadElements();
		UI.Init();

		Menu.Init();
		Dinner.Init();

		if(loadDinner) StartCoroutine(LoadModule("dinner"));
		else StartCoroutine(LoadModule("menu"));

		GivenDefaultResources = PlayerPrefs.GetInt("DefRes") == 1;
		if(!GivenDefaultResources || WorldRes.Meds.Current == 0)
		{
			StartCoroutine(UI.ResourceAlert(WorldRes.Meds, 25));
			StartCoroutine(UI.ResourceAlert(WorldRes.Funds, 100));
			StartCoroutine(UI.ResourceAlert(WorldRes.Rep, 0));	
			PlayerPrefs.SetInt("DefRes", 1);
		}
	}
	
	// Update is called once per frame
	void Update () {
		CurrentModule.ControlledUpdate();
	}

	public void OnApplicationQuit()
	{
		WorldRes.Save();
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

	public void ExitMinigame()
	{
		//StartCoroutine(UI.Module(UI.Module_Menu));
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

	
	public void FocusOn(InputTarget t)
	{
		if(t is GreatGrand) UI.SetGrandUI(t as GreatGrand);
	}
}



