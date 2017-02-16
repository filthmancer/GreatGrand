
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;
using Scumworks;
using Filthworks;

public class GameData : MonoBehaviour {

	public WorldResources World;
	public List<GrandData> Grands;

	public FIRL [] Frames;
	public FIRL RandomFrame()
	{
		return Frames[Random.Range(0, Frames.Length)];
	}

	public System.DateTime TimeLast;
	public int RepLast = 0;

	public GoodBoy FundsHourly;

	public SaveData Save_Data;
	private string Save_Location; 
    private string Save_File = "SaveData"; 
    private string Save_Target
    {
    	get{
    		return System.IO.Path.Combine(Save_Location, Save_File + ".uml") ;
    		//Save_Location + "\\" + Save_File;
    	}
    }

    public bool Alert_Letter = false,
    			Alert_Pigeonhole = false,
    			Alert_ScrollUp = false;

 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init()
	{
		Debug.Log("INITIALISING");
		World.Init();
		Save_Location=Application.persistentDataPath;
		
		Load();

		for(int i = 0; i < Grands.Count; i++)
		{
			for(int a = 0; a < Grands[i].Resources.Length; a++)
			{
				Grands[i].Resources[a].TimeLast = TimeLast;
			}
		}
		Alert_Pigeonhole = PlayerPrefs.GetInt("Alert_Pigeonhole") == 1;
		Alert_ScrollUp = PlayerPrefs.GetInt("Alert_ScrollUp") == 1;
		Alert_Letter = PlayerPrefs.GetInt("Alert_Letter") == 1;
	}

	public void SetupData()
	{
		World.VillageName = "Tall Trees";
		World[0].Name = "Rep";
		//World[0].Col = Color.red;
		World[0].Set(0);
		(World[0] as Stat).SetLevel(1);
		RepLast = 1;

		World[1].Name = "Funds";
		//World[1].Col = Color.green;
		World[1].Set(0);

		World[2].Name = "Meds";
		//World[2].Col = Color.blue;
		World[2].Set(0);

		FundsHourly = new GoodBoy(TimeLast, new System.TimeSpan(1, 0, 0));

		Grands.Clear();
		for(int i = 0; i < World.Population; i++)
		{
			Grands.Add(GameManager.Generator.GenerateGrand());
		}
	}

	private string [] f_inf = new string[]
	{
		"Base", "Eye", "Ear", "Brow", "Hair", "Jaw", "Nose"
	};

	public void Save()
	{

		PlayerPrefs.SetInt("Alert_Pigeonhole", Alert_Pigeonhole ? 1 : 0);
		PlayerPrefs.SetInt("Alert_ScrollUp", Alert_ScrollUp ? 1 : 0);
		PlayerPrefs.SetInt("Alert_Letter", Alert_Letter ? 1 : 0);

		Save_Data = new SaveData(Save_File); 

		Save_Data["Time"] = System.DateTime.Now;

		//Saving World
		Save_Data["World-Resources"] = World.Names;
		for(int i = 0; i < World.Length; i++)
		{
			Save_Data["World-" + World[i].Name+"-Current"] = World[i].Current;
			Save_Data["World-" + World[i].Name+"-Multiplier"] = World[i].Multiplier;
			Save_Data["World-" + World[i].Name+"-Index"] = World[i].Index;
			Save_Data["World-" + World[i].Name+"-Max"] = World[i].Max;
			Save_Data["World-" + World[i].Name+"-Col"] = World[i].Col;
			if(World[i] is Stat)
			{
				Save_Data["World-"+World[i].Name+"-Level"] = (World[i] as Stat).Level;
			}
		}

		//Saving Grands
		GrandData [] prev_GG = GameManager.instance.Grands;
		System.Guid [] prevgrands = new System.Guid[prev_GG.Length];
		for(int i = 0; i < prev_GG.Length; i++)
		{
			string pref = "Grand:" + prev_GG[i].Hex.ToString();

			GrandData Data = prev_GG[i];
			prevgrands[i] = Data.Hex;

			Save_Data[pref+"-Name"] = Data.Info.Name;
			Save_Data[pref+"-Gender"] = Data.Info.Gender;
			Save_Data[pref+"-Pupils"] = Data.Info.PupilScale;
			Save_Data[pref+"-Nation"] = Data.Info.Nation;

			Save_Data[pref+"-C_Skin"] = Data.Info.C_Skin;
			Save_Data[pref+"-C_Hair"] = Data.Info.C_Hair;
			Save_Data[pref+"-C_Eye"] = Data.Info.C_Eye;
			Save_Data[pref+"-C_Offset"] = Data.Info.C_Offset;
			Save_Data[pref+"-C_Nose"] = Data.Info.C_Nose;

			for(int a = 0; a < f_inf.Length; a++)
			{
				FaceInfo f = Data.GetFaceInfo(f_inf[a]);
				string s = pref+"-"+f_inf[a];
				Save_Data[s+":Index"] = f.Index;
				Save_Data[s+":Values"] = f.Values;
				Save_Data[s+":Colour"] = f.Colour;
			}

			for(int r = 0; r < Data.Resources.Length; r++)
			{
				Resource res = Data.Resources[r];
				string s = pref + "-Res " + r;

				Save_Data[s+":Current"] = res.Current;
				Save_Data[s+":Max"] = res.Max;
				Save_Data[s+":Index"] = res.Index;
				Save_Data[s+":Rate"] = res.Rate;

				Save_Data[s+":SpanHours"] = res.Span.TotalHours;
				Save_Data[s+":SpanMins"] = res.Span.Minutes;
				Save_Data[s+":SpanSecs"] = res.Span.TotalSeconds;
				Save_Data[s+":TimeLast"] = res.TimeLast;
			}
		}

		Save_Data["Rep Last"] = RepLast;
		Save_Data["Prev Grands"] = prevgrands;
		Save_Data.Save(Save_Target);

		print("Saved at " + Save_Target);
		print("Saved info: " + prevgrands.Length + " grands");
	}

