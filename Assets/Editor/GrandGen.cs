using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GrandGen : EditorWindow {


	[MenuItem("Custom/Grand Generator")]
	static void Init()
	{
		GrandGen window = (GrandGen)EditorWindow.GetWindow(typeof(GrandGen));
		window.Show();
	}

	static FaceObjInfoContainer Base;

	static FaceObjInfoContainer Eye,
								Nose,
								Brow,
								Hair,
								Ear,
								Jaw;

	int tab = 0;

	bool Eye_Pupils = false;
	int Eye_Current  = 0,
		Nose_Current  = 0,
		Brow_Current  = 0,
		Hair_Current  = 0,
		Ear_Current  = 0,
		Jaw_Current  = 0,
		Base_Current = 0;

	public FaceObjInfo 	Base_Final,
						Eye_Final,
						Nose_Final,
						Hair_Final,
						Brow_Final,
						Ear_Final,
						Jaw_Final;

	public Color Skin, HairCol, Offset;
	public int Skin_num, Hair_num, Offset_num;
	public GameData Data;

	public FaceObjInfoContainer FOA(int num)
	{
		switch(num)
		{
			//GRAND INFO
			case 0:
			return Base;
			break;
			//Base
			case 1:
			return Hair;
			break;
			//Jaw
			case 2:
			return Jaw;
			break;
			//Eye
			case 3:
			return Eye;
			break;
			//Nose
			case 4:
			return Nose;
			break;
			//Brow
			case 5:
			return Brow;
			break;
			//Ear
			case 6:
			return Ear;
			break;
		}
		return Base;
	}

	void OnGUI()
	{
		if(FaceParent == null) FaceParent = GameObject.Find("FaceParent").transform;
		if(Data == null) Data = GameObject.Find("GameManager").GetComponent<GameData>();
		
		GUILayout.Label("Grand Generator", EditorStyles.boldLabel);
		if(GUILayout.Button("Get Elements"))
		{
			LoadElements();
		}
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.green;
		if(GUILayout.Button("Generate"))
		{
			Generate();
		}
		GUI.color = Color.white;

		GUI.color = Color.yellow;
		if(GUILayout.Button("Randomise"))
		{
			Randomise();
		}
		GUI.color = Color.white;

		GUI.color = Color.yellow;
		if(GUILayout.Button("Delete"))
		{
			if(TargetGrand != null) DestroyImmediate(TargetGrand.gameObject);
		}
		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if(!elements_loaded) return;
		tab = GUILayout.Toolbar(tab, new string[] {"Base", "Hair","Jaw","Eyes", 
													"Nose", "Brows", "Ears"});
		FaceObjInfoContainer targ = FOA(tab);


		if(targ == null) return; 

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Elements: " + targ.Length, EditorStyles.boldLabel);
		targ.Colour = (ColorType) EditorGUILayout.EnumPopup(targ.Colour);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("<",  GUILayout.Width(50))) targ.Index --;
		GUILayout.Label(targ.Current.Name +"", EditorStyles.boldLabel);
		if(GUILayout.Button(">",  GUILayout.Width(50))) targ.Index++;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Scale", GUILayout.Width(50));
		targ._Scale.x = EditorGUILayout.Slider(targ._Scale.x, 0.1F, 2.5F, GUILayout.Width(150));
		targ._Scale.y = EditorGUILayout.Slider(targ._Scale.y, 0.1F, 2.5F, GUILayout.Width(150));
		EditorGUILayout.EndHorizontal();

		if(targ == Base)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Skin", GUILayout.Width(50));
			Skin = EditorGUILayout.ColorField(Skin, GUILayout.Width(150));
			if(GUILayout.Button("<",  GUILayout.Width(20))) Skin = Data.CycleSkin(Skin_num++);
			if(GUILayout.Button(">",  GUILayout.Width(20))) Skin = Data.CycleSkin(Skin_num--);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Hair", GUILayout.Width(50));
			HairCol = EditorGUILayout.ColorField(HairCol, GUILayout.Width(150));
			if(GUILayout.Button("<",  GUILayout.Width(20))) HairCol = Data.CycleHair(Hair_num++);
			if(GUILayout.Button(">",  GUILayout.Width(20))) HairCol = Data.CycleHair(Hair_num--);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Col B", GUILayout.Width(50));
			Offset = EditorGUILayout.ColorField(Offset, GUILayout.Width(150));
			EditorGUILayout.EndHorizontal();
		}
		else
		{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Position", GUILayout.Width(50));
				targ._Position.x = EditorGUILayout.Slider(targ._Position.x, -30.0F, 30.0F, GUILayout.Width(150));
				targ._Position.y = EditorGUILayout.Slider(targ._Position.y, -30.0F, 30.0F, GUILayout.Width(150));
				EditorGUILayout.EndHorizontal();

			//	targ._Position = EditorGUILayout.Vector3Field("Position", targ._Position);
				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Rotation", GUILayout.Width(50));
				targ._Rotation.z = EditorGUILayout.Slider("", targ._Rotation.z, -25.0F, 25.0F, GUILayout.Width(150));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();
				/*if(targ == Eye)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Pupil", GUILayout.Width(50));
					targ._Rotation.z = EditorGUILayout.Slider("", targ._Rotation.z, -25.0F, 25.0F, GUILayout.Width(150));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}*/

				if(targ.Symm)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Distance", GUILayout.Width(50));
					targ.Symm_Distance = EditorGUILayout.Slider("", targ.Symm_Distance, -3.0F, 3.0F, GUILayout.Width(150));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Scale Diff", GUILayout.Width(50));
					targ.Symm_ScaleDiff = EditorGUILayout.Slider("", targ.Symm_ScaleDiff, -0.5F, 0.5F, GUILayout.Width(150));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}

				if(targ == Eye && TargetGrand)
				{
					if(GUILayout.Button("Pupil",  GUILayout.Width(80))) 
					{
						TargetGrand[0][0].Img[1].enabled = !TargetGrand[0][0].Img[1].enabled;
						TargetGrand[1][0].Img[1].enabled = !TargetGrand[1][0].Img[1].enabled;
					}
				}
		}
		CheckDifferences();
		
		
	}
	public Transform FaceParent;
	public FaceObj TargetGrand;
	public void Generate()
	{
		if(TargetGrand != null) DestroyImmediate(TargetGrand.gameObject);

		GameObject _base = (GameObject) Instantiate(Base.Current.Prefab);
		_base.transform.SetParent(FaceParent.transform);

		TargetGrand = _base.GetComponent<FaceObj>();

		TargetGrand.Reset(Base);
		TargetGrand.Start();
		(TargetGrand[0] as FaceObj).SetupObj(TargetGrand, Eye);
		(TargetGrand[1] as FaceObj).SetupObj(TargetGrand, Eye);
		(TargetGrand[2][0] as FaceObj).Setup(TargetGrand, Ear);
		(TargetGrand[3][0] as FaceObj).Setup(TargetGrand, Ear);
		(TargetGrand[4][0] as FaceObj).Setup(TargetGrand, Brow);
		(TargetGrand[5][0] as FaceObj).Setup(TargetGrand, Brow);
		(TargetGrand[6][0] as FaceObj).Setup(TargetGrand, Hair);
		(TargetGrand[7][0] as FaceObj).Setup(TargetGrand, Jaw);
		(TargetGrand[8][0] as FaceObj).Setup(TargetGrand, Nose);

		//FaceObj Shadow = TargetGrand[9] as FaceObj;
		//Shadow.Img[0].sprite = Ear.Current._Sprite;
		//Shadow.Img[1].sprite = Ear.Current._Sprite;
		//Shadow.Img[4].sprite = Hair.Current._Sprite;
		//Shadow.Img[5].sprite = Jaw.Current._Sprite;

		_base.name = "GRAND";
		_base.transform.localScale = Vector3.one;
	}

	void CheckDifferences()
	{
		if(TargetGrand == null) return;

		TargetGrand.SetSkinColor(Skin);
		TargetGrand.SetHairColor(HairCol);
		TargetGrand.SetOffsetColor(Offset);

		TargetGrand.Reset(Base);
		(TargetGrand[0] as FaceObj).GetObjInfo(Eye);
		(TargetGrand[1] as FaceObj).GetObjInfo(Eye, false);
		//(TargetGrand[0][0] as FaceObj).Reset(Eye);
		//(TargetGrand[1][0] as FaceObj).Reset(Eye);
		(TargetGrand[2][0] as FaceObj).Reset(Ear);
		(TargetGrand[3][0] as FaceObj).Reset(Ear);
		(TargetGrand[4][0] as FaceObj).Reset(Brow);
		(TargetGrand[5][0] as FaceObj).Reset(Brow);
		(TargetGrand[6][0] as FaceObj).Reset(Hair);
		(TargetGrand[7][0] as FaceObj).Reset(Jaw);
		(TargetGrand[8][0] as FaceObj).Reset(Nose);
	}

	public void Randomise()
	{
		Eye.Randomise();
		Brow.Randomise();
		Ear.Randomise();
		Jaw.Randomise(0.0F, 0.0F);
		Hair.Randomise();
		Nose.Randomise();
		Base.Randomise(0.0F, 0.0F, 0.15F);

		Skin = Data.RandomSkin();
		HairCol = Data.RandomHair();
		Offset = Random.ColorHSV();
		Offset = Color.Lerp(Offset, Color.white, 0.3F);

		Generate();
		CheckDifferences();
	}

	void OnEnable()
	{
		elements_loaded = false;
		LoadElements();
		FaceParent = GameObject.Find("FaceParent").transform;
	}

	bool elements_loaded = false;
	void LoadElements()
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


		elements_loaded = true;
	}
}
