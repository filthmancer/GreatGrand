using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GrandGenerator : EditorWindow {

	[MenuItem("Custom/Grand Generator")]
	static void Init()
	{
		GrandGenerator window = (GrandGenerator)EditorWindow.GetWindow(typeof(GrandGenerator));
		window.Show();
	}

	int tab = 0;

	public GameData Data;
	public Generator Gen;

	public FaceObjInfoContainer FOA(int num)
	{
		switch(num)
		{
			//GRAND INFO
			case 0:
			return Gen.Base;
			break;
			//Base
			case 1:
			return Gen.Hair;
			break;
			//Jaw
			case 2:
			return Gen.Jaw;
			break;
			//Eye
			case 3:
			return Gen.Eye;
			break;
			//Nose
			case 4:
			return Gen.Nose;
			break;
			//Brow
			case 5:
			return Gen.Brow;
			break;
			//Ear
			case 6:
			return Gen.Ear;
			break;
		}
		return Gen.Base;
	}

	float progress = 0.0F;
	float time_per_face = 0.3F;
	float time_init = 0.0F;
	float time_total = 0.0F;

	void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI()
	{
		
		if(Data == null) Data = GameObject.Find("GameManager").GetComponent<GameData>();
		if(Gen == null)
		{ 
			Gen = GameObject.Find("Generator").GetComponent<Generator>();
			if(Gen) Gen.LoadElements();
		}
		
		GUILayout.Label("Generator", EditorStyles.boldLabel);
		if(GUILayout.Button("Get Elements"))
		{
			Gen.LoadElements();
		}
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.green;



		if(GUILayout.Button("Generate"))
		{
			/*int facenum = 30;
			time_total = facenum * time_per_face;
			time_init = (float) EditorApplication.timeSinceStartup;	*/	
			Gen.Destroy();
			Gen.TargetGrand = Gen.Generate(0);
			FaceObj f = Gen.GenerateFace(Gen.TargetGrand);
			f.transform.localRotation = Quaternion.identity;
			f.transform.SetParent(GameManager.GetCanvas());
			f.transform.localScale = Vector3.one;
			f.transform.localRotation = Quaternion.identity;
		}

		if(progress < time_total)
		{
			EditorUtility.DisplayProgressBar("Simple", "Show", progress/time_total);
			Gen.Destroy();
			Gen.TargetGrand = Gen.Generate(0);
			FaceObj f = Gen.GenerateFace(Gen.TargetGrand);
			f.transform.localRotation = Quaternion.identity;
			f.transform.localScale = new Vector3(1.0F, 0.6F, 1.0F);
		}
		else EditorUtility.ClearProgressBar();
		progress = (float) (EditorApplication.timeSinceStartup - time_init);


		GUI.color = Color.white;

		GUI.color = Color.yellow;
		if(GUILayout.Button("Randomise"))
		{
			Gen.Destroy();
			Gen.TargetGrand = Gen.Randomise();
		}
		GUI.color = Color.white;

		GUI.color = Color.yellow;
		if(GUILayout.Button("Delete"))
		{
			Gen.Destroy();
		}
		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

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

		if(targ == Gen.Base)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Skin", GUILayout.Width(50));
			Gen.SkinCurrent = EditorGUILayout.ColorField(Gen.SkinCurrent, GUILayout.Width(150));
			if(GUILayout.Button("<",  GUILayout.Width(20))) Gen.SkinCurrent = Gen.CycleSkin(1);
			if(GUILayout.Button(">",  GUILayout.Width(20))) Gen.SkinCurrent = Gen.CycleSkin(-1);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Hair", GUILayout.Width(50));
			Gen.HairCurrent = EditorGUILayout.ColorField(Gen.HairCurrent, GUILayout.Width(150));
			if(GUILayout.Button("<",  GUILayout.Width(20))) Gen.HairCurrent = Gen.CycleHair(1);
			if(GUILayout.Button(">",  GUILayout.Width(20))) Gen.HairCurrent = Gen.CycleHair(-1);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Col B", GUILayout.Width(50));
			Gen.OffsetCurrent = EditorGUILayout.ColorField(Gen.OffsetCurrent, GUILayout.Width(150));
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

				if(targ == Gen.Eye && Gen.TargetGrand)
				{
					if(GUILayout.Button("Pupil",  GUILayout.Width(80))) 
					{
						Gen.TargetGrand.Face[0][0].Img[1].enabled = !Gen.TargetGrand.Face[0][0].Img[1].enabled;
						Gen.TargetGrand.Face[1][0].Img[1].enabled = !Gen.TargetGrand.Face[1][0].Img[1].enabled;
					}
				}
		}
		//Gen.CheckDifferences(Gen.TargetGrand.Face);
		
		
	}



	void OnEnable()
	{
		if(Gen != null) Gen.LoadElements();
	}

}