	public void Load()
	{
		Debug.Log("---- LOADING FROM " + Save_Target);
		Grands = new List<GrandData>();

		if(!System.IO.File.Exists(Save_Target))
		{
			Debug.Log("COULDN'T FIND SAVE FILE");
			SetupData();
			Save();
			return;
		}

		Save_Data =  SaveData.Load(Save_Target);
		Debug.Log("--LOADING SAVE DATA: " + Save_Data);
		if(Save_Data == null) 
		{
			Debug.Log("COULDN'T FIND SAVE FILE");
			SetupData();
			return;
		}

		if(!Save_Data.TryGetValue<System.DateTime>("Time", out TimeLast)) 
		{
			TimeLast = System.DateTime.Now;
		}

		FundsHourly = new GoodBoy(TimeLast, new System.TimeSpan(1, 0, 0));
		//Loading World
		string [] s;
		if(Save_Data.TryGetValue<string[]>("World-Resources", out s))
		{
			//World = new WorldResources();
			for(int i = 0; i < s.Length; i++)
			{
				World[i].Name = s[i];
				World[i].Multiplier = Save_Data.GetValue<float>("World-"+s[i]+"-Multiplier");
				//World[i].Index = Save_Data.GetValue<int>("World-"+s[i]+"-Index");
				//World[i].Col = Save_Data.GetValue<Color>("World-"+s[i]+"-Col");
				if(World[i] is Stat)
				{
					(World[i] as Stat).SetLevel(Save_Data.GetValue<int>("World-"+s[i]+"-Level"));
				}
				else World[i].Max = Save_Data.GetValue<int>("World-"+s[i]+"-Max");

				World[i].Set(Save_Data.GetValue<int>("World-"+s[i]+"-Current"));
			}

		}
		RepLast = Save_Data.TryGetValue<int>("Rep Last");
		
		
		//Loading Grands
		System.Guid [] prevhex;
		
		if(Save_Data.TryGetValue<System.Guid[]>("Prev Grands", out prevhex))
		{
			for(int i = 0; i < World.Population; i++)
			{
				if(i >= prevhex.Length || prevhex[i] == null) continue;
				GrandData g =  new GrandData(prevhex[i]);

				string pref = "Grand:" + prevhex[i].ToString();

				g.Info.Name = Save_Data.TryGetValue<string>(pref+"-Name");
				g.Info.Gender = Save_Data.TryGetValue<bool>(pref+"-Gender");
				g.Info.PupilScale = Save_Data.TryGetValue<Vector3>(pref+"-Pupils");
				g.Info.Nation = Save_Data.TryGetValue<NationStatus>(pref+"-Nation");

				g.Info.C_Skin = Save_Data.TryGetValue<Color>(pref+"-C_Skin");
				g.Info.C_Hair = Save_Data.TryGetValue<Color>(pref+"-C_Hair");
				g.Info.C_Eye = Save_Data.TryGetValue<Color>(pref+"-C_Eye");
				g.Info.C_Offset = Save_Data.TryGetValue<Color>(pref+"-C_Offset");
				g.Info.C_Nose = Save_Data.TryGetValue<Color>(pref+"-C_Nose");

				for(int a = 0; a < f_inf.Length; a++)
				{
					g.SetFaceInfo(f_inf[a], new FaceInfo(
						Save_Data.TryGetValue<int>(pref+"-"+f_inf[a]+":Index"),
						Save_Data.TryGetValue<Vector3[]>(pref+"-"+f_inf[a]+":Values"),
						Save_Data.TryGetValue<ColorType>(pref+"-"+f_inf[a]+":Colour")));
				}

				for(int r = 0; r < g.Resources.Length; r++)
				{
					g.Resources[r] = new Resource(
						Save_Data.TryGetValue<int>(pref + "-Res " + r + ":Index"),
						Save_Data.TryGetValue<int>(pref + "-Res " + r + ":Current"),
						Save_Data.TryGetValue<int>(pref + "-Res " + r + ":Max")
						);
					g.Resources[r].Set(Save_Data.TryGetValue<int>(pref + "-Res " + r + ":Current"));

					System.TimeSpan span = System.TimeSpan.FromSeconds(Save_Data.GetValue<double>(pref + "-Res " + r + ":SpanSecs"));
					g.Resources[r].SetRate(Save_Data.TryGetValue<float>(pref+"-Res " + r + ":Rate"), span);

					g.Resources[r].TimeLast = Save_Data.TryGetValue<System.DateTime>(pref+"-Res " + r + ":TimeLast");
				}
				Grands.Add(g);
			}
		}
		print("Loaded info: " + Grands.Count + " grands");
	}
}


