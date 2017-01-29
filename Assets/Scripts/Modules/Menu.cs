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
									 "We pride ourselves on giving the best care and experience for your Grand.",
									 "I assume you've brought your Grand along?",
									 "Great! We'll get started on setting up your room!",
									 "Why don't you and your Grand take a look around?",
									 "It's just about dinner time, head to the dining hall to meet some other residents!")
			};
		}
	}

	FaceObj [] faces;
	public override void InitUI()
	{
		MUI[0].ClearActions();
		MUI[0].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Dinner"));
		});
	}

	public override IEnumerator Load()
	{
		faces = new FaceObj[MUI[1].Child.Length];
		List<GrandData> allgrands = new List<GrandData>();
		allgrands.AddRange(GameManager.instance.Grands);

		for(int i = 0; i < MUI[1].Child.Length; i++)
		{
			int num = Random.Range(0, allgrands.Count);
			GreatGrand f = allgrands[num].GrandObj;
			faces[i] = GameManager.instance.Generator.GenerateFace(f);

			MUI[1].Child[i][0].AddChild(faces[i]);

			faces[i].transform.position = MUI[1].Child[i][0].transform.position;
			faces[i].transform.localScale = Vector3.one * 0.65F;
			f.gameObject.SetActive(false);

			allgrands.RemoveAt(num);

			yield return null;
		}
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

		for(int i = 0; i < MUI[1].Child.Length; i++)
		{
			s.Insert(0.4F, Tweens.PictureSway(MUI[1].Child[i].transform, new Vector3(0,0,12 * v.x)));
		}

		return s;
	}

	public override void Clear()
	{
		for(int i = 0; i < MUI[1].Child.Length; i++)
		{
			MUI[1].Child[i][0].DestroyChildren();
		}
	}

	
}
