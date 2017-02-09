using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Menu : Module {

	public override UIQuote [] Intro_String
	{
		get{
			return new UIQuote[]
			{
				new UIQuote("Carer", "Welcome to " + GameManager.WorldRes.VillageName + " retirement village!",
									 "We pride ourselves on giving the best care and experience for Grands.",
									 "Why don't you take a look around?",
									 "It's just about dinner time, head to the dining hall to meet the residents!")
			};
		}
	}

	FaceObj [] Faces;
	UIObj [] Frames;

	public override void InitUI()
	{
		MUI[1].ClearActions();
		MUI[1].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Dinner"));
		});
		MUI[2].ClearActions();
		MUI[2].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Bowls"));
		});
		menuobj = MUI["back"];
	}

	public RectTransform ScrollTrack;
	public UIObj menuobj;
	public override void ControlledUpdate()
	{
		Vector2 v = GameManager._Input.GetScroll();
		v.x = 0.0F;

		Vector2 newpos = menuobj.GetUIPosition() + v;
		newpos.y = Mathf.Clamp(newpos.y, ScrollTrack.transform.position.y, Screen.height/2);
		menuobj.SetUIPosition(newpos);
	}

	public override IEnumerator Load()
	{
		Faces = new FaceObj[GameManager.WorldRes.Population];
		Frames = new UIObj[GameManager.WorldRes.Population];

		List<GrandData> allgrands = new List<GrandData>();
		allgrands.AddRange(GameManager.instance.Grands);

		for(int i = 0; i < Faces.Length; i++)
		{
			if(allgrands.Count <= 0) continue;
			int num = Random.Range(0, allgrands.Count);
			GreatGrand f = allgrands[num].GrandObj;
			if(f == null) continue;
			Frames[i] = (UIObj) Instantiate(GameManager.Data.RandomFrame());
			Frames[i].SetParent(MUI[0]);
			Frames[i].transform.position = MUI[0].Img[i].transform.position;
			Frames[i].Svg[1].color = f.Info.C_Eye;
			Frames[i].Svg[0].color = GameManager.Generator.RandomSkin();

			Faces[i] = GameManager.Generator.GenerateFace(f);
			Frames[i][0].AddChild(Faces[i]);

			Faces[i].transform.position = Frames[i][0].transform.position;
			SetupFace(Faces[i], f);

			allgrands.RemoveAt(num);

			yield return null;
		}

		if(GameManager.instance.Alerts.Count > 0)
		{
			UIObj alert = GameManager.UI.PermUI["exit"];
			alert.SetActive(false);
			alert.TweenActive(true);
			alert[0].TweenActive(true);
			alert[0].Txt[0].text = "" + GameManager.instance.Alerts.Count;

			alert.ClearActions();
			alert.AddAction(UIAction.MouseUp, () =>
			{
				StartCoroutine(GameManager.instance.ShowAlerts());
				GameManager.UI.PermUI["exit"].TweenActive(false);
				GameManager.UI.PermUI["exit"][0].SetActive(false);
			});
		}
		else GameManager.UI.PermUI["exit"].SetActive(false);
	}

	private UIObj ginfo;
	private GreatGrand ginfo_target;

	public void SetupFace(FaceObj f, GreatGrand g)
	{
		f.AddAction(UIAction.MouseDown, ()=>
		{
			if(ginfo_target != g)
			{
				if(ginfo != null) ginfo.PoolDestroy();
				ginfo_target = g;
				ginfo = GameManager.UI.GrandInfo(g);
				ginfo.SetParent(MUI["back"]);
				ginfo.FitUIPosition(f.transform.position + Vector3.down*50, null, 1.0F);
				ginfo.SetActive(false);
				ginfo.TweenActive(true);
			}
			else if(ginfo != null) 
			{
				ginfo.PoolDestroy();
				ginfo_target = null;
			}
			
		});
	}

	public override Sequence OpeningSequence(IntVector v)
	{
		UIObj mui = MUI;
		Transform start = GameManager.UI.ModuleRight;
		Transform end = GameManager.UI.ModuleTarget;

		if(v.x == 1) start = GameManager.UI.ModuleRight;
		else if(v.x == -1) start = GameManager.UI.ModuleLeft;
		else start = GameManager.UI.ModuleRight;

		mui.transform.position = start.position;

		Sequence s = Tweens.SwoopTo(mui.transform, end.position);

		for(int i = 0; i < Frames.Length; i++)
		{
			s.Insert(0.4F, Tweens.PictureSway(Frames[i].transform, new Vector3(0,0,12 * v.x)));
		}

		return s;
	}

	public override void Clear()
	{
		for(int i = 0; i < Frames.Length; i++)
		{
			Frames[i].Destroy();
		}
	}

	
}
