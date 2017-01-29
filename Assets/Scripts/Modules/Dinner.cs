using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using DG.Tweening;

public class Dinner : Module {

	public TableManager _TableManager;

	public static int Difficulty = 0;
	public static int [] Difficulty_GG = new int[]
	{
		4, 6, 8
	};
	public static int GGNum{
		get{return Difficulty_GG[Difficulty];}
	}

	private static float [] Difficulty_Timer = new float []
	{
		10.0F, 20.0F, 25.0F
	};
	public static float Timer
	{
		get
		{
			return Difficulty_Timer[Difficulty];
		}
	}

	public VectorObject2D GrumpLine;
	private int [] GG_Indexes;

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

	private GreatGrand Target;
	private bool isDragging;
	private _Seat drag_targ;	
	private float drag_distance = 3.4F;
	public override void ControlledUpdate()
	{
		if(Running)
		{

		//TIMER
			Timer_current += Time.deltaTime;
			if(Timer_current >= Timer)
			{
				Complete();
			}

			if(timerobj == null) timerobj = MUI["kitchen"];

			float timer_ui = (Timer - Timer_current);
			int secs = (int) timer_ui % 60;

			timerobj.Txt[0].text = (int) timer_ui + "";
			timerobj.Txt[0].color = (Timer_current > Timer*0.75F) ? Color.red : Color.white;

			if(secs != last_sec) 
			{
				last_sec = secs;
				if (Timer_current > Timer*0.75F) 
					Tweens.Bounce(timerobj.Txt[0].transform, 
									Vector3.one + (Vector3.one * Timer_current / Timer/2));
			}

		//TARGET GRAND

			if(Target != null)
			{
				if(isDragging)
				{
					if(drag_targ == null) drag_targ = Target.Seat;
					_Seat n = _TableManager.NearestSeat(GameManager.InputPos);

					if(n != drag_targ)
					{
						drag_targ.Reset();
						drag_targ = n;
						drag_targ.Highlight(true);	
					}

					Vector3 dpos = GameManager.InputPos;
					//dpos.y = Target.Face.transform.position.y;
					//dpos += new Vector3(0.0F, 0.0F, -0.5F);

					Target.Face.transform.position = Vector3.Lerp(Target.Face.transform.position, dpos, Time.deltaTime * 20);
					Target.Face.transform.LookAt(_TableManager.TableObj.transform, Vector3.up);
					
					Target.GrumpLines(0.8F, true); 
				}
				else 
				{
					Target.Face.transform.position = Target.Seat.Position;
					Target.Face.transform.rotation = Target.Seat.Rotation * Quaternion.Euler(4, 0,0);
					Target.GrumpLines(0.8F, true); 
					if(Vector3.Distance(GameManager.InputPos, Target.Face.transform.position) > drag_distance)
					{
						isDragging = true;
					}
				}
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
		MUI["kitchen"].AddAction(UIAction.MouseDown, ()=>
		{
			Complete();
		});
		MUI[3].AddAction(UIAction.MouseDown, ()=>
		{
			StartCoroutine(GameManager.instance.LoadModule("Menu"));
		});

		if(timerobj == null) timerobj = MUI["kitchen"];

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
		f.Append(MUI[7].transform.DOLocalRotate(new Vector3(0, 25,0), 0.1F));
		f.Append(MUI[7].transform.DOLocalRotate(new Vector3(0, 35,0), 0.2F));
		f.Append(MUI[7].transform.DOLocalRotate(new Vector3(0, 0,0), 0.15F));

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
		_TableManager.Clear();
		MUI["faceparent"].DestroyChildren();

		UIObj end = MUI["endgame"];
		for(int i = 0; i < end.Length; i++)
		{
			end[i].TweenActive(false);
		}
	}

	public void SetupFace(FaceObj f, GreatGrand g)
	{
		f.AddAction(UIAction.MouseDown, () =>
		{
			GameManager.instance.SetTargetGrand(g);
			SetTarget(g);
		});

		f.AddAction(UIAction.MouseUp, () =>
		{
			GameManager.instance.SetTargetGrand(null);
			ReleaseTarget();
		});
	}


	public void SetTarget(GreatGrand g)
	{
		Target = g;
		Target.ShowGrumpLines();
	}

	public void ReleaseTarget()
	{
		drag_targ = _TableManager.NearestSeat(GameManager.InputPos);
		DragSit(Target, drag_targ);	
		//if(drag_targ.CanSeat(Target) && drag_targ != Target.Seat) Target.DragSit(drag_targ);
		//else 

		Target = null;
	}

	public void DragSit(GreatGrand t, _Seat s)
	{
		if(s == null) return;

		if(s.Target && s.Target != t)
		{
			t.Seat.Target = null;

			StartCoroutine(SitAt(s.Target, t.Seat));
		}

		t.Seat = s;
		t.Seat.SetTarget(t);

		Vector3 sitpos = t.Seat.transform.position;
		t.transform.position = sitpos;

		t.Face.transform.position = t.Seat.Position;
		t.Face.transform.rotation = t.Seat.Rotation;
		//t.Face.transform.localScale = new Vector3(0.2F, 0.2F, 1.0F);
		t.isSeated = true;
	}

	public IEnumerator SitAt(GreatGrand t, _Seat s)
	{
		if(s == null) yield break;
		int ind = t.Seat.Index;

		if(s.Target && s.Target != t)
		{
			t.Seat.Target = null;
			StartCoroutine(s.Target.SitAt(t.Seat));
		}

		yield return StartCoroutine(_TableManager.MoveSeat(t, ind, s.Index, 0.22F));

		t.Seat = s;
		t.Seat.SetTarget(t);

		Vector3 sitpos = t.Seat.transform.position;
		sitpos.y -= 0.5F;
		t.transform.position = sitpos;

		t.Face.transform.position = t.Seat.Position;
		t.Face.transform.rotation = t.Seat.Rotation;
		//Face.transform.localScale = new Vector3(0.35F, 0.35F, 1.0F);
		
		t.isSeated = true;
		//GameManager.instance.CheckGrumps();
	}

	public void CreateDinnerGame(int d = 0)
	{
		Running = false;

		UIObj fparent = MUI["faceparent"];
		Difficulty = d;
		Grands = new GreatGrand[GGNum];

		_TableManager.SetupTable(Difficulty);
		for(int i = 0; i < GGNum; i++)
		{
			if(i < GameManager.instance.Grands.Length)
				Grands[i] = GameManager.instance.Grands[i].GrandObj;
			else Grands[i] = GameManager.instance.Generator.Generate(i);

			Grands[i].gameObject.SetActive(true);

			FaceObj f = GameManager.instance.Generator.GenerateFace(Grands[i]);
			fparent.AddChild(f);
			f.transform.localScale = Vector3.one * 0.32F;
			SetupFace(f, Grands[i]);
		}

		for(int i = 0; i < GGNum; i++)
		{
			Grands[i].SitImmediate(_TableManager.Seat[i]);
		}

		for(int i = 0; i < GGNum; i++)
		{
			int gnum = 1;
			if(Random.value > 0.8F) gnum ++;
			GenerateGrumpsReal(Grands[i], gnum);
		}

		GG_Indexes = new int[0];
		while(NumberHappy > GGNum/2) GG_Indexes = ShuffleGG();

		for(int i = 0; i < GGNum; i++)
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
			Grands[i].Face.transform.position = _TableManager.EntryDoor.position;
		}

		for(int i = Grands.Length-1; i >= 0; i--)
		//for(int i = 0; i < _TableManager.Seat.Length; i++)
		{
			StartCoroutine(_TableManager.DoorToSeat(Grands[i], index[i], 0.35F));
			yield return new WaitForSeconds(Time.deltaTime * 10);
		}

		while(!AllSeated) yield return null;
		Running = true;
		Timer_current = 0.0F;

		timerobj.Txt[0].text = "";
		timerobj.Svg[0].transform.localScale = Vector3.one;
		timerobj.TweenActive(true);
		
		yield return null;
	}

	int FinalScore = 0;
	float final_timer = 0.8F;
	IEnumerator EndDinner()
	{
		while(!AllSeated) yield return null;

		Running = false;
		GameManager.IgnoreInput = true;
		timerobj.TweenActive(false);

		UIObj endgame = MUI["endgame"];

		endgame[0].TweenActive(true);
		endgame[1].SetActive(false);
		endgame[2].SetActive(false);

		endgame[0].Txt[0].text = "Let's Eat!";

		FinalScore = 0;

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
			a.transform.localScale = Vector3.one * 0.5F;
			a.TweenActive(true);

			if(targ_grumps >= 0) correct.Add(a);
			else wrong.Add(a);

			yield return new WaitForSeconds(0.22F);
		}

		UIObj total = endgame[1];
		total.Txt[0].text = FinalScore + "";
		total.Txt[1].text = "HAPPY\nGRANDS";
		total.Txt[1].color = Color.white;
		total.TweenActive(true);
	
		yield return new WaitForSeconds(0.5F);

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

		yield return new WaitForSeconds(0.4F);

		int rep = FinalScore * 5;

		total.Txt[0].text = rep + "";
		total.Txt[1].text = "REP";
		
		float repscale = 1.5F + ((float)rep / 150);
		repscale = Mathf.Clamp(repscale, 1.05F, 3.4F);
		Tweens.Bounce(total.transform, Vector3.one * repscale);

		if(Timer_current < Timer)
		{
			yield return new WaitForSeconds(0.5F);

			float mult = (1.0F - Timer_current/Timer);
			mult = Mathf.Clamp(1.0F + mult, 1.0F, 1.5F);
			rep = (int) ((float) rep * mult);
			total.Txt[0].text = rep + "";
			total.Txt[1].text = "TIME";
			total.Txt[1].color = Color.red;

			repscale = 1.5F + ((float)rep / 150);
			repscale = Mathf.Clamp(repscale, 1.05F, 3.4F);
			Tweens.Bounce(total.transform, Vector3.one * repscale);
		}
		
		if(FinalScore == GGNum)
		{
			yield return new WaitForSeconds(0.5F);
			rep *= 2;
			total.Txt[0].text = rep + "";
			total.Txt[1].text = "PERFECT!";
			total.Txt[1].color = Color.blue;

			repscale = 1.5F + ((float)rep / 150);
			repscale = Mathf.Clamp(repscale, 1.05F, 3.4F);
			Tweens.Bounce(total.transform, Vector3.one * repscale);
		}

		for(int i = Grands.Length-1; i >= 0; i--)
		{
			StartCoroutine(_TableManager.Exit(Grands[i], 0.3F));
		}
		

		StartCoroutine(GameManager.UI.ResourceAlert(GameManager.WorldRes.Rep, rep));
		GameManager.IgnoreInput = false;

		yield return new WaitForSeconds(final_timer);
		Clear();

		StartCoroutine(Enter(false, new IntVector(0,0)));
		/*endgame[2].TweenActive(true);
		endgame[2].ClearActions();
		endgame[2].AddAction(UIAction.MouseUp, () => 
		{
			
		});*/
	}

	public void SendCorrectAlert(UIAlert u, Transform t)
	{
		u.transform.DOScale(Vector3.one * 0.4F, 0.23F);
		u.transform.DOMove(t.position, 0.4F).OnComplete(()=>{
			//Tweens.Bounce(t);
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

		for(int i = 0; i < GGNum; i++)
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
