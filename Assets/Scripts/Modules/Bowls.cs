using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;

public class Bowls : Module {


	public override UIQuote [] Intro_String
	{
		get{
			return new UIQuote[]
			{
				new UIQuote("Carer", "Grands enjoy exercise and the outdoors.",
									 "But some Grands have trouble with balancing and controlling their movement",
									"Tilt the screen to keep the Grand on the path!")
			};
		}
	}

	public GreatGrand TargetGrand;
	private FaceObj TargetGrand_Face;
	public UIObj FaceParent;
	private UIObj Pathway;
	//Grands used as hazards
	public GreatGrand [] Hazard_Grands;

	//Points along the path
	public Transform StartPoint, EndPoint;
	public Transform [] MiddlePoints;

	//Vectored path you must follow to guide the grand
	public VectorLine Safeway;
	//The distance you can get from the path before failing
	public float Safeway_Threshold
	{
		get{
			int [] d = new int[]{120, 90, 60};
			return d[Difficulty];
		}
	}
	private float Safeway_Threshold_actual = 100.0F;

	//If the player has 'stopped' the grand (any input)
	public bool Stopped = false;

	public int Difficulty = 0;

	private float TotalDistance =  12000;
	private int [] Difficulty_PathPoints = new int []
	{
		6,7,8
	};

	public int PathPoints {get {return Difficulty_PathPoints[Difficulty];} }
	public Color PathColor;

	public float MoveSpeed = 0.4F;
	public float MoveSpeed_inc = 0.1F;
	public float MoveSpeed_Max = 150;

	private float MoveSpeed_actual = 0.0F;
	
	private Vector3 Velocity;
	private Vector3 CrossVelocity;

	private Vector3 Sway_CurrentVelocity;
	private float Sway_Speed = 2;
	private float Sway_Timer;
	private float Sway_Extra;
	private Vector2 Sway_TimeBracket = new Vector2(0.2F, 0.5F);

	private Vector3 Control_Velocity;
	private float Control_Speed = 190F;
	private Vector2 BalancePoint;

	private float GameTime;
	private UIObj FitnessObj;
	private float FitnessPerTick = 0.01F;

	public float DistanceAlongPath()
	{
		Vector3 pos = TargetGrand_Face.transform.position;
		Vector3 start = StartPoint.position;
		Vector3 end = EndPoint.position;

		//Get distance from start and end
		float d_s = Vector2.Distance(pos, start);
		float d_e = Vector2.Distance(pos, end);

		//Add distances together then divide the distance from the start by the total
		float d_total = d_s + d_e;
		return d_s / d_total;
	}

	public float DistanceFromSafeway()
	{
		Safeway.SetDistances();
		BalancePoint = Safeway.GetPoint01(DistanceAlongPath());
		return Vector2.Distance(BalancePoint, TargetGrand_Face.transform.position);
	}

	public override void ControlledUpdate()
	{
		if(!Running) return;
		GameTime += Time.deltaTime;

		Safeway.Draw();
		if(Safeway.GetPoint01(1.0F).y > TargetGrand_Face.transform.position.y)
		{
			//WIN
			StartCoroutine(Win());
			return;
		}

		if(DistanceFromSafeway() > Safeway_Threshold)
		{
			Lose();
		}

		if(Application.isMobilePlatform)
		{
			Vector2 inacc = Input.acceleration;
			Control_Velocity = new Vector3(inacc.x*2.2F, 0.0F, 0.0F);
		}    
		else
		{
			if(Input.GetKey(KeyCode.A)) Control_Velocity = new Vector3(-1,0,0);
			else if(Input.GetKey(KeyCode.D)) Control_Velocity = new Vector3(1,0,0);
			else Control_Velocity = Vector3.zero;
		}
		Movement();

		float tickrate = Mathf.Clamp(1.0F + MoveSpeed_actual / MoveSpeed, 1.1F, 3.0F);
		if(TargetGrand.Data.Fitness.Ratio < 1.0F)
			TargetGrand.Data.Fitness.Add(FitnessPerTick * tickrate);
		else TargetGrand.Data.Fitness.AddMax(FitnessPerTick * tickrate/5);

		FitnessObj.Img[1].transform.localScale = new Vector3(TargetGrand.Data.Fitness.Ratio, 1,1);
	}

