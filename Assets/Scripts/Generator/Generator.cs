using UnityEngine;
using System.Collections;

public class Generator : MonoBehaviour {
	public GreatGrand GGObj;

	public FaceObj Face_Parent;
	public FaceObj [] Face_Bases;
	public FaceObj [] Face_Hair_Male;
	public FaceObj [] Face_Hair_Female;
	public FaceObj [] Face_Eyes_Male, Face_Eyes_Female;
	public FaceObj [] Face_Brow_Male, Face_Brow_Female;
	public FaceObj [] Face_Ears_Male, Face_Ears_Female;

	public Color [] SkinTones;
	public Color [] HairTones;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GreatGrand Generate(int num)
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Generate(num);

		FaceObj _base = (FaceObj) Instantiate(Face_Parent);
		_base.SetParent(GameManager.UI.FaceParent);
		_base.transform.localScale = Vector3.one;
		(_base.transform as RectTransform).sizeDelta = Vector3.one;
		_base.transform.position = Vector3.zero;

		_base.FaceParent = _base;
		_base.SetSkinColor(SkinTones[Random.Range(0, SkinTones.Length)]);
		_base.SetHairColor(HairTones[Random.Range(0, HairTones.Length)]);


		CreateFaceHalf(_base);
		
		FaceObj _eyes = final.Info.Gender ? Face_Eyes_Male[Random.Range(0, Face_Eyes_Male.Length)] :
														Face_Eyes_Female[Random.Range(0, Face_Eyes_Female.Length)];
		(_base.Child[0] as FaceObj).CreateAnchor(0, _eyes);

		FaceObj _brow = final.Info.Gender ? Face_Brow_Male[Random.Range(0, Face_Brow_Male.Length)] :
														Face_Brow_Female[Random.Range(0, Face_Brow_Female.Length)];
		(_base.Child[0] as FaceObj).CreateAnchor(1, _brow);

		FaceObj _ear = final.Info.Gender ? Face_Ears_Male[Random.Range(0, Face_Ears_Male.Length)] :
											Face_Ears_Female[Random.Range(0, Face_Ears_Female.Length)];

		(_base.Child[0] as FaceObj).CreateAnchor(2, _ear);

		CreateFaceHalf(_base, (FaceObj)_base.Child[0]);

		_base.AddAnimTrigger("Blink", new Vector2(4.0F, 7.0F), _base[0][0][0].GetComponent<Animator>(), _base[1][0][0].GetComponent<Animator>());
		_base.AddAnimTrigger("Raise", new Vector2(6.0F, 8.0F), _base[0][1][0].GetComponent<Animator>(), _base[1][1][0].GetComponent<Animator>());
		

		_base.SetActive(false);
		final.SetFace(_base);

		return final;
	}

	public FaceObj CreateFaceHalf(FaceObj parent, FaceObj prev = null)
	{
		FaceObj final = null;

		if(prev) final = (FaceObj) Instantiate(prev);
		else final = (FaceObj) Instantiate(Face_Bases[Random.Range(0, Face_Bases.Length)]);

		final.SetParent(parent);

		RectTransform r = final.transform as RectTransform;

		r.localScale = prev ? Vector3.one : new Vector3(-1,1,1);
		
		r.position = Vector3.zero;
		r.anchorMax = Vector2.one;//_anchorpoint.anchorMax;
		r.anchorMin = Vector2.zero;//_anchorpoint.anchorMin;
		r.anchoredPosition = Vector2.zero;//_anchorpoint.anchoredPosition;
		r.sizeDelta = Vector3.one;

		final.SetActive(true);

		return final;
	}
}
	[System.Serializable]
	public class GreatGrand_Data
	{
		public bool Gender;
		public int Age;
		public string Name;
		public bool Military;

		public MaritalStatus MStat;
		public NationStatus Nationality;

		public float GFactor = 0.75F;

		//Visuals
		public int FaceType, HairType, EyeType;

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

		public static Color [] Colours = new Color[]
		{
			Color.red,
			Color.green,
			Color.blue,
			Color.yellow,
			Color.white,
			Color.black,
			Color.grey
		};

		public static Color Colours_Random
		{
			get{return Colours[Random.Range(0,Colours.Length)];}
		}
	}