[System.Serializable]
public class WorldResources
{
	public string VillageName;
	public Stat Rep;
	public Resource Funds, Meds;
	public Resource [] AllRes
	{
		get{return new Resource[]{Rep, Funds, Meds};}
	}

	public int Length{get{return AllRes.Length;}}
	public int Population{
		get{return 3 + Rep.Level * 2;}
	}
	public WorldResources()
	{

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

	public string [] Names
	{
		get{
			string [] fin = new string[Length];
			for(int i = 0; i < Length; i++)
			{
				fin[i] = AllRes[i].Name;
			}
			return fin;
		}
	}
}

[System.Serializable]
public class GrandData
{
	public System.Guid Hex;
	public Role RoleType;
	public GreatGrand GrandObj;
	public FaceObj Face;
	public List<Face> Faces = new List<Face>();
	public GrandInfo Info;

	public Resource Smiles, Grumps;
	public Resource Fitness, Social;
	public Resource Hunger;
	public Resource Age;

	public GrandMod [] Mods;

	public Resource [] Resources
	{
		get{return new Resource[] {Age, Smiles, Grumps, Fitness, Social, Hunger};}
	}


	public GrandAlert [] CheckTime(System.DateTime t)
	{
		List<GrandAlert> fin = new List<GrandAlert>();
	
		int A = Age.Check(t);
		if(A > 0)	fin.Add(new GrandAlert(AlertType.Ageup, this, new int [1] {A}));

		int F = Fitness.Check(t);
		int soc = Social.Check(t);
		int H = Hunger.Check(t);

		if(Hunger.Ratio < 0.2F) fin.Add(new GrandAlert(AlertType.Hungry,this));
		if(Fitness.Ratio < 0.2F) fin.Add(new GrandAlert(AlertType.Fitness,this));

		float agerate = 1.0F + (float)Age.Value/200.0F;
		float baserate = 0.05F * agerate;
		float multrate = 0.05F * agerate;

		float from_hunger = multrate * Hunger.Ratio;
		float from_fitness = multrate * Fitness.Ratio;
		float from_social = multrate * Social.Ratio;

		Smiles.Rate = baserate + (from_hunger + from_fitness + from_social);
		Grumps.Rate = baserate + (multrate - from_hunger) + (multrate - from_fitness) + (multrate - from_social);

		int S = Smiles.Check(t);
		int G = Grumps.Check(t);
		return fin.ToArray();
	}

