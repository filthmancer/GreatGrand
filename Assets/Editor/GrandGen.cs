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
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Grand Generator", EditorStyles.boldLabel);
		if(GUILayout.Button("Get Elements"))
		{
			LoadElements();
		}
		if(GUILayout.Button("Generate"))
		{
			Generate();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if(!elements_loaded) return;
		tab = GUILayout.Toolbar(tab, new string[] {"Base", "Hair","Jaw","Eye", 
													"Nose", "Brow", "Ear"});
		FaceObjInfoContainer targ = FOA(tab);

		if(targ == null) return; 
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Elements: " + targ.Length, EditorStyles.boldLabel);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("<")) targ.Index --;
		GUILayout.Label(targ.Current.Name +"", EditorStyles.boldLabel);
		if(GUILayout.Button(">")) targ.Index++;
		EditorGUILayout.Space();
		targ.Current._Scale = EditorGUILayout.Vector3Field("Scale", targ.Current._Scale);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		targ.Current._Position = EditorGUILayout.Vector3Field("Position", targ.Current._Position);
		EditorGUILayout.Space();
		targ.Current._Rotation = EditorGUILayout.Vector3Field("Rotation", targ.Current._Rotation);
		EditorGUILayout.EndHorizontal();

		CheckDifferences();
		
	}
	public Transform FaceParent;
	public FaceObj TargetGrand;

	public void Generate()
	{
		if(TargetGrand != null) DestroyImmediate(TargetGrand.gameObject);
		GameObject _base = Instantiate(Base.Current.Prefab);
		_base.transform.SetParent(FaceParent.transform);

		TargetGrand = _base.GetComponent<FaceObj>();

		TargetGrand.CreateAnchor(0, Eye.Current);
		TargetGrand.CreateAnchor(1, Brow.Current);
		TargetGrand.CreateAnchor(2, Ear.Current);
		TargetGrand.CreateAnchor(3, Jaw.Current);
		TargetGrand.CreateAnchor(4, Hair.Current);

		_base.name = "GRAND";
	}

	void CheckDifferences()
	{
		if(TargetGrand == null) return;

		TargetGrand.GetInfo(0, Eye.Current);
		TargetGrand.GetInfo(1, Brow.Current);
		TargetGrand.GetInfo(2, Ear.Current);
		TargetGrand.GetInfo(3, Jaw.Current);
		TargetGrand.GetInfo(4, Hair.Current);

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
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Base = new FaceObjInfoContainer(final.ToArray());

		List<Object> _eye = new List<Object>();
		final = new List<FaceObjInfo>();
		_eye.AddRange(Resources.LoadAll("Objects/Eye", typeof(Object)));
		for(int i =0 ; i < _eye.Count; i++)
		{
			GameObject g = _eye[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Eye =new FaceObjInfoContainer(final.ToArray());

		List<Object> _nose = new List<Object>();
		final = new List<FaceObjInfo>();
		_nose.AddRange(Resources.LoadAll("Objects/Nose", typeof(Object)));
		for(int i =0 ; i < _nose.Count; i++)
		{
			GameObject g = _nose[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Nose = new FaceObjInfoContainer(final.ToArray());

		List<Object> _brow = new List<Object>();
		final = new List<FaceObjInfo>();
		_brow.AddRange(Resources.LoadAll("Objects/Brow", typeof(Object)));
		for(int i =0 ; i < _brow.Count; i++)
		{
			GameObject g = _brow[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Brow = new FaceObjInfoContainer(final.ToArray());

		List<Object> _hair = new List<Object>();
		final = new List<FaceObjInfo>();
		_hair.AddRange(Resources.LoadAll("Objects/Hair", typeof(Object)));
		for(int i =0 ; i < _hair.Count; i++)
		{
			GameObject g = _hair[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Hair = new FaceObjInfoContainer(final.ToArray());

		List<Object> _ear = new List<Object>();
		final = new List<FaceObjInfo>();
		_ear.AddRange(Resources.LoadAll("Objects/Ear", typeof(Object)));
		for(int i =0 ; i < _ear.Count; i++)
		{
			GameObject g = _ear[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Ear = new FaceObjInfoContainer(final.ToArray());

		List<Object> _jaw = new List<Object>();
		final = new List<FaceObjInfo>();
		_jaw.AddRange(Resources.LoadAll("Objects/Jaw", typeof(Object)));
		for(int i =0 ; i < _jaw.Count; i++)
		{
			GameObject g = _jaw[i] as GameObject;
			if((g != null) && g.transform.root ==g.transform)
			{
				final.Add(new FaceObjInfo(i, g));
			}
		}
		Jaw = new FaceObjInfoContainer(final.ToArray());

		elements_loaded = true;
	}


}