	private void Movement()
	{
		if(Stopped && !Input.GetMouseButton(0)) MoveSpeed_actual = MoveSpeed;
		Stopped = Input.GetMouseButton(0);

		float moveinc = MoveSpeed_inc;
		moveinc *= Mathf.Clamp(1.0F + Vector2.Distance(BalancePoint, TargetGrand_Face.GetUIPosition())/4,
								0.7F, 1.6F);

		MoveSpeed_actual += Stopped ? -MoveSpeed_actual/2 : moveinc;
		MoveSpeed_actual = Mathf.Clamp(MoveSpeed_actual, 0.0F, MoveSpeed_Max);
		Pathway.transform.position += 
			Pathway.transform.up * MoveSpeed_actual * Time.deltaTime;

		float MoveSpeed_factor = MoveSpeed_actual / MoveSpeed;
		if(!Stopped)
		{
			BalancePoint = Safeway.GetPoint01(DistanceAlongPath());
			Sway_CurrentVelocity = (BalancePoint -  TargetGrand_Face.GetUIPosition());

			Rotation();

			if((Sway_Timer-=Time.deltaTime) < 0.0F)
			{
				Sway_Extra = (Random.value - Random.value) * 7;
				Sway_Timer = Random.Range(Sway_TimeBracket.x, Sway_TimeBracket.y);
			}

			if(Sway_CurrentVelocity == Vector3.zero) 
			{
				Sway_CurrentVelocity = CrossVelocity * (Random.value - Random.value);
			}
			Sway_CurrentVelocity.Normalize();
			Sway_Speed = Vector2.Distance(BalancePoint, 
				TargetGrand_Face.GetUIPosition())*(MoveSpeed_factor+0.8F);

			Sway_Speed = Mathf.Clamp(Sway_Speed, 0.8f, Control_Speed * (0.2F + 0.06F * MoveSpeed_factor));
			Sway_CurrentVelocity.y = 0.0F;
			Control_Velocity.y = 0.0F;

			Vector3 finalpos = TargetGrand_Face.transform.position;
			finalpos -= Sway_CurrentVelocity * Sway_Speed * Time.deltaTime;
			finalpos += Control_Velocity * Control_Speed * Time.deltaTime;	
			TargetGrand_Face.transform.position = finalpos;
		}

		Vector3 fpos =  TargetGrand_Face.transform.localPosition;
		fpos.y = 0.0F + (MoveSpeed_factor * 100);

		TargetGrand_Face.transform.localPosition = Vector3.Lerp(
			TargetGrand_Face.transform.localPosition,
			fpos,
			Time.deltaTime * 5);
	}

	public void Rotation()
	{
		Vector2 v = (Safeway.GetPoint01(DistanceAlongPath()+0.1F) - Safeway.GetPoint01(DistanceAlongPath())).normalized;
		
		float rot = v.x*5;

		rot += Sway_CurrentVelocity.x*0.7F;
		rot += Sway_Extra;
		rot += Control_Velocity.x /2;
		rot = Mathf.Clamp(rot, -65, 65);
		
		TargetGrand_Face.transform.rotation = Quaternion.Slerp(
			TargetGrand_Face.transform.rotation,
			Quaternion.Euler(0.0F,0.0F, rot),
			Time.deltaTime * 10
		);
	}

