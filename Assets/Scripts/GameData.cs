
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;

public class GameData : MonoBehaviour {

	public WorldResources World;
	public List<GrandData> Grands;

	public SaveData Save_Data;
	private string Save_Location; 
    private string Save_File = "SaveData"; 
    private string Save_Target
    {
    	get{return Save_Location + "\\" + Save_File +".uml";}
    }
 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init()
	{
		World.Init();
		Save_Location=Application.persistentDataPath;
		Load();
	}

	private string [] f_inf = new string[]
	{
		"Base", "Eye", "Ear", "Brow", "Hair", "Jaw", "Nose"
	};

	public void Save()
	{
		Save_Data = new SaveData(Save_File); 

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
			Save_Data[pref+"-Age"] = Data.Info.Age;
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
		}

		Save_Data["Prev Grands"] = prevgrands;
		Save_Data.Save(Save_Target);

		print("Saved at " + Save_Target);
		print("Saved info: " + prevgrands.Length + " grands");
	}

	public void Load()
	{
		print("Loading from " + Save_Target);
		if(!System.IO.File.Exists(Save_Target)) return;
		Save_Data =  SaveData.Load(Save_Target);
		//if(Save_Data == null) return;
	//Loading World
		string [] s;
		if(Save_Data.TryGetValue<string[]>("World-Resources", out s))
		{
			//World = new WorldResources();
			for(int i = 0; i < s.Length; i++)
			{
				World[i].Name = s[i];
				World[i].Set(Save_Data.GetValue<int>("World-"+s[i]+"-Current"));
				World[i].Multiplier = Save_Data.GetValue<float>("World-"+s[i]+"-Multiplier");
				World[i].Index = Save_Data.GetValue<int>("World-"+s[i]+"-Index");
				
				//World[i].Col = Save_Data.GetValue<Color>("World-"+s[i]+"-Col");
				if(World[i] is Stat)
				{
					(World[i] as Stat).SetLevel(Save_Data.GetValue<int>("World-"+s[i]+"-Level"));
				}
				else World[i].Max = Save_Data.GetValue<int>("World-"+s[i]+"-Max");
			}
		}
		

	//Loading Grands
		System.Guid [] prevhex;
		Grands = new List<GrandData>();

		if(Save_Data.TryGetValue<System.Guid[]>("Prev Grands", out prevhex))
		{
			for(int i = 0; i < prevhex.Length; i++)
			{
				GrandData g =  new GrandData(prevhex[i]);

				string pref = "Grand:" + prevhex[i].ToString();

				g.Info.Name = Save_Data.GetValue<string>(pref+"-Name");
				g.Info.Gender = Save_Data.GetValue<bool>(pref+"-Gender");
				g.Info.Age = Save_Data.GetValue<int>(pref+"-Age");
				g.Info.PupilScale = Save_Data.GetValue<Vector3>(pref+"-Pupils");
				g.Info.Nation = Save_Data.GetValue<NationStatus>(pref+"-Nation");

				g.Info.C_Skin = Save_Data.GetValue<Color>(pref+"-C_Skin");
				g.Info.C_Hair = Save_Data.GetValue<Color>(pref+"-C_Hair");
				g.Info.C_Eye = Save_Data.GetValue<Color>(pref+"-C_Eye");
				g.Info.C_Offset = Save_Data.GetValue<Color>(pref+"-C_Offset");
				g.Info.C_Nose = Save_Data.GetValue<Color>(pref+"-C_Nose");

				for(int a = 0; a < f_inf.Length; a++)
				{
					g.SetFaceInfo(f_inf[a], new FaceInfo(
						Save_Data.GetValue<int>(pref+"-"+f_inf[a]+":Index"),
						Save_Data.GetValue<Vector3[]>(pref+"-"+f_inf[a]+":Values"),
						Save_Data.GetValue<ColorType>(pref+"-"+f_inf[a]+":Colour")));
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
		Rep = new Stat(0);
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
	public GrandInfo Info;

	public Resource Smiles, Grumps;
	public float Fitness, Social;
	public float Hunger;

	public void AgeUp()
	{
		Info.Age++;
	}

	public GrandData(System.Guid h){
		Hex = h;
		Info = new GrandInfo();
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

[System.Serializable]
public class GrandInfo
{
	public string Name;
	public bool Gender;
	public int Age;
	public NationStatus Nation;
	public MaritalStatus MStat;
	public bool Military;

	public FaceInfo Eye, Ear, Brow, Base, Hair, Jaw, Nose;
	public Vector3 PupilScale;
	public Color C_Skin, C_Hair, C_Offset, C_Nose, C_Eye;
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

	public virtual string ToString()
	{
		return Value+"";
	}

	public float Multiplier;
	public int Current;

	public Color Col;

	[HideInInspector]
	public int Index;
	public string Name;
	public int Max = 99999;

	public Resource(int ind)
	{
		Current = 0;
		Current_soft = 0.0F;
		Multiplier = 1.0F;
		Col = Color.white;
		Index = ind;
	}

	public virtual void Set(int i)
	{
		Current = i;
		Current_soft = (float)i;
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
		Current_soft += n;
		Current = (int) Mathf.Round(Current_soft);
		GameManager.UI.CheckResourcesUI();
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
			Current -= Max;
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

public enum Role
{
	Resident, Visitor, Orderly, Villain
}
public enum ResourceType
{
	Rep, Funds, Meds, Smiles, Grumps, Fit, Slob
}