	public GrandData(System.Guid h){
		Hex = h;
		Info = new GrandInfo();
		Age = new Resource(0, 60, -1);

		Smiles = new Resource(1,10,-1);
		Smiles.SetRate(0, new System.TimeSpan(0,0,1));
		Grumps = new Resource(2,10,-1);
		Grumps.SetRate(0, new System.TimeSpan(0,0,1));

		Hunger = new Resource(3, 0, 100);
		Fitness = new Resource(4, 0, 100);
		Social = new Resource(5, 0, 100);
	}

	public void SetTimeLast(System.DateTime n)
	{
		for(int i = 0; i < Resources.Length; i++) Resources[i].TimeLast = n;
	}
	public FaceInfo GetFaceInfo(string s)
	{
		switch(s)
		{
			case "Eye": return Info.Eye;
			case "Ear": return Info.Ear;
			case "Brow": return Info.Brow;
			case "Base": return Info.Base;
			case "Hair": return Info.Hair;
			case "Jaw": return Info.Jaw;
			case "Nose": return Info.Nose;
		}
		return null;
	}

	public void SetFaceInfo(string s, FaceInfo n)
	{
		switch(s)
		{
			case "Eye": Info.Eye = n; break;
			case "Ear": Info.Ear = n; break;
			case "Brow": Info.Brow = n; break;
			case "Base": Info.Base = n; break;
			case "Hair": Info.Hair = n; break;
			case "Jaw": Info.Jaw = n; break;
			case "Nose": Info.Nose = n; break;
		}
	}


	public static string [] Names_Male = new string []
		{
			"Ralph",
			"Wally",
			"Ed",
			"Thomas",
			"Max",
			"Luton"
		};

		public static string Names_Male_Random
		{
			get{return Names_Male[Random.Range(0, Names_Male.Length)];}
		}

		public static string [] Names_Female = new string [] 
		{
			"Lucille",
			"Sandy",
			"Meryl",
			"Barb",
			"Louise"
		};

		public static string Names_Female_Random
		{
			get{return Names_Female[Random.Range(0, Names_Female.Length)];}
		}
}
public struct GrandAlert
{
	public GrandData Grand;
	public int [] Values;
	public AlertType Type;
	public GrandAlert(AlertType t, GrandData g, params int [] v)
	{
		Type = t;
		Grand = g;
		Values = v;
	}
}
public enum AlertType{	Ageup, Smiles, Grumps, 
						Hungry, Fitness, Social, 
						Senile, Fight, Gift, 
						Repup, None}

[System.Serializable]
public class GrandInfo
{
	public string Name;
	public bool Gender;
	public NationStatus Nation;
	public MaritalStatus MStat;
	public bool Military;

	public FaceInfo Eye, Ear, Brow, Base, Hair, Jaw, Nose;
	public Vector3 PupilScale;
	public Color C_Skin, C_Hair, C_Offset, C_Nose, C_Eye;
	public GrandInfo()
	{
		Name = "";
		Gender = false;
		Nation = NationStatus.Australian;
		Military = false;
	}
}

public class FaceInfo
{
	public int Index;
	public GameObject Obj;

	public Vector3 _Position = Vector3.zero;
	public Vector3 _Rotation = Vector3.zero;
	public Vector3 _Scale = Vector3.one;
	public ColorType Colour = ColorType.Skin;

	public bool Symm;
	public float Symm_Distance;
	public float Symm_ScaleDiff;

	public Vector3 [] Values{get{return new Vector3[]{_Position, _Rotation, _Scale};}}

	public FaceInfo(int i, ColorType c)
	{
		Index = i;
		Colour = c;
	}

	public FaceInfo(int i, Vector3 [] v, ColorType c)
	{
		Index = i;
		_Position = v[0];
		_Rotation = v[1];
		_Scale = v[2];
		Colour = c;
	}

	public FaceInfo(FaceInfo old)
	{
		Index = old.Index;
		_Position = old._Position;
		_Rotation = old._Rotation;
		_Scale = old._Scale;
		Colour = old.Colour;
	}


	public void Randomise(Vector3 pos, float rot, Vector3 sc)
	{

		_Position = Utility.RandomVectorInclusive(pos.x, pos.y);
		_Rotation = Utility.RandomVectorInclusive(0.0F, 0.0F, rot);
		_Scale = Vector3.one + Utility.RandomVectorInclusive(sc.x, sc.y);
	}

