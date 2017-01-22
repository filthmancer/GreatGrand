using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using DG.Tweening;

public class Dinner : Module {

	public TableManager _TableManager;
	public static int GG_num = 8;

	public VectorObject2D GrumpLine;
	private int [] GG_Indexes;

	private float Timer = 60.0F;
	private float Timer_current = 0.0F;

	public override UIQuote [] Intro_String
	{
		get{
			return new UIQuote[]
			{
				new UIQuote("Carer", "Many residents have strong feelings about each other...",
									 "They may get rowdy if seated close to enemies, or away from friends!")
			};
		}
	}

	private UIObj timerobj;
	private int last_sec = 0;
	public override void ControlledUpdate()
	{
		if(Running)
		{
			Timer_current += Time.deltaTime;
			if(Timer_current >= Timer)
			{
				Complete();
			}

			if(timerobj == null) timerobj = MUI["timer"];

			float timer_ui = (Timer - Timer_current);
			int mins = (int) timer_ui/60;
			int secs = (int) timer_ui % 60;

			timerobj.Txt[0].text = mins + ":" + secs;
			timerobj.Txt[0].color = (Timer_current > Timer*0.75F) ? Color.red : Color.white;

			if(secs != last_sec) 
			{
				last_sec = secs;
				if (Timer_current > Timer*0.75F) 
					Tweens.Bounce(timerobj.Img[0].transform, 
									Vector3.one + (Vector3.one * Timer_current / Timer/2));
			}

		}
	}

	public override void InitObj()
	{
		_TableManager.Init();
	}

	public override void InitUI()
	{
		MUI[1].AddAction(UIAction.MouseDown, () =>
		{
			if(GameManager.WorldRes.Meds.Charge(1))
			{
				for(int i = 0; i < Grands.Length; i++)
				{
					Grands[i].GrumpLines(3.0F, false);
				}
			}	
		});
		MUI[2].AddAction(UIAction.MouseDown, ()=>
		{
			Complete();
		});
		MUI[3].AddAction(UIAction.MouseDown, ()=>
		{
			StartCoroutine(GameManager.instance.LoadModule("Menu"));
		});
		MUI.SetActive(false);
	}

	public override IEnumerator Enter(bool entry, IntVector v)
	{
		this.gameObject.SetActive(true);
		yield return StartCoroutine(Load());
		
		MUI.SetActive(true);

		if(entry)
		{
			Sequence f = OpeningSequence(v);
			yield return f.WaitForCompletion();

			yield return new WaitForSeconds(0.3F);	
			MUI["endgame"][0].Txt[0].text = "DINNERTIME!";
			MUI["endgame"][0].TweenActive(true);
			yield return new WaitForSeconds(1.0F);
			MUI["endgame"][0].TweenActive(false);
		}
		CreateDinnerGame();
		yield return StartCoroutine(StartDinner(GG_Indexes));
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

		Sequence s = Tweens.SwoopTo(mui.transform, end.position);

		Sequence f = DOTween.Sequence();
		f.Append(MUI.Img[0].transform.DOLocalRotate(new Vector3(0, 25,0), 0.1F));
		f.Append(MUI.Img[0].transform.DOLocalRotate(new Vector3(0, 35,0), 0.2F));
		f.Append(MUI.Img[0].transform.DOLocalRotate(new Vector3(0, 0,0), 0.15F));

		s.Insert(0.35F, f);

		return s;
	}

	public override void Complete()
	{
		if(!Running) return;
		StartCoroutine(EndDinner());
	}

	public override void Clear()
	{
		for(int i = 0; i < Grands.Length; i++)
		{
			Grands[i].Destroy();
		}
		_TableManager.Clear();
		MUI["faceparent"].DestroyChildren();

		UIObj end = MUI["endgame"];
		for(int i = 0; i < end.Length; i++)
		{
			end[i].TweenActive(false);
		}
	}



