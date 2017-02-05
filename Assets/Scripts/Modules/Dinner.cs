using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using DG.Tweening;

public class Dinner : Module {

	public TableManager _TableManager;

	public int Difficulty = 0;
	private int [] Difficulty_GG = new int[]
	{
		5, 6, 8
	};
	public int GGNum {get {return Difficulty_GG[Difficulty];} }

	private float [] Difficulty_Timer = new float []
	{
		35.0F, 50.0F, 60.0F
	};
	public float Timer {get {return Difficulty_Timer[Difficulty];} }

	private int [] Difficulty_ThirdEyeCost = new int []
	{
		4,6,8
	};
	public int ThirdEyeCost {get {return Difficulty_ThirdEyeCost[Difficulty];}}

	public int DinnerCost = 10;

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
	private float drag_distance = 0.8F;
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

			timerobj.Txt[1].text = "SERVING IN";
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
					_Seat n = _TableManager.NearestSeat(Input.mousePosition);
					if(n != drag_targ)
					{
						drag_targ.Reset();
						drag_targ = n;
						drag_targ.Highlight(true);	
					}

					Vector2 npos = Vector2.Lerp(Target.Face.GetUIPosition(), Input.mousePosition, Time.deltaTime * 30);
					Target.Face.SetUIPosition(npos);
					Target.Face.transform.LookAt(_TableManager.TableObj.transform, Vector3.forward);
					Target.Face.transform.rotation *= Quaternion.Euler(80, 0,180);
					Target.GrumpLines(0.8F, true); 
				}
				else 
				{
					Vector3 dpos = GameManager.InputPos;
					dpos.y = Target.Face.transform.position.y;

					Target.Face.transform.position = Target.Seat.Position;
					Target.Face.transform.rotation = Target.Seat.Rotation * Quaternion.Euler(5, 0,0);
					Target.GrumpLines(0.8F, true); 
					if(Vector3.Distance(dpos, Target.Face.transform.position) > drag_distance)
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
		MUI["thirdeye"].AddAction(UIAction.MouseDown, () =>
		{
			if(GameManager.WorldRes.Meds.Charge(ThirdEyeCost))
			{
				Ability_ThirdEye();
			}	
		});
		MUI["thirdeye"].Txt[0].text = ThirdEyeCost + "";
		MUI["kitchen"].AddAction(UIAction.MouseDown, ()=>
		{
			Complete();
		});
		
		if(timerobj == null) timerobj = MUI["kitchen"];
		MUI.SetActive(false);
	}

	public override IEnumerator Enter(bool entry, IntVector v)
	{
		this.gameObject.SetActive(true);
		yield return StartCoroutine(Load());
		
		MUI.SetActive(true);
		GameManager.UI.PermUI["exit"].TweenActive(true);
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
		CreateDinner();
		yield return StartCoroutine(StartGame(GG_Indexes));
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
		f.Insert(0.0F, Tweens.PictureSway(MUI["thirdeye"].transform, new Vector3(0,0,12 * v.x)));

		s.Insert(0.35F, f);

		return s;
	}

	public FaceObj [] Faces;
	public void CreateDinner(int d = 0)
	{
		Running = false;

		UIObj fparent = MUI["faceparent"];
		Difficulty = d;
		Grands = new GreatGrand[GGNum];

		_TableManager.SetupTable(Difficulty);
		Faces = new FaceObj[GGNum];
		for(int i = 0; i < GGNum; i++)
		{
			if(i < GameManager.instance.Grands.Length && GameManager.instance.Grands[i].Hunger.Current > 60)
				Grands[i] = GameManager.instance.Grands[i].GrandObj;
			else Grands[i] = GameManager.instance.Generator.Generate(i);

			Grands[i].gameObject.SetActive(true);

			Faces[i] = GameManager.instance.Generator.GenerateFace(Grands[i]);
			fparent.AddChild(Faces[i]);
			SetupFace(Faces[i], Grands[i]);
		}

		for(int i = 0; i < GGNum; i++)
		{
			Grands[i].SitImmediate(_TableManager.Seat[i]);
		}

		for(int i = 0; i < GGNum; i++)
		{
			int gnum = 1 + Random.Range(0, Difficulty);
			//if(Random.value > 0.8F) gnum ++;
			GenerateGrumpsReal(Grands[i], gnum);
		}

		GG_Indexes = new int[0];
		while(NumberHappy > GGNum/2) GG_Indexes = ShuffleGG();

		for(int i = 0; i < GGNum; i++)
		{
			Faces[i].SetActive(false);
		}
	}

	IEnumerator StartGame(int [] index)
	{
		yield return null;
		for(int i = 0; i < Grands.Length; i++)
		{
			Faces[i].SetActive(true);
			//Faces[i].transform.position = _TableManager.EntryDoor.position;
		}

		for(int i = Grands.Length-1; i >= 0; i--)
		//for(int i = 0; i < _TableManager.Seat.Length; i++)
		{
			StartCoroutine(_TableManager.DoorToSeat(Grands[i], index[i], 0.35F));
			//yield return new WaitForSeconds(Time.deltaTime * 10);
		}

		while(!AllSeated) yield return null;

		Ability_ThirdEye(1.3F);
		Running = true;
		Timer_current = 0.0F;

		timerobj.Txt[0].text = "";
		timerobj.transform.localScale = Vector3.one;
		timerobj.TweenActive(true);
		
		yield return null;
	}

	int FinalScore = 0;
	float final_timer = 0.8F;
	public float Bonus_DifficultyMultiplier = 1.0F;
	public float Bonus_TimerMax = 1.0F;
	public float Bonus_TimerDecay = 1.0F;
	public float Bonus_Perfect = 2.0F;

	IEnumerator EndGame()
	{
		while(!AllSeated) yield return null;

		UIObj endgame = MUI["endgame"];
		Running = false;
		GameManager.IgnoreInput = true;
		timerobj.Txt[0].text = "";
		timerobj.Txt[1].text = "";

		if(!GameManager.WorldRes.Funds.Charge(DinnerCost))
		{
			endgame[0].TweenActive(true);
			endgame[1].SetActive(false);
			endgame[2].SetActive(false);

			endgame[0].Txt[0].text = "No Funds!";

			FinalScore = 0;

			yield return new WaitForSeconds(0.8F);

			endgame[0].TweenActive(false);

			yield return StartCoroutine(FinishDinner());

			yield break;
		}

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
			a.SetActive(false);
			a.transform.localScale = Vector3.one * 0.5F;
			a.TweenActive(true);

			if(targ_grumps >= 0) correct.Add(a);
			else wrong.Add(a);

			yield return new WaitForSeconds(0.22F);
		}

		UIObj info = endgame[1];
		UIObj points = endgame[2];
		info.Txt[0].text = "HAPPY GRANDS";
		points.Txt[0].text = FinalScore + "";
		info.Txt[0].color = Color.white;

		info.TweenActive(true);
		points.TweenActive(true);
	
		yield return new WaitForSeconds(0.5F);

		bool isCounting = true;

		for(int i = 0; i < correct.Count; i++)
		{
			SendCorrectAlert(correct[i],points.transform);
		}

		for(int i = 0; i < wrong.Count; i++)
		{
			wrong[i].transform.DOScale(Vector3.zero, 0.3F).OnComplete(()=>{});
		}

		while(isCounting)
		{
			points.Txt[0].text = FinalScore + "";
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

		yield return new WaitForSeconds(0.2F);

		int rep = (int) (FinalScore * (1.0F+(Difficulty * Bonus_DifficultyMultiplier)));
		points.Txt[0].text = rep + "";
		
		if(Timer_current < Timer)
		{
			yield return new WaitForSeconds(0.5F);

			float mult = 1.0F - (Timer_current/Timer*Bonus_TimerDecay);
			mult = Mathf.Clamp(1.0F + mult, 1.0F, Bonus_TimerMax);
			rep = (int) ((float) rep * mult);
			points.Txt[0].text = rep + "";
			info.Txt[0].text = "TIME BONUS";
			info.Txt[0].color = Color.green;

			Tweens.Bounce(info.transform);
			yield return null;
			Tweens.Bounce(points.transform);
		}
		
		if(FinalScore == GGNum)
		{
			yield return new WaitForSeconds(0.5F);
			rep = (int) ((float)rep * Bonus_Perfect);
			points.Txt[0].text = rep + "";
			info.Txt[0].text = "PERFECT!";
			info.Txt[0].color = Color.blue;

			Tweens.Bounce(info.transform);
			yield return null;
			Tweens.Bounce(points.transform);
		}

		StartCoroutine(GameManager.UI.ResourceAlert(GameManager.WorldRes.Rep, rep));
		yield return StartCoroutine(FinishDinner());
	}

	IEnumerator FinishDinner()
	{
		for(int i = Grands.Length-1; i >= 0; i--)
		{
			StartCoroutine(_TableManager.Exit(Grands[i], 0.3F));
			Grands[i].Data.Hunger.Add(-50);
			UIAlert a = GameManager.UI.StringAlert("-50% Hunger", Faces[i].transform.position, 1.2F);
			a.Txt[0].color = Color.white;
			a.Txt[0].fontSize = 40;

			a.TweenActive(true);
		}

		yield return new WaitForSeconds(final_timer);
		GameManager.IgnoreInput = false;
		Clear();
		StartCoroutine(Enter(false, new IntVector(0,0)));
		yield return null;
	}

	public override void Complete()
	{
		if(!Running) return;
		StartCoroutine(EndGame());
	}

	public override void Clear()
	{
		_TableManager.Clear();

		for(int x = 0; x < Grands.Length; x++)
		{
			for(int i = 0; i < Grands[x].Grumps.Length; i++)
			{
				Grands[x].Grumps[i].Destroy();
			}
		}

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

	private void Ability_ThirdEye(float time = 2.5F)
	{
		for(int i = 0; i < Grands.Length; i++)
		{
			Grands[i].GrumpLines(time, false);
		}
	}


	public void SetTarget(GreatGrand g)
	{
		Target = g;
		StartCoroutine(GrumpAlert(Target.Grumps[0]));
		//Target.ShowGrumpLines();
	}

	public void ReleaseTarget()
	{
		drag_targ = _TableManager.NearestSeat(Input.mousePosition);
		DragSit(Target, drag_targ);	

		//if(drag_targ.CanSeat(Target) && drag_targ != Target.Seat) Target.DragSit(drag_targ);
		//else 
		isDragging = false;
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

		t.Face.transform.position = t.Seat.Position;
		t.Face.transform.rotation = t.Seat.Rotation;
		t.Face.transform.rotation *= Quaternion.Euler(0, 0,180);
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

		t.Face.transform.position = t.Seat.Position;
		t.Face.transform.rotation = t.Seat.Rotation;
		t.Face.transform.rotation *= Quaternion.Euler(0, 0,180);		
		t.isSeated = true;
	}


	public void SendCorrectAlert(UIAlert u, Transform t)
	{
		u.transform.DOScale(Vector3.one * 0.4F, 0.23F);
		u.transform.DOMove(t.position, 0.3F).OnComplete(()=>{
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


	public IEnumerator GrumpAlert(_Grump g)
	{
		float time_pause = 1.2F;

		UIObj alert = Instantiate(GameManager.UI.Prefabs.GetObject("grump") as GameObject).GetComponent<UIObj>();
		GameManager.UI.WorldObjects.AddChild(alert);
		alert.Init(-1,	GameManager.UI.WorldObjects, 1.0F, 0.0F);

		alert.Child[0].AddChild(Instantiate(g.Target.UIObject));
		alert.Img[0].sprite = g.LikesIt ? 
		GameManager.UI.Sprites.GetObject("Correct") as Sprite :  
		GameManager.UI.Sprites.GetObject("Incorrect") as Sprite;

		alert.SetActive(false);
		alert.transform.localScale = Vector3.one * 1.7F;
		alert.TweenActive(true);

		float t = 0.0F;
		while( Target == g.Parent)
		{
			Vector3 targpos = g.Parent.Position;
			/*if(targpos.x + alert.RectT.rect.width > Screen.width)
			{
				Vector3 sc = new Vector3(-1,1,1);
				alert.transform.localScale = sc;
				alert.Img[0].transform.localScale = sc;
				alert.Child[0].transform.localScale = sc;
			}*/
			alert.transform.position = targpos;
			alert.Child[0].transform.rotation = Quaternion.identity;
			alert.Img[0].transform.rotation = Quaternion.identity;
			yield return null;
		}

		alert.TweenActive(false);
		yield return new WaitForSeconds(0.4F);
		if(alert != null) alert.PoolDestroy();
		yield return null;
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