	public FaceInfo Clone()
	{
		FaceInfo fin = new FaceInfo(this);
		fin._Position = _Position;
		fin._Rotation = _Rotation;
		fin._Scale = _Scale;
		return fin;
	}
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

	public float Ratio
	{
		get{
			if(Max == -1) return 1.0F;
			return Current_soft / (float)Max;
		}
	}

	public string RatioToString()
	{
		return (Ratio * 100).ToString("0");
	}

	public virtual string ToString()
	{
		return Value+"";
	}

	public float Multiplier;
	public int Current;

	public Color Col;
	private int _index;
	public int Index
	{
		get{return _index;}
		set{_index = value;}
	}
	public string Name;
	public int Max = -1;

	public Resource(int ind, int curr = 0, int max = -1, float mult = 1.0F)
	{
		Index = ind;

		Max = max;

		Set(curr);

		Multiplier = mult;
		Col = Color.white;
	}

	public virtual void Set(int i)
	{
		int m = (Max == -1) ? 99999: Max;
		Current = Mathf.Clamp(i, 0, m);
		Current_soft = (float) Current;
	}

	public virtual void Set (float i)
	{
		int m = (Max == -1) ? 99999: Max;
		Current_soft = Mathf.Clamp(i, 0.0F, m);
		Current = (int) Mathf.Round(Current_soft);
	}

	public float Rate = 0.0F;
	public System.DateTime TimeLast;
	public System.TimeSpan Span;
	public virtual void SetRate(float r, System.TimeSpan t)
	{
		Rate = r;
		Span = t;
	}

	public virtual int Check(System.DateTime t)
	{
		if((System.DateTime.Compare(t, TimeLast.Add(Span)) > 0))
		{
			int diff = (int) Mathf.Round(Current_soft + Rate) - Current;
			Set(Current_soft + Rate);
			
			TimeLast = t;
			return diff;
		}
		return 0;
	}

	public virtual bool Charge(int n)
	{
		if(Current > n)
		{
			Add(-n);

			return true;
		}
		else return false;
	}

	protected float Current_soft;
	public virtual void Add(float n)
	{
		float m = (Max == -1) ? 99999 : Max;
		Set(Current_soft + n);
		GameManager.UI.CheckResourcesUI();
	}

	public virtual void AddMax(float n)
	{
		if(Max == -1) return;
		float r = Ratio;
		Max += (int)n;
		Set((float)Max * Ratio);
	}
}

[System.Serializable]
public class Stat:Resource
{
	public override string ToString()
	{
		return Value + "/" + Max;
	}

	public int Level;
	[SerializeField]
	private float Max_Mult_Per_Lvl;
	public override void Add(float n)
	{
		Current_soft += n;
		Current = (int) Mathf.Round(Current_soft);
		while(Current > Max) 
		{
			Level++;
			Max = Max + (int) ((float)Max * Max_Mult_Per_Lvl);
		}
	}

	public Stat(int ind):base(ind)
	{
		Current = 0;
		Current_soft = 0.0F;
		Level = 1;
		Max_Mult_Per_Lvl = 1.1F;
		Max = 100;
	}

	public void SetLevel(int l)
	{
		int diff = l - Level;
		for(int i = 0; i < diff; i++)
		{
			Max = Max + (int) ((float)Max * Max_Mult_Per_Lvl);
		}
		Level = l;
	}
}


public class RewardCon
{
	public string Title = "";
	public string Description = "";
	public int Rep = 0, Funds = 0, Meds = 0;
	public RewardCon(string t, string d, int [] re)
	{
		Title = t;
		Description = d;
		Rep = re[0];
		Funds = re[1];
		Meds = re[2];
	}
}

public class GrandMod
{
	private System.Action<GrandData, float[]> _action;
	private float [] Values;
	public GrandMod(System.Action<GrandData, float[]> a, params float [] f)
	{
		_action = a;
		Values = f;
	}
	public void Act(GrandData d)
	{
		_action(d, Values);
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
	}

	public void Update()
	{

	}

	public void Trace(float spawn, float fade = 1.3F)
	{

	}


	public void Destroy()
	{
		VectorLine.Destroy(ref Line);
		VectorLine.Destroy(ref Arrow);
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

public enum Role
{
	Resident, Visitor, Orderly, Villain
}
public enum ResourceType
{
	Rep, Funds, Meds, Smiles, Grumps, Fit, Slob
}


