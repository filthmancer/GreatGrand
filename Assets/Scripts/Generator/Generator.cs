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

	public GrandData GenerateGrand()
	{
		GrandData fin = new GrandData(System.Guid.NewGuid());
		fin.RoleType = Role.Resident;
		
		RandomiseGrandData(fin);
		RandomiseFaceData(fin);
		return fin;
	}


	public GrandData EditorGenerate()
	{
		GrandData fin = new GrandData(System.Guid.NewGuid());
		fin.RoleType = Role.Resident;

		RandomiseGrandData(fin);
		
		CheckEditorInfo(fin);
		RandomiseOffsetColours(fin);
		SetPupilInfo(fin);
		return fin;
	}

	public void CheckEditorInfo(GrandData fin)
	{
		fin.Info.Base = Base.GetCurrent();
		fin.Info.Eye = Eye.GetCurrent();
		fin.Info.Ear = Ear.GetCurrent();
		fin.Info.Brow = Brow.GetCurrent();
		fin.Info.Hair = Hair.GetCurrent();
		fin.Info.Jaw = Jaw.GetCurrent();
		fin.Info.Nose = Nose.GetCurrent();

		fin.Info.C_Hair = HairCurrent;		
		fin.Info.C_Skin = SkinCurrent;
		fin.Info.C_Eye = EyeCurrent;
	}


	public GreatGrand Generate(int num)
	{
		GreatGrand final = (GreatGrand) Instantiate(GGObj);
		final.Generate(num);

		final.Data.Hex = System.Guid.NewGuid();
		final.Data.RoleType = Role.Visitor;
		final.Data.GrandObj = final;
		RandomiseGrandData(final.Data);
		RandomiseFaceData(final.Data);
		final.gameObject.name = final.Data.Info.Name + "-" + final.Data.RoleType;
		return final;
	}

	public void RandomiseGrandData(GrandData final)
	{
		final.Info.Gender = Random.value > 0.5F;
		final.Info.Name = final.Info.Gender ? GrandData.Names_Male_Random : GrandData.Names_Female_Random;
		final.SetTimeLast(System.DateTime.Now);
		final.Social.Set((int)Random.Range(30, 70));
		final.Social.SetRate(-4.0F + Random.Range(-2.0F, 4.0F), new System.TimeSpan(0,1,0));

		final.Fitness.Set((int)Random.Range(30, 70));
		final.Fitness.SetRate(-2.0F + Random.Range(-3.0F, -1.0F), new System.TimeSpan(0,1,0));

		final.Hunger.Set(100);
		final.Hunger.SetRate(-2.0F + Random.Range(-3.0F, -1.0F), new System.TimeSpan(0,1,0));

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
	}

	public void RandomiseFaceData(GrandData final)
	{
		final.Info.C_Hair = RandomHair();		
		final.Info.C_Skin = RandomSkin();
		final.Info.C_Eye = RandomEye();

		RandomiseOffsetColours(final);

		final.Info.Base = Base.Randomise(final.Info.Gender, Simple2x2.zero, Simple2x2.zero, Simple2x2.zero);

		final.Info.Eye = Eye.Randomise(final.Info.Gender, new Simple2x2(0.0F, -0.05F, 0.0F, 0.15F),
									 Simple2x2.zero,
									 new Simple2x2(-0.2F, -0.2F, 0.1F, 0.1F));
		//	new Vector3(0,0.05F), 0.0F, new Vector3(0.1F, 0.1F));

		final.Info.Ear = Ear.Randomise(final.Info.Gender, new Simple2x2(0.0F, -0.3F, 0.0F, 0.3F),
									 new Simple2x2(-4.0F, 0.0F, 4.0F, 0.0F),
									 new Simple2x2(-0.3F, -0.3F, 0.25F, 0.25F));
			//new Vector3(0,0.3F), 4.0F,  new Vector3(0.3F, 0.3F));

		final.Info.Brow = Brow.Randomise(final.Info.Gender, new Simple2x2(0.0F, -0.1F, 0.0F, 0.15F),
									  new Simple2x2(-5.0F, 0.0F, 7.0F, 0.0F),
									 new Simple2x2(-0.2F, -0.15F, 0.35F, 0.35F));
			//new Vector3(0,0.1F), 6.0F,  new Vector3(0.3F, 0.2F));

		final.Info.Hair = Hair.Randomise(final.Info.Gender, Simple2x2.zero,
									 Simple2x2.zero,
									 new Simple2x2(0.0F, -0.1F, 0.0F, 0.1F));
			//Vector3.zero, 0.0F,  new Vector3(0.0F, 0.1F));

		final.Info.Jaw = Jaw.Randomise(final.Info.Gender, Simple2x2.zero,
									 Simple2x2.zero,
									 new Simple2x2(-0.15F, -0.2F, 0.15F, 0.24F));
			//Vector3.zero, 0.0F, new Vector3(0.35F, 0.25F));

		final.Info.Nose = Nose.Randomise(final.Info.Gender, new Simple2x2(0.0F, -0.1F, 0.0F, 0.2F),
									 Simple2x2.zero,
									 new Simple2x2(-0.2F, -0.15F, 0.1F, 0.2F));
			//new Vector3(0,0.1F), 0.0F,  new Vector3(0.1F, 0.3F));	
		SetPupilInfo(final);
	}
	public void RandomiseOffsetColours(GrandData final)
	{
		Color skin = final.Info.C_Skin;

		HSBColor offtemp = new HSBColor(skin);
		//offtemp.h += (Random.value - Random.value)/50;
		//offtemp.s += (Random.value)/10;
		offtemp.b += Random.Range(-0.05F, 0.02F);
		final.Info.C_Offset = offtemp.ToColor();

		HSBColor nosetemp = new HSBColor(skin);
		nosetemp.s *= 1.6F;
		final.Info.C_Nose = nosetemp.ToColor();
	}

	public void SetPupilInfo(GrandData final)
	{
		Vector3 eyescale = (final.Info.Eye._Scale - Vector3.one);
		Vector3 r = eyescale * Random.Range(0.7F, 1.3F);
		final.Info.PupilScale = Vector3.one*0.3F + Utility.RandomVectorInclusive(r.x + 0.1F, r.y + 0.1F, 0.0F);
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

	public FaceContainer Base;

	public FaceContainer Eye,
								Nose,
								Brow,
								Hair,
								Ear,
								Jaw;

	public GreatGrand TargetGrand;
	public GrandData TargetData;
	public FaceObj GenerateFace(GreatGrand targ)
	{
		GameObject _base = (GameObject) Instantiate(targ.Data.Info.Base.Obj);

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

	public Face GenerateNewFace(GrandData targ)
	{
		GameObject _base = (GameObject) Instantiate(Base.GetObject(targ.Info.Gender, targ.Info.Base.Index));

		Face final = _base.GetComponent<Face>();

		final.FaceChildren.Left_Eye = Instantiate(Eye.GetObject(targ.Info.Gender, targ.Info.Eye.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Right_Eye = Instantiate(Eye.GetObject(targ.Info.Gender, targ.Info.Eye.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Left_Ear = Instantiate(Ear.GetObject(targ.Info.Gender, targ.Info.Ear.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Right_Ear = Instantiate(Ear.GetObject(targ.Info.Gender, targ.Info.Ear.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Left_Brow = Instantiate(Brow.GetObject(targ.Info.Gender, targ.Info.Brow.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Right_Brow = Instantiate(Brow.GetObject(targ.Info.Gender, targ.Info.Brow.Index)).GetComponent<Face_Obj>();

		final.FaceChildren.Hair = Instantiate(Hair.GetObject(targ.Info.Gender, targ.Info.Hair.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Nose = Instantiate(Nose.GetObject(targ.Info.Gender, targ.Info.Nose.Index)).GetComponent<Face_Obj>();
		final.FaceChildren.Jaw = Instantiate(Jaw.GetObject(targ.Info.Gender, targ.Info.Jaw.Index)).GetComponent<Face_Obj>();

		final.Create(targ.Info);

		_base.transform.SetParent(GameManager.GetWorldObjects().transform);
		_base.transform.localPosition = Vector3.zero;

		_base.name = targ.Info.Name;
		targ.Faces.Add(final);
		return final;
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
		//CheckDifferences(fin);
		return fin;
	}
	public void Destroy()
	{
		if(TargetGrand != null) 
		{
			DestroyImmediate(TargetGrand.gameObject);

			for(int i = 0; i < TargetGrand.Data.Faces.Count; i++)
					DestroyImmediate(TargetGrand.Data.Faces[i].gameObject);
		}
		if(TargetData != null) 
		{
			if(TargetData.GrandObj != null) DestroyImmediate(TargetData.GrandObj);

			for(int i = 0; i < TargetData.Faces.Count; i++)
					{
						if(TargetData.Faces[i] != null) 
							DestroyImmediate(TargetData.Faces[i].gameObject);
					}
		}
	}
	bool elements_loaded = false;

	string root = "Faces";
	public void LoadElements()
	{
		int x = 0;
		List<Object> _base = new List<Object>();
		List<GameObject> male = new List<GameObject>();
		List<GameObject> female = new List<GameObject>();

		_base.AddRange(Resources.LoadAll(root + "/base/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _base.Count; i++)
		{
			GameObject g = _base[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				male.Add(g);
				x++;
			}
		}
		_base.Clear();
		_base.AddRange(Resources.LoadAll(root + "/base/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _base.Count; i++)
		{
			GameObject g = _base[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				female.Add(g);
				x++;
			}
		}

		Base = new FaceContainer(male.ToArray(), female.ToArray());

		List<Object> _eye = new List<Object>();
		male.Clear();
		female.Clear();
		_eye.AddRange(Resources.LoadAll(root + "/eyes/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _eye.Count; i++)
		{
			GameObject g = _eye[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				male.Add(g);
				x++;
			}
		}
		_eye.Clear();
		_eye.AddRange(Resources.LoadAll(root + "/eyes/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _eye.Count; i++)
		{
			GameObject g = _eye[i] as GameObject;
			if((g != null) && g.transform.root == g.transform)
			{
				
				female.Add(g);
				x++;
			}
		}
		Eye =new FaceContainer(male.ToArray(), female.ToArray());
		Eye.Colour = ColorType.Offset;


		List<Object> _nose = new List<Object>();
		male = new List<GameObject>();
		female = new List<GameObject>();
		_nose.AddRange(Resources.LoadAll(root + "/nose/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _nose.Count; i++)
		{
			GameObject g = _nose[i] as GameObject;
			if((g != null))
			{
				
				male.Add(g);
					x++;
			}
		}
		_nose.Clear();
		_nose.AddRange(Resources.LoadAll(root + "/nose/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _nose.Count; i++)
		{
			GameObject g = _nose[i] as GameObject;
			if((g != null))
			{
				
				female.Add(g);
					x++;
			}
		}
		
		Nose = new FaceContainer(male.ToArray(), female.ToArray());
		Nose.Colour = ColorType.Nose;

		List<Object> _brow = new List<Object>();
		male = new List<GameObject>();
		female = new List<GameObject>();
		_brow.AddRange(Resources.LoadAll(root + "/brow/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _brow.Count; i++)
		{
			GameObject g = _brow[i] as GameObject;
			if((g != null))
			{
				
				male.Add(g);
					x++;
			}
		}
		_brow.Clear();
		_brow.AddRange(Resources.LoadAll(root + "/brow/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _brow.Count; i++)
		{
			GameObject g = _brow[i] as GameObject;
			if((g != null))
			{
				
				female.Add(g);
					x++;
			}
		
		}
		Brow = new FaceContainer(male.ToArray(), female.ToArray());
		Brow.Colour = ColorType.Hair;
		

		List<Object> _hair = new List<Object>();
		male = new List<GameObject>();
		female = new List<GameObject>();
		_hair.AddRange(Resources.LoadAll(root + "/hair/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _hair.Count; i++)
		{
			GameObject g = _hair[i] as GameObject;
			if((g != null))
			{
				
				male.Add(g);
					x++;
			}
		
		}
		_hair.Clear();
		_hair.AddRange(Resources.LoadAll(root + "/hair/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _hair.Count; i++)
		{
			GameObject g = _hair[i] as GameObject;
			if((g != null))
			{
				
				female.Add(g);
					x++;
			}
		
		}
		Hair = new FaceContainer(male.ToArray(), female.ToArray());
		Hair.Colour = ColorType.Hair;

		List<Object> _ear = new List<Object>();
		male = new List<GameObject>();
		female = new List<GameObject>();
		_ear.AddRange(Resources.LoadAll(root + "/ears/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _ear.Count; i++)
		{
			GameObject g = _ear[i] as GameObject;
			if((g != null))
			{
				
				male.Add(g);
					x++;
			}
		}
		_ear.Clear();
		_ear.AddRange(Resources.LoadAll(root + "/ears/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _ear.Count; i++)
		{
			GameObject g = _ear[i] as GameObject;
			if((g != null))
			{
				
				female.Add(g);
					x++;
			}
		}
		Ear = new FaceContainer(male.ToArray(), female.ToArray());
		Ear.Colour = ColorType.Skin;


		List<Object> _jaw = new List<Object>();
		male = new List<GameObject>();
		female = new List<GameObject>();
		_jaw.AddRange(Resources.LoadAll(root + "/jaw/Males", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _jaw.Count; i++)
		{
			GameObject g = _jaw[i] as GameObject;
			if((g != null))
			{
				
				male.Add(g);
					x++;
			}
		}
		_jaw.Clear();
		_jaw.AddRange(Resources.LoadAll(root + "/jaw/Females", typeof(Object)));
		x = 0;
		for(int i =0 ; i < _jaw.Count; i++)
		{
			GameObject g = _jaw[i] as GameObject;
			if((g != null))
			{
				
				female.Add(g);
					x++;
			}
		}
		Jaw = new FaceContainer(male.ToArray(), female.ToArray());
		Jaw.Colour = ColorType.Offset;

		SkinCurrent = SkinTones[0];
		HairCurrent = HairTones[0];
		OffsetCurrent = Color.white;

		elements_loaded = true;
	}
}

