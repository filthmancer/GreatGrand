﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Generator : MonoBehaviour {
	public GreatGrand GGObj;

	public FaceObj Face_Parent;
	public TextMeshProUGUI TitleObj;

	public Color [] SkinTones;
	public Color [] HairTones;

	public Color Skin, HairCol, Offset;
	public int Skin_num, Hair_num, Offset_num;
	public Color CycleSkin(int i)
	{
		if(SkinTones.Length == 0) return Color.white;
		int actual = Mathf.Abs(i) % SkinTones.Length;
		return SkinTones[actual];
	}
	public Color RandomSkin(){return SkinTones[Random.Range(0, SkinTones.Length)];}
	public Color CycleHair(int i)
	{
		if(HairTones.Length == 0) return Color.white;
		int actual = Mathf.Abs(i) % HairTones.Length;
		return HairTones[actual];
	}
	public Color RandomHair(){return HairTones[Random.Range(0, HairTones.Length)];}
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

		FaceObj _base = RandomiseFace(final);
		final.SetFace(_base);
		TitleObj.text = final.Info.Name + "\nAge " + final.Info.Age;
		return final;
	}


	public GreatGrand Randomise()
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Generate(0);

		FaceObj _base = RandomiseFace(final);
		final.SetFace(_base);
		TitleObj.text = final.Info.Name + "\nAge " + final.Info.Age;
		return final;
	}

	public FaceObjInfoContainer Base;

	public FaceObjInfoContainer Eye,
								Nose,
								Brow,
								Hair,
								Ear,
								Jaw;
		
	public Transform FaceParent;
	public GreatGrand TargetGrand;
	public FaceObj GenerateFace(GreatGrand targ)
	{
		Destroy();

		GameObject _base = (GameObject) Instantiate(Base.Current.Prefab);
		_base.transform.SetParent(FaceParent.transform);

		FaceObj final = _base.GetComponent<FaceObj>();

		final.SetSkinColor(Skin);
		final.SetHairColor(HairCol);
		final.SetOffsetColor(Offset);

		final.Reset(Base);
		final.Start();
		(final[0] as FaceObj).SetupObj(final, Eye);
		(final[1] as FaceObj).SetupObj(final, Eye);
		(final[2][0] as FaceObj).Setup(final, Ear);
		(final[3][0] as FaceObj).Setup(final, Ear);
		(final[4][0] as FaceObj).Setup(final, Brow);
		(final[5][0] as FaceObj).Setup(final, Brow);
		(final[6][0] as FaceObj).Setup(final, Hair);
		(final[7][0] as FaceObj).Setup(final, Jaw);
		(final[8][0] as FaceObj).Setup(final, Nose);

		_base.name = targ.Info.Name;
		_base.transform.localScale = Vector3.one;
		return final;
	}

	public void CheckDifferences(FaceObj f)
	{
		if(f == null) return;

		f.SetSkinColor(Skin);
		f.SetHairColor(HairCol);
		f.SetOffsetColor(Offset);
		f[9].Img[0].sprite = Ear.Current._Sprite;
		f[9].Img[1].sprite = Ear.Current._Sprite;

		//f[9].Img[2].sprite = Base.Current._Sprite;
		//f[9].Img[3].sprite = Base.Current._Sprite;

		f[9].Img[4].sprite = Hair.Current._Sprite;
		f[9].Img[5].sprite = Jaw.Current._Sprite;

		f.Reset(Base);
		(f[0] as FaceObj).GetObjInfo(Eye);
		(f[1] as FaceObj).GetObjInfo(Eye, false);
		(f[2][0] as FaceObj).Reset(Ear);
		(f[3][0] as FaceObj).Reset(Ear);
		(f[4][0] as FaceObj).Reset(Brow);
		(f[5][0] as FaceObj).Reset(Brow);
		(f[6][0] as FaceObj).Reset(Hair);
		(f[7][0] as FaceObj).Reset(Jaw);
		(f[8][0] as FaceObj).Reset(Nose);

		
	}

	public FaceObj RandomiseFace(GreatGrand f)
	{
		Eye.Randomise();
		Brow.Randomise();
		Ear.Randomise();
		Jaw.Randomise(0.0F, 0.0F);
		Hair.Randomise();
		Nose.Randomise();
		Base.Randomise(0.0F, 0.0F, 0.15F);

		Skin = RandomSkin();
		HairCol = RandomHair();
		Offset = Random.ColorHSV();
		Offset = Color.Lerp(Offset, Color.white, 0.1F);

		FaceObj fin = GenerateFace(f);
		CheckDifferences(fin);
		return fin;
	}
	public void Destroy()
	{
		if(TargetGrand != null) 
		{
			DestroyImmediate(TargetGrand.gameObject);
			DestroyImmediate(TargetGrand.Face.gameObject);
		}
	}
	bool elements_loaded = false;
	public void LoadElements()
	{
		List<Object> _base = new List<Object>();
		List<FaceObjInfo> final = new List<FaceObjInfo>();
		_base.AddRange(Resources.LoadAll("Objects/Base", typeof(Object)));
		for(int i =0 ; i < _base.Count; i++)
		{
			GameObject g = _base[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				final.Add(f);
			}
		}
		Base = new FaceObjInfoContainer(final.ToArray());

		List<Object> _eye = new List<Object>();
		final = new List<FaceObjInfo>();
		_eye.AddRange(Resources.LoadAll("Objects/Eye", typeof(Object)));
		for(int i =0 ; i < _eye.Count; i++)
		{
			GameObject g = _eye[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				final.Add(f);
			}
		}
		Eye =new FaceObjInfoContainer(final.ToArray());
		Eye.Symm = true;
		Eye.Colour = ColorType.Offset;


		List<Object> _nose = new List<Object>();
		final = new List<FaceObjInfo>();
		_nose.AddRange(Resources.LoadAll("Objects/Nose", typeof(Object)));
		for(int i =0 ; i < _nose.Count; i++)
		{
			Sprite g = _nose[i] as Sprite;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				final.Add(f);
			}
		}
		Nose = new FaceObjInfoContainer(final.ToArray());
		Nose.Colour = ColorType.Offset;

		List<Object> _brow = new List<Object>();
		final = new List<FaceObjInfo>();
		_brow.AddRange(Resources.LoadAll("Objects/Brow", typeof(Object)));
		for(int i =0 ; i < _brow.Count; i++)
		{
			Sprite g = _brow[i] as Sprite;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				
				final.Add(f);
			}
		}
		Brow = new FaceObjInfoContainer(final.ToArray());
		Brow.Symm = true;
		Brow.Colour = ColorType.Hair;
		

		List<Object> _hair = new List<Object>();
		final = new List<FaceObjInfo>();
		_hair.AddRange(Resources.LoadAll("Objects/Hair", typeof(Object)));
		for(int i =0 ; i < _hair.Count; i++)
		{
			Sprite g = _hair[i] as Sprite;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				
				final.Add(f);
			}
		}
		Hair = new FaceObjInfoContainer(final.ToArray());
		Hair.Symm = true;
		Hair.Colour = ColorType.Hair;

		List<Object> _ear = new List<Object>();
		final = new List<FaceObjInfo>();
		_ear.AddRange(Resources.LoadAll("Objects/Ear", typeof(Object)));
		for(int i =0 ; i < _ear.Count; i++)
		{
			Sprite g = _ear[i] as Sprite;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				
				final.Add(f);
			}
		}
		Ear = new FaceObjInfoContainer(final.ToArray());
		Ear.Symm = true;
		Ear.Colour = ColorType.Offset;


		List<Object> _jaw = new List<Object>();
		final = new List<FaceObjInfo>();
		_jaw.AddRange(Resources.LoadAll("Objects/Jaw", typeof(Object)));
		for(int i =0 ; i < _jaw.Count; i++)
		{
			Sprite g = _jaw[i] as Sprite;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(i, g);
				final.Add(f);
			}
		}
		Jaw = new FaceObjInfoContainer(final.ToArray());
		Jaw.Colour = ColorType.Offset;

		Skin = SkinTones[0];
		HairCol = HairTones[0];
		Offset = Color.white;

		elements_loaded = true;
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