	public void CreateDinnerGame()
	{
		Running = false;

		UIObj fparent = MUI["faceparent"];
		Grands = new GreatGrand[GG_num];
		for(int i = 0; i < GG_num; i++)
		{
			Grands[i] = GameManager.instance.Generator.Generate(i);
			FaceObj f = GameManager.instance.Generator.GenerateFace(Grands[i]);
			fparent.AddChild(f);

			Grands[i].transform.SetParent(this.transform);
		}

		for(int i = 0; i < GG_num; i+=2)
		{
			MaritalStatus m = Random.value > 0.55F ? MaritalStatus.Married : (Random.value < 0.9F ? MaritalStatus.Divorced : MaritalStatus.Donor);
			Grands[i].Info.MStat = m;
			Grands[i+1].Info.MStat = m;
			Grands[i].Relation = Grands[i+1];
			Grands[i+1].Relation = Grands[i];
		}

		for(int i = 0; i < GG_num; i++)
		{
			Grands[i].SitImmediate(_TableManager.Seat[i]);
		}

		for(int i = 0; i < GG_num; i++)
		{
			GenerateGrumpsReal(Grands[i], 1);
		}

		GG_Indexes = new int[0];
		while(NumberHappy > 3) GG_Indexes = ShuffleGG();

		for(int i = 0; i < GG_num; i++)
		{
			Grands[i].Face.SetActive(false);
		}
	}

	IEnumerator StartDinner(int [] index)
	{
		GameManager.UI.WinMenu.SetActive(false);
		for(int i = 0; i < Grands.Length; i++)
		{
			Grands[i].Face.SetActive(true);
			Grands[i].Face.transform.position = _TableManager.Door.position;
		}

		for(int i = Grands.Length-1; i >= 0; i--)
		{
			StartCoroutine(_TableManager.DoorToSeat(Grands[i], index[i], 0.7F));
		}

		while(!AllSeated) yield return null;
		Running = true;
		Timer_current = 0.0F;
		
		yield return null;
	}

	int FinalScore = 0;
	IEnumerator EndDinner()
	{
		while(!AllSeated) yield return null;

		Running = false;
		UIObj endgame = MUI["endgame"];

		endgame[0].TweenActive(true);
		endgame[1].SetActive(false);
		endgame[2].SetActive(false);

		endgame[0].Txt[0].text = "Let's Eat!";

		FinalScore = 0;
		UIObj total = endgame[1];

		yield return new WaitForSeconds(0.8F);

		endgame[0].TweenActive(false);

		List<UIAlert> correct = new List<UIAlert>();
		List<UIAlert> wrong = new List<UIAlert>();

		for(int i = 0; i < _TableManager.Seat.Length; i++)
		{
			GreatGrand grand = _TableManager.Seat[i].Target;

			if(grand == null) continue;

			int targ_grumps = grand.GetGrumps(false);
			yield return new WaitForSeconds(Time.deltaTime  * 5);

			Sprite s = targ_grumps >= 0 ? GameManager.UI.Sprites.GetObject("Correct") as Sprite :  
								         GameManager.UI.Sprites.GetObject("Incorrect") as Sprite;

			UIAlert a = GameManager.UI.ImgAlert(s, grand.Face.transform.position, -1.0F);
			a.transform.localScale = Vector3.one * 0.7F;
			a.TweenActive(true);

			if(targ_grumps >= 0) correct.Add(a);
			else wrong.Add(a);

			yield return new WaitForSeconds(Time.deltaTime  * 14);
		}


		total.Txt[0].text = FinalScore + "";
		total.Txt[1].text = "HAPPY\nGRANDS";
		total.Txt[1].color = Color.white;
		total.TweenActive(true);
	
		yield return new WaitForSeconds(0.8F);

		bool isCounting = true;

		for(int i = 0; i < correct.Count; i++)
		{
			SendCorrectAlert(correct[i],total.transform);
		}

		for(int i = 0; i < wrong.Count; i++)
		{
			wrong[i].transform.DOScale(Vector3.zero, 0.6F).OnComplete(()=>{});
		}

		while(isCounting)
		{
			total.Txt[0].text = FinalScore + "";
			bool complete = true;
			for(int i = 0; i < correct.Count; i++)
			{
				if(correct[i] != null) 
				{
					complete = false;
					break;
				}
			}
			if(complete) isCounting = false;
			yield return null;
		}

		yield return new WaitForSeconds(0.8F);

		int rep = FinalScore * 5;

		total.Txt[0].text = rep + "";
		total.Txt[1].text = "REP";
		
		float repscale = 1.05F + ((float)rep / 150);
		repscale = Mathf.Clamp(repscale, 1.05F, 1.4F);
		Tweens.Bounce(total.transform, Vector3.one * repscale);

		if(Timer_current < Timer)
		{
			yield return new WaitForSeconds(0.8F);

			float mult = (1.0F - Timer_current/Timer);
			mult = Mathf.Clamp(1.0F + mult, 1.0F, 1.5F);
			rep = (int) ((float) rep * mult);
			total.Txt[0].text = rep + "";
			total.Txt[1].text = "TIME";
			total.Txt[1].color = Color.red;

			repscale = 1.05F + ((float)rep / 150);
			repscale = Mathf.Clamp(repscale, 1.05F, 1.4F);
			Tweens.Bounce(total.transform, Vector3.one * repscale);
		}
		
		if(FinalScore == GG_num)
		{
			yield return new WaitForSeconds(0.8F);
			rep *= 2;
			total.Txt[0].text = rep + "";
			total.Txt[1].text = "PERFECT!";
			total.Txt[1].color = Color.blue;

			repscale = 1.05F + ((float)rep / 150);
			repscale = Mathf.Clamp(repscale, 1.05F, 1.4F);
			Tweens.Bounce(total.transform, Vector3.one * repscale);
		}
		

		yield return StartCoroutine(GameManager.UI.ResourceAlert(GameManager.WorldRes.Rep, rep));
	
		endgame[2].TweenActive(true);
		endgame[2].ClearActions();
		endgame[2].AddAction(UIAction.MouseUp, () => 
		{
			Clear();
			StartCoroutine(Enter(false, new IntVector(0,0)));
		});
	}