	public IEnumerator Win()
	{
		Running = false;

		TargetGrand_Face.transform.position = EndPoint.position;
		EndInfo.TweenActive(true);
		EndInfo.Txt[0].text = "SAFE!";

		yield return new WaitForSeconds(0.7F);

		EndInfo.Txt[0].text = "FITNESS UP: " + (TargetGrand.Data.Fitness.RatioToString()) + "%";
		Tweens.Bounce(EndInfo.Txt[0].transform);

		yield return new WaitForSeconds(Time.deltaTime * 40);

		int fit = 30 + (int) Mathf.Clamp(50 - GameTime, 0, 30);
		TargetGrand.Data.Fitness.Add(fit);

		EndInfo.Txt[0].text = "FITNESS UP: " + (TargetGrand.Data.Fitness.RatioToString()) + "%";
		Tweens.Bounce(EndInfo.Txt[0].transform);

		yield return new WaitForSeconds(Time.deltaTime * 40);
		int rep = fit;
		StartCoroutine(GameManager.UI.ResourceAlert(GameManager.WorldRes.Rep, rep));
		EndInfo.Txt[0].text = "REP UP: " + rep;
		Tweens.Bounce(EndInfo.Txt[0].transform);

		EndButton.TweenActive(true);
	}

	public void Lose()
	{
		Running = false;

		TargetGrand_Face.transform.rotation = Quaternion.Slerp(
			TargetGrand_Face.transform.rotation,
			Quaternion.Euler(0.0F,0.0F, Sway_CurrentVelocity.x*90),
				Time.deltaTime * 60);
		EndInfo.TweenActive(true);
		EndInfo.Txt[0].text = "FELL OVER!";
		EndButton.TweenActive(true);
	}

	private UIObj EndButton, EndInfo;
	private Vector3 Pathway_init;
	public override void InitUI()
	{
		Pathway = MUI["pathway"];
		Pathway_init = Pathway.transform.position;

		EndButton = MUI["reset"];
		EndButton.ClearActions();
		EndButton.AddAction(UIAction.MouseUp, ()=>
		{
			Clear();
			StartCoroutine(StartGame());
			});		

		EndInfo = MUI["endcond"];
		FitnessObj = MUI["fitobj"];
	}

	public override IEnumerator Enter(bool entry, IntVector v)
	{
		this.gameObject.SetActive(true);
		yield return StartCoroutine(Load());
		
		MUI.SetActive(true);
		GameManager.UI.PermUI["exit"].TweenActive(true);
		GameManager.UI.PermUI["exit"].ClearActions();
		GameManager.UI.PermUI["exit"].AddAction(UIAction.MouseUp, () =>
		{
			StartCoroutine(GameManager.instance.LoadModule("Menu"));
		});
		GameManager.UI.PermUI["exit"][0].TweenActive(false);

		if(entry)
		{
			Sequence f = OpeningSequence(v);
			yield return f.WaitForCompletion();
		}
		yield return StartCoroutine(CheckForIntro());
		yield return StartCoroutine(StartGame());
	}

	public override void Clear()
	{
		FaceParent.DestroyChildren();
		VectorLine.Destroy(ref Safeway);
		for(int i = 0; i < MiddlePoints.Length; i++)
		{
			Destroy(MiddlePoints[i].gameObject);
		}
		for(int i = 0; i < PathInstances.Length; i++)
		{
			PathInstances[i].PoolDestroy();
		}
	}

	IEnumerator StartGame()
	{
		EndButton.TweenActive(false);
		EndInfo.TweenActive(false);
		FitnessObj.SetActive(false);
		yield return StartCoroutine(CreatePath());
		yield return new WaitForSeconds(0.4F);
	
		for(int i = 3; i > 0; i--)
		{
			MUI.Txt[0].text = "" + i;
			Tweens.Bounce(MUI.Txt[0].transform);
			yield return new WaitForSeconds(0.8F);
		}
		MUI.Txt[0].text = "";
		FitnessObj.TweenActive(true);
		FitnessObj.Img[0].transform.localScale = new Vector3(
				Mathf.Clamp(TargetGrand.Data.Fitness.Max / 200, 0.4F, 2.0F),1,1);
		FitnessObj.Img[1].transform.localScale = new Vector3(
			TargetGrand.Data.Fitness.Ratio, 1,1);
		yield return new WaitForSeconds(0.1F);

		GameTime = 0.0F;
		Running = true;
	}

	public UIObj [] PathPrefabs;
	public UIObj [] PathInstances;

