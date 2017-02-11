using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Generator : MonoBehaviour {
	public GreatGrand GGObj;

	public GameObject Grand_Parent;
	public FaceObj Face_Parent;

	public TextMeshProUGUI TitleObj;

	public Color [] SkinTones;
	public Color [] HairTones;
	public Color [] EyeTones;

	public Color SkinCurrent, HairCurrent, OffsetCurrent, EyeCurrent;

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

	public Color CycleEye(int i)
	{
		if(EyeTones.Length == 0) return Color.white;
		int actual = Mathf.Abs(i) % EyeTones.Length;
		return EyeTones[actual];
	}
	public Color RandomEye(){return EyeTones[Random.Range(0, EyeTones.Length)];}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GrandData GenerateGrand()
	{
		GrandData fin = new GrandData(System.Guid.NewGuid());
		fin.RoleType = Role.Resident;
		
		GRandomise(fin);
		return fin;
	}

	public GreatGrand Generate(int num)
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Generate(num);

		final.Data.Hex = System.Guid.NewGuid();
		final.Data.RoleType = Role.Visitor;
		final.Data.GrandObj = final;
		GRandomise(final.Data);
		final.gameObject.name = final.Data.Info.Name + "-" + final.Data.RoleType;
		return final;
	}

	public void GRandomise(GrandData final)
	{
		final.Info.Gender = Random.value > 0.5F;
		final.Info.Name = final.Info.Gender ? GrandData.Names_Male_Random : GrandData.Names_Female_Random;
		
		final.Social.Set((int)Random.Range(30, 70));
		final.Social.SetRate(-4.0F + Random.Range(-2.5F, 4.5F), new System.TimeSpan(0,10,0));

		final.Fitness.Set((int)Random.Range(30, 70));
		final.Fitness.SetRate(-2.0F + Random.Range(-3.0F, -1.0F), new System.TimeSpan(0,10,0));

		final.Hunger.Set(100);
		final.Hunger.SetRate(-2.0F + Random.Range(-3.0F, -1.0F), new System.TimeSpan(0,10,0));

		final.Age.Set(Random.Range(60, 70));
		final.Age.SetRate(1,new System.TimeSpan(24,0,0));
		
		//Add grump based on age if male, remove if female
		//float agefactor = Mathf.Clamp((float)Info.Age/100.0F, 0.0F, 0.2F);
		//final.Info.GFactor += gender ? agefactor : -agefactor;
		final.Info.MStat = Random.value > 0.65F ? MaritalStatus.Married : (Random.value > 0.8F ? MaritalStatus.Divorced : MaritalStatus.Donor);
		//Add grump if divorced, remove if married
		switch(final.Info.MStat)
		{
			case MaritalStatus.Married:
			//Info.GFactor -= 0.15F;
			break;
			case MaritalStatus.Divorced:
			//Info.GFactor += 0.15F;
			break;
			case MaritalStatus.Donor:

			break;
		}
		final.Info.Military = Random.value > 0.95F;

		final.Info.C_Hair = RandomHair();		
		final.Info.C_Skin = RandomSkin();
		final.Info.C_Eye = RandomEye();

		Color skin = final.Info.C_Skin;

		HSBColor offtemp = new HSBColor(skin);
		//offtemp.h += (Random.value - Random.value)/50;
		//offtemp.s += (Random.value)/10;
		offtemp.b += Random.Range(-0.05F, 0.02F);
		final.Info.C_Offset = offtemp.ToColor();

		HSBColor nosetemp = new HSBColor(skin);
		nosetemp.s *= 1.6F;
		final.Info.C_Nose = nosetemp.ToColor();

		final.Info.Base = Base.Randomise(Simple2x2.zero, Simple2x2.zero, Simple2x2.zero);

		final.Info.Eye = Eye.Randomise(new Simple2x2(0.0F, -0.05F, 0.0F, 0.15F),
									 Simple2x2.zero,
									 new Simple2x2(-0.2F, -0.2F, 0.1F, 0.1F));
		//	new Vector3(0,0.05F), 0.0F, new Vector3(0.1F, 0.1F));

		final.Info.Ear = Ear.Randomise(new Simple2x2(0.0F, -0.3F, 0.0F, 0.3F),
									 new Simple2x2(-4.0F, 0.0F, 4.0F, 0.0F),
									 new Simple2x2(-0.3F, -0.3F, 0.25F, 0.25F));
			//new Vector3(0,0.3F), 4.0F,  new Vector3(0.3F, 0.3F));

		final.Info.Brow = Brow.Randomise(new Simple2x2(0.0F, -0.1F, 0.0F, 0.15F),
									  new Simple2x2(-5.0F, 0.0F, 7.0F, 0.0F),
									 new Simple2x2(-0.2F, -0.15F, 0.35F, 0.35F));
			//new Vector3(0,0.1F), 6.0F,  new Vector3(0.3F, 0.2F));

		final.Info.Hair = Hair.Randomise(Simple2x2.zero,
									 Simple2x2.zero,
									 new Simple2x2(0.0F, -0.1F, 0.0F, 0.1F));
			//Vector3.zero, 0.0F,  new Vector3(0.0F, 0.1F));

		final.Info.Jaw = Jaw.Randomise(Simple2x2.zero,
									 Simple2x2.zero,
									 new Simple2x2(-0.15F, -0.2F, 0.15F, 0.24F));
			//Vector3.zero, 0.0F, new Vector3(0.35F, 0.25F));

		final.Info.Nose = Nose.Randomise(new Simple2x2(0.0F, -0.1F, 0.0F, 0.2F),
									 Simple2x2.zero,
									 new Simple2x2(-0.2F, -0.15F, 0.1F, 0.2F));
			//new Vector3(0,0.1F), 0.0F,  new Vector3(0.1F, 0.3F));


		Vector3 eyescale = (final.Info.Eye._Scale - Vector3.one);
		Vector3 r = eyescale * Random.Range(0.7F, 1.3F);
		final.Info.PupilScale = Vector3.one*0.9F + Utility.RandomVectorInclusive(r.x + 0.1F, r.y + 0.1F, 0.0F);
		
	}

	public GreatGrand Generate(GrandData g)
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Data = g;
		final.Data.GrandObj = final;

		final.transform.SetParent(Grand_Parent.transform);
		final.gameObject.name = final.Data.Info.Name + "-" + final.Data.RoleType;
		return final;
	}


	public GreatGrand Randomise()
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Generate(0);

		FaceObj _base = RandomiseFace(final);
		final.SetFace(_base);
		TitleObj.text = final.Data.Info.Name + "\nAge " + final.Data.Age.Value;
		return final;
	}

	public FaceObjInfoContainer Base;

	public FaceObjInfoContainer Eye,
								Nose,
								Brow,
								Hair,
								Ear,
								Jaw;

	public GreatGrand TargetGrand;
	public FaceObj GenerateFace(GreatGrand targ)
	{
		GameObject _base = (GameObject) Instantiate(Base[targ.Data.Info.Base.Index].Prefab);

		FaceObj final = _base.GetComponent<FaceObj>();
		final.SetSkinColor(targ.Data.Info.C_Skin);
		final.SetHairColor(targ.Data.Info.C_Hair);
		final.SetOffsetColor(targ.Data.Info.C_Offset);
		final.SetNoseColor (targ.Data.Info.C_Nose);

		targ.SetFace(final);
		final._Name = targ.Data.Info.Name;
		_base.name = targ.Data.Info.Name;
		return final;
	}

	public void CheckDifferences(FaceObj f)
	{
		if(f == null) return;

		f.SetSkinColor(SkinCurrent);
		f.SetHairColor(HairCurrent);
		f.SetOffsetColor(OffsetCurrent);
		f[9].Img[0].sprite = Ear.Current._Sprite;
		f[9].Img[1].sprite = Ear.Current._Sprite;

		//f[9].Img[2].sprite = Base.Current._Sprite;
		//f[9].Img[3].sprite = Base.Current._Sprite;

		f[9].Img[4].sprite = Hair.Current._Sprite;
		f[9].Img[5].sprite = Jaw.Current._Sprite;

		/*f.Reset(Base);
		(f[0] as FaceObj).GetObjInfo(Eye);
		(f[1] as FaceObj).GetObjInfo(Eye, false);
		(f[2][0] as FaceObj).Reset(Ear);
		(f[3][0] as FaceObj).Reset(Ear);
		(f[4][0] as FaceObj).Reset(Brow);
		(f[5][0] as FaceObj).Reset(Brow);
		(f[6][0] as FaceObj).Reset(Hair);
		(f[7][0] as FaceObj).Reset(Jaw);
		(f[8][0] as FaceObj).Reset(Nose);*/
	}

	public FaceObj RandomiseFace(GreatGrand f)
	{
		/*Eye.Randomise(Vector3.zero);
		Brow.Randomise(Vector3.zero);
		Ear.Randomise(Vector3.zero);
		Jaw.Randomise(Vector3.zero, 0.0F);
		Hair.Randomise(Vector3.zero, 0.0F, 0.0F);
		Nose.Randomise(Vector3.zero);
		Base.Randomise(Vector3.zero, 0.0F, 0.15F);*/

		SkinCurrent = RandomSkin();
		HairCurrent = RandomHair();
		OffsetCurrent = Random.ColorHSV();

		FaceObj fin = GenerateFace(f);
		CheckDifferences(fin);
		return fin;
	}
	public void Destroy()
	{
		if(TargetGrand != null) 
		{
			DestroyImmediate(TargetGrand.gameObject);
			if(TargetGrand.Face != null) DestroyImmediate(TargetGrand.Face.gameObject);
		}
	}
	bool elements_loaded = false;
	public void LoadElements()
	{
		int x = 0;
		List<Object> _base = new List<Object>();
		List<FaceObjInfo> final = new List<FaceObjInfo>();
		_base.AddRange(Resources.LoadAll("SVG/base", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _base.Count; i++)
		{
			GameObject g = _base[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				final.Add(f);
				x++;
			}
		}
		Base = new FaceObjInfoContainer(final.ToArray());

		List<Object> _eye = new List<Object>();
		final = new List<FaceObjInfo>();
		_eye.AddRange(Resources.LoadAll("SVG/eyes", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _eye.Count; i++)
		{
			GameObject g = _eye[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				f.Colour = ColorType.Skin;
				final.Add(f);
				x++;
			}

		}
		Eye =new FaceObjInfoContainer(final.ToArray());
		Eye.Symm = true;
		Eye.Colour = ColorType.Offset;


		List<Object> _nose = new List<Object>();
		final = new List<FaceObjInfo>();
		_nose.AddRange(Resources.LoadAll("SVG/nose", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _nose.Count; i++)
		{
			GameObject g = _nose[i] as GameObject;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				final.Add(f);
					x++;
			}
		}
		
		Nose = new FaceObjInfoContainer(final.ToArray());
		Nose.Colour = ColorType.Nose;

		List<Object> _brow = new List<Object>();
		final = new List<FaceObjInfo>();
		_brow.AddRange(Resources.LoadAll("SVG/brow", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _brow.Count; i++)
		{
			GameObject g = _brow[i] as GameObject;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				f.Colour = ColorType.Hair;
				final.Add(f);
					x++;
			}
		
		}
		Brow = new FaceObjInfoContainer(final.ToArray());
		Brow.Symm = true;
		Brow.Colour = ColorType.Hair;
		

		List<Object> _hair = new List<Object>();
		final = new List<FaceObjInfo>();
		_hair.AddRange(Resources.LoadAll("SVG/hair", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _hair.Count; i++)
		{
			GameObject g = _hair[i] as GameObject;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				f.Colour = ColorType.Hair;
				final.Add(f);
					x++;
			}
		
		}
		Hair = new FaceObjInfoContainer(final.ToArray());
		Hair.Symm = true;
		Hair.Colour = ColorType.Hair;

		List<Object> _ear = new List<Object>();
		final = new List<FaceObjInfo>();
		_ear.AddRange(Resources.LoadAll("SVG/ears", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _ear.Count; i++)
		{
			GameObject g = _ear[i] as GameObject;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				f.Colour = ColorType.Offset;
				final.Add(f);
					x++;
			}
		
		}
		Ear = new FaceObjInfoContainer(final.ToArray());
		Ear.Symm = true;
		Ear.Colour = ColorType.Skin;


		List<Object> _jaw = new List<Object>();
		final = new List<FaceObjInfo>();
		_jaw.AddRange(Resources.LoadAll("SVG/jaw", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _jaw.Count; i++)
		{
			GameObject g = _jaw[i] as GameObject;
			if((g != null))
			{
				FaceObjInfo f = new FaceObjInfo(x, g);
				final.Add(f);
					x++;
			}
		
		}
		Jaw = new FaceObjInfoContainer(final.ToArray());
		Jaw.Colour = ColorType.Offset;

		SkinCurrent = SkinTones[0];
		HairCurrent = HairTones[0];
		OffsetCurrent = Color.white;

		elements_loaded = true;
	}
}