	public void SendCorrectAlert(UIAlert u, Transform t)
	{
		u.transform.DOScale(Vector3.one * 0.4F, 0.3F);
		u.transform.DOMove(t.position, 0.6F).OnComplete(()=>{
			Tweens.Bounce(t);
			FinalScore++;
			u.PoolDestroy();
		});
	}
	

	public void GenerateGrumpsPrimitive(GreatGrand g, int num)
	{
		g.Grumps = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			GrumpObj targ = GetRandomGG(g);
			_Grump newg = new _Grump(false, g, targ);
			g.Grumps[i] = newg;
		}
	}

	public void GenerateGrumpsReal(GreatGrand g, int num)
	{
		_Grump [] final = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			bool has_targ = false;

			GrumpObj targ = null;

			while(!has_targ)
			{
				targ = GetRandomGG(g);
				has_targ = true;

				for(int x = 0; x < i; x++)
				{
					if(final[x].Target == targ) has_targ = false;
				}
			}
			int dist = _TableManager.SeatDistance(targ as GreatGrand, g);
			bool like = false;
			if(dist == 1) like = true;
			

			final[i] = new _Grump(like, g, targ);	
		}
		g.Grumps = final;	
	}

	public int [] ShuffleGG()
	{	
		int [] fin = new int[_TableManager.Seat.Length];
		List<_Seat> finalpos = new List<_Seat>();
		finalpos.AddRange(_TableManager.Seat);

		for(int i = 0; i < GG_num; i++)
		{
			int num = Random.Range(0, finalpos.Count);
			_Seat point = finalpos[num];
			if(point == Grands[i].Seat)
			{
				num = Random.Range(0, finalpos.Count);
				point = finalpos[num];
			}

			fin[i] = finalpos[num].Index;
			Grands[i].SitImmediate(point);
			finalpos.RemoveAt(num);

		}

		return fin;
	}

	public GreatGrand GetRandomGG(GreatGrand g)
	{
		GreatGrand final = Grands[Random.Range(0, Grands.Length)];
		while(final == g) final = Grands[Random.Range(0, Grands.Length)];
		return final;
	}

	public GreatGrand GetNonNeighbourGG(GreatGrand g)
	{
		return _TableManager.GetNonNeighbourSeat(g.Seat).Target;
	}

	public void FocusOn(InputTarget t)
	{
		if(t is GreatGrand) GameManager.UI.SetGrandUI(t as GreatGrand);
	}

	public bool Resolved
	{
		get{
			for(int i = 0; i < Grands.Length; i++)
			{
				if(Grands[i]==null) continue;
				if(!Grands[i].IsHappy) return false;
			}
			return true;
		}
	}

	public int NumberHappy
	{
		get{
			int num = 0;
			for(int i = 0; i < Grands.Length; i++)
			{
				if(Grands[i].IsHappy) num++;
			}
			return num;
		}
	}

	public bool AllSeated
	{
		get{
			for(int i = 0; i < Grands.Length; i++)
			{
				if(!Grands[i].isSeated) return false;
			}
			return true;
		}
	}

}