	IEnumerator CreatePath()
	{
		Pathway.transform.position = Pathway_init;
		MoveSpeed_actual = MoveSpeed;

		Safeway_Threshold_actual = Safeway_Threshold;
		//if(Application.isMobilePlatform) Safeway_Threshold_actual *= 2;

		int checks = 0;
		while(TargetGrand == null || TargetGrand.Data.Fitness.Ratio > 0.6F)
		{
			int r = Random.Range(0, GameManager.instance.Grands.Length);
			TargetGrand = GameManager.instance.Grands[r].GrandObj;
			checks ++;
			if(checks > 10) break;
		}

		if(TargetGrand == null) TargetGrand = GameManager.Generator.Generate(0);
		
		TargetGrand_Face = GameManager.Generator.GenerateFace(TargetGrand);
		FaceParent.AddChild(TargetGrand_Face);

		TargetGrand_Face.transform.localPosition = Vector3.zero;
		BalancePoint = TargetGrand_Face.GetUIPosition();


		EndPoint.position = StartPoint.position + Vector3.down * TotalDistance;

		MiddlePoints = new Transform[PathPoints];

		Velocity = EndPoint.position - StartPoint.position;
		Velocity.Normalize();
		CrossVelocity = Vector3.Cross(Velocity, -Vector3.forward).normalized;

		float pathrate = 0.9F/PathPoints;
		for(int i = 0; i < PathPoints; i++)
		{
			GameObject g = new GameObject("Path Point " + i);
			MiddlePoints[i] = g.transform;
			MiddlePoints[i].position = Vector3.Lerp(StartPoint.position, EndPoint.position,
													Mathf.Clamp(pathrate + (pathrate * i), 0.0F, 1.0F));
			MiddlePoints[i].position += CrossVelocity * ((Random.value - Random.value) * Screen.width/2);
			MiddlePoints[i].SetParent(MUI["pathway"].transform);
		}

		List<Vector2> splinepoints = new List<Vector2>();
		splinepoints.Add(StartPoint.localPosition);
	 	for(int i = 0; i < MiddlePoints.Length; i++)
	 	{
	 		splinepoints.Add(MiddlePoints[i].localPosition);
	 	}
	 	splinepoints.Add(EndPoint.localPosition);
	 	VectorLine.Destroy(ref Safeway);
	 
		int mvmt_segments = 10 + MiddlePoints.Length * 10;

	 	Safeway = new VectorLine("Safeway Path", new List<Vector2>(mvmt_segments+1), Safeway_Threshold_actual*1.2F, LineType.Continuous);
		Safeway.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, false);
		Safeway.drawTransform = MUI["pathway"].transform;
		Safeway.SetCanvas(UIObj._UICanvas);
		Safeway.rectTransform.SetParent(MUI[2].transform);
		Safeway.SetColor(PathColor);
		Safeway.joins = Joins.Fill;

		int objnum = 20 + MiddlePoints.Length * 5;
		PathInstances = new UIObj[objnum];

		float point = 0.0F;
		float objratio = 0.9F/objnum;
		for(int i = 0; i < objnum; i++)
		{
			PathInstances[i] = (UIObj) Instantiate(PathPrefabs[Random.Range(0, PathPrefabs.Length)]);
			PathInstances[i].SetParent(MUI["pathway"]);

			point += Random.Range(objratio/1.5F, objratio*1.5F);

			Vector2 pos = Safeway.GetPoint01(point);
			Vector2 nextpos = Safeway.GetPoint01(point+0.1F);
			Vector2 vel = (nextpos - pos).normalized;

			Vector3 cross = Vector3.Cross(vel, -Vector3.forward).normalized;
			float xd = cross.x * Safeway_Threshold_actual * 1.1F;
			pos.x += Random.value > 0.5F ? xd : -xd;

			PathInstances[i].SetUIPosition(pos);
			
			PathInstances[i].transform.LookAt(pos + vel);
			PathInstances[i].transform.rotation *= Quaternion.Euler(0,90,90);
		}
		Safeway.Draw();
		yield return null;
	}

}
