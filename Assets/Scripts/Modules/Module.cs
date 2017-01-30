using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Module : MonoBehaviour {
	public int Index = 0;
	public bool Running = false;

	public UIObj MUI;
	public ObjectContainer MObj;

	public GreatGrand [] Grands;

	public bool Intro;
	public virtual UIQuote [] Intro_String
	{
		get{return new UIQuote[0];}
	}
	private bool Intro_Shown;
	public void SetIntro(bool f)
	{
		Intro_Shown = f;
		PlayerPrefs.SetInt("Intro-" + Index, 0);
	}

	public virtual void ControlledUpdate()
	{

	}

	public IEnumerator CheckForIntro()
	{
		if(Intro && !Intro_Shown && Intro_String.Length > 0)
		{
			Intro_Shown = true;
			PlayerPrefs.SetInt("Intro-"+Index, 1);
			yield return StartCoroutine(GameManager.UI.QuoteRoutine(Intro_String[0]));
		}
	}

//Used to preload resources and set up the module
	public void Init()
	{
		Intro_Shown = PlayerPrefs.GetInt("Intro-" + Index) == 1;
		InitObj();
		InitUI();
	}

	public virtual void InitObj()
	{

	}

	public virtual void InitUI()
	{

	}

//Used to load resources
	public virtual IEnumerator Load()
	{
		yield return new WaitForSeconds(0.3F);
	}

//Used to begin the module running
	public virtual IEnumerator Enter(bool entry, IntVector v)
	{
		this.gameObject.SetActive(true);
		yield return StartCoroutine(Load());
		
		MUI.SetActive(true);
		if(entry)
		{
			Sequence f = OpeningSequence(v);
			yield return f.WaitForCompletion();
		}

		yield return StartCoroutine(CheckForIntro());
	}

	public virtual Sequence OpeningSequence(IntVector v)
	{
		UIObj mui = MUI;
		Transform start = GameManager.UI.ModuleRight;
		Transform end = GameManager.UI.ModuleTarget;

		if(v.x == 1) start = GameManager.UI.ModuleRight;
		else if(v.x == -1) start = GameManager.UI.ModuleLeft;
		else start = GameManager.UI.ModuleRight;

		mui.transform.position = start.position;

		return Tweens.SwoopTo(mui.transform, end.position);
	}

	public virtual Sequence ClosingSequence(IntVector v)
	{
		UIObj mui = MUI;
		Transform end = GameManager.UI.ModuleRight;

		if(v.x == 1) end = GameManager.UI.ModuleRight;
		else if(v.x == -1) end = GameManager.UI.ModuleLeft;
		else end = GameManager.UI.ModuleRight;

		return Tweens.SwoopTo(mui.transform, end.position);
	}


//Used to reset scene assets and scripts
	public virtual void Clear()
	{

	}

//Generic call for completing game modules
	public virtual void Complete()
	{

	}


//used to exit the module and unload resources
	public virtual IEnumerator Exit(IntVector v)
	{
		Sequence f = ClosingSequence(v);
		yield return f.WaitForCompletion();
		
		Running = false;
		Clear();
		GameManager.UI.WorldObjects.DestroyChildren();
		this.gameObject.SetActive(false);
		MUI.SetActive(false);
	}

	public List<_Grump> GetRelatedGrumps(GreatGrand g)
	{
		List<_Grump> fin = new List<_Grump>();
		for(int i = 0; i < Grands.Length; i++)
		{
			if(Grands[i] == g) continue;
			for(int x = 0; x < Grands[i].Grumps.Length; x++)
			{
				if(Grands[i].Grumps[x].Target == g) fin.Add(Grands[i].Grumps[x]);
			}
		}
		return fin;
	}

}
