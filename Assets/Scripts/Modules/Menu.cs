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

	FaceObj [] faces;
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
	}

	public override IEnumerator Load()
	{
		faces = new FaceObj[MUI[0].Child.Length];
		List<GrandData> allgrands = new List<GrandData>();
		allgrands.AddRange(GameManager.instance.Grands);

		for(int i = 0; i < MUI[0].Child.Length; i++)
		{
			int num = Random.Range(0, allgrands.Count);
			GreatGrand f = allgrands[num].GrandObj;
			faces[i] = GameManager.instance.Generator.GenerateFace(f);
			MUI[0].Child[i][0].AddChild(faces[i]);

			faces[i].transform.position = MUI[0].Child[i][0].transform.position;
			SetupFace(faces[i], f);

			allgrands.RemoveAt(num);

			yield return null;
		}

		GameManager.UI.PermUI["exit"].TweenActive(false);
	}

	public void SetupFace(FaceObj f, GreatGrand g)
	{
		f.AddAction(UIAction.MouseDown, ()=>
		{
			UIObj u = GameManager.UI.GrandInfo(g);
			u.FitUIPosition(f.transform.position + Vector3.down*50);
			u.SetActive(false);
			u.TweenActive(true);
			f.Alerts.Add(u);
		});
		f.AddAction(UIAction.MouseUp, ()=>
		{
			for(int i = 0; i < f.Alerts.Count; i++)
			{
				Destroy(f.Alerts[i].gameObject);
			}
			f.Alerts.Clear();
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

		for(int i = 0; i < MUI[0].Child.Length; i++)
		{
			s.Insert(0.4F, Tweens.PictureSway(MUI[0].Child[i].transform, new Vector3(0,0,12 * v.x)));
		}

		return s;
	}

	public override void Clear()
	{
		for(int i = 0; i < MUI[0].Child.Length; i++)
		{
			MUI[0].Child[i][0].DestroyChildren();
		}
	}

	
}
