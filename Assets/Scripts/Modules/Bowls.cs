using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Vectrosity;
using DG.Tweening;
using Filthworks;

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
	private Face TargetGrand_Face;
	public UIObj FaceParent;
	private FOBJ Pathway;
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
			int [] d = new int[]{150, 80, 60};
			return d[Difficulty];
		}
	}
	private float Safeway_Threshold_actual = 100.0F;

	//If the player has 'stopped' the grand (any input)
	public bool Stopped = false;

	public int Difficulty = 0;

	private float TotalDistance =  800;
	private int [] Difficulty_PathPoints = new int []
	{
		5,7,8
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
	private float Control_Speed = 20;
	private Vector3 BalancePoint;

	private float GameTime;
	private UIObj FitnessObj;
	private float FitnessPerTick = 0.01F;

	private float face_rotation_x = -33.0F;

	public float DistanceAlongPath()
	{
		Vector3 pos = MOB.transform.position;
		Vector3 start = StartPoint.position;
		Vector3 end = EndPoint.position;

		//Get distance from start and end
		float d_s = Vector3.Distance(pos, start);
		float d_e = Vector3.Distance(pos, end);

		//Add distances together then divide the distance from the start by the total
		float d_total = d_s + d_e;
		return d_s / d_total;
	}

	public float DistanceFromSafeway()
	{
		Safeway.SetDistances();
		BalancePoint = Safeway.GetPoint3D01(DistanceAlongPath());
		Vector3 vpos = MOB[1].pos;
		vpos.y = BalancePoint.y;
		vpos.z = BalancePoint.z;
		return Vector3.Distance(BalancePoint, vpos);
	}

	public override void ControlledUpdate()
	{
		if(!Running) return;
		GameTime += Time.deltaTime;

		Safeway.Draw3D();
		if(EndPoint.position.y > 2)
		{
			//WIN
			StartCoroutine(Win());
			return;
		}

		if(DistanceFromSafeway() > Safeway_Threshold/20)
		{
			Lose();
		}

		if(Application.isMobilePlatform)
		{
			Vector2 inacc = Input.acceleration;
			Control_Velocity = new Vector3(inacc.x*2.2F, 0.0F, 0.0F);
			Control_Velocity.x = Mathf.Clamp(Control_Velocity.x, -1.3F, 1.3F);
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
		moveinc *= Mathf.Clamp(1.0F + Vector3.Distance(BalancePoint, TargetGrand_Face.pos)/4,
								0.7F, 1.6F);

		MoveSpeed_actual += Stopped ? -MoveSpeed_actual/2 : moveinc;
		MoveSpeed_actual = Mathf.Clamp(MoveSpeed_actual, 0.0F, MoveSpeed_Max);
		Pathway.transform.position += 
			Pathway.transform.up * MoveSpeed_actual * Time.deltaTime;

		float MoveSpeed_factor = MoveSpeed_actual / MoveSpeed;
		if(!Stopped)
		{
			BalancePoint = Safeway.GetPoint3D01(DistanceAlongPath());
			Sway_CurrentVelocity = (BalancePoint -  MOB[1].pos);

			Rotation();

			if(Sway_CurrentVelocity == Vector3.zero) 
			{
				Sway_CurrentVelocity = CrossVelocity * (Random.value - Random.value);
			}
			Sway_CurrentVelocity.Normalize();
			Sway_Speed = Vector3.Distance(BalancePoint, 
				MOB[1].pos)*(MoveSpeed_factor);

			Sway_Speed = Mathf.Clamp(Sway_Speed, 0.0f, Control_Speed * (0.05F + 0.02F * MoveSpeed_factor));
			
			Sway_CurrentVelocity.y = 0.0F;
			Sway_CurrentVelocity.z = 0.0F;
			Control_Velocity.y = 0.0F;
			Control_Velocity.z = 0.0F;

			Vector3 finalpos = MOB[1].transform.position;
			finalpos -= Sway_CurrentVelocity * Sway_Speed * Time.deltaTime;
			finalpos += Control_Velocity * Control_Speed * Time.deltaTime;	
			MOB[1].transform.position = finalpos;
		}

		//Vector3 fpos =  MOB[1].transform.localPosition;
		//fpos.y = 0.0F + (MoveSpeed_factor * 100);

		//MOB[1].transform.localPosition = Vector3.Lerp(
		//	MOB[1].transform.localPosition,
		//	fpos,
		//	Time.deltaTime * 5);
	}

	public void Rotation()
	{
		if((Sway_Timer-=Time.deltaTime) < 0.0F)
		{
			Sway_Extra = (Random.value - Random.value) * 10;
			Sway_Timer = Random.Range(Sway_TimeBracket.x, Sway_TimeBracket.y);
		}

		Vector3 v = (Safeway.GetPoint3D01(DistanceAlongPath()+0.05F) - Safeway.GetPoint3D01(DistanceAlongPath())).normalized;
		
		float rotz = v.x*5;
		float roty = v.y * 13;

		rotz += Sway_CurrentVelocity.x * Sway_Speed * 0.7F;
		rotz += Sway_Extra;
		rotz += Control_Velocity.x * 15;
		rotz = Mathf.Clamp(rotz, -60, 60);

		
		MOB[1].transform.rotation = Quaternion.Slerp(
			MOB[1].transform.rotation,
			Quaternion.Euler(face_rotation_x,roty, rotz),
			Time.deltaTime * 10
		);
	}

	public IEnumerator Win()
	{
		Running = false;

		MOB[1].transform.position = EndPoint.position;
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

		MOB[1].transform.rotation = Quaternion.Slerp(
			MOB[1].transform.rotation,
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
		Pathway = MOB["pathway"];
		Pathway_init = Pathway.transform.position;

		EndButton = MUI["reset"];
		EndButton.ClearActions();
		EndButton.AddAction(UIAction.MouseUp, ()=>
		{
			Clear();
			StartCoroutine(Reload());
			});		

		EndInfo = MUI["endcond"];
		FitnessObj = MUI["fitobj"];
	}

	public IEnumerator Reload()
	{
		float speed = 0.3F;
		while(Vector3.Distance(Pathway.transform.position, Pathway_init)> 0.3F)
		{
			Vector3 vel = (Pathway_init) - Pathway.transform.position;
			Pathway.transform.position += vel * speed;
			yield return null;
		}
		
		yield return StartCoroutine(Load());
		yield return StartCoroutine(StartGame());
	}
	public override IEnumerator Load()
	{
		yield return StartCoroutine(CreatePath());
	}

	public override IEnumerator Enter(bool entry, IntVector v)
	{
		this.gameObject.SetActive(true);
		yield return StartCoroutine(Load());
		
		//MUI.SetActive(true);
		MOB.SetActive(true);
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
		MOB[1].DestroyChildren();
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

	public FOBJ [] PathPrefabs;
	public FOBJ [] PathInstances;

	IEnumerator CreatePath()
	{
		Pathway.transform.position = Pathway_init;
		MoveSpeed_actual = MoveSpeed;

		Safeway_Threshold_actual = Safeway_Threshold;

		int checks = 0;
		while(TargetGrand == null || TargetGrand.Data.Fitness.Ratio > 0.6F)
		{
			int r = Random.Range(0, GameManager.instance.Grands.Length);
			TargetGrand = GameManager.instance.Grands[r].GrandObj;
			checks ++;
			if(checks > 10) break;
		}

		if(TargetGrand == null) TargetGrand = GameManager.Generator.Generate(0);
		
		TargetGrand_Face = GameManager.Generator.GenerateNewFace(TargetGrand.Data);
		MOB[1].T.rotation = Quaternion.Euler(face_rotation_x,0,0);
		MOB[1].T.localPosition = Vector3.zero;
		MOB[1].AddChild(TargetGrand_Face);

		TargetGrand_Face.transform.localPosition = Vector3.up * 4;
		BalancePoint = Vector3.zero;


		EndPoint.position = StartPoint.position - StartPoint.up * TotalDistance;
		MOB[0][0].transform.localScale = new Vector3(19.5F, TotalDistance /3, 1.0F);

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
			MiddlePoints[i].position += CrossVelocity * ((Random.value - Random.value)*10);
			MiddlePoints[i].SetParent(MOB[0].transform);
		}
		List<float> splinewidths = new List<float>();
		List<Vector3> splinepoints = new List<Vector3>();
		splinepoints.Add(StartPoint.localPosition);
		splinewidths.Add(Safeway_Threshold);
	 	for(int i = 0; i < MiddlePoints.Length; i++)
	 	{
	 		splinepoints.Add(MiddlePoints[i].localPosition);
	 		splinewidths.Add(Safeway_Threshold/i+1);
	 	}
	 	splinepoints.Add(EndPoint.localPosition);
	 	splinewidths.Add(0.0F);
	 	VectorLine.Destroy(ref Safeway);
	 
		int mvmt_segments = 10 + MiddlePoints.Length * 10;

	 	Safeway = new VectorLine("Safeway Path", new List<Vector3>(mvmt_segments+1), Safeway_Threshold_actual*1.2F, LineType.Continuous);
		Safeway.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, false);

		Safeway.drawTransform = MOB[0].transform;
		Safeway.SetColor(PathColor);
		Safeway.joins = Joins.Fill;

		int objnum = 20 + MiddlePoints.Length * 5;
		PathInstances = new FOBJ[objnum];

		float point = 0.0F;
		float objratio = 0.9F/objnum;
		for(int i = 0; i < objnum; i++)
		{
			PathInstances[i] = (FOBJ) Instantiate(PathPrefabs[Random.Range(0, PathPrefabs.Length)]);
			MOB[0].AddChild(PathInstances[i]);

			point += Random.Range(objratio/1.5F, objratio*1.5F);

			Vector3 pos = Safeway.GetPoint3D01(point);
			Vector3 nextpos = Safeway.GetPoint3D01(point+0.1F);
			Vector3 vel = (nextpos - pos).normalized;

			Vector3 cross = Vector3.Cross(vel, -Vector3.forward).normalized;
			float xd = cross.x * Safeway_Threshold_actual/10;
			pos.x += Random.value > 0.5F ? xd : -xd;

			PathInstances[i].T.position = pos;
			PathInstances[i].T.LookAt(pos + vel);
			PathInstances[i].T.rotation *= Quaternion.Euler(0,90,90);
		}
		Safeway.Draw3D();
		VectorLine.SetCamera3D(Camera.main);
		VectorManager.useDraw3D = true;

		yield return null;
	}

}
