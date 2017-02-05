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
				new UIQuote("Carer", "A Grand has sprained their back!",
									"Tilt the screen to guide them to the ambulance!")
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
	public float Safeway_Threshold = 1.5F;

	//If the player has 'stopped' the grand (any input)
	public bool Stopped = false;

	public int Difficulty = 0;

	private float [] Difficulty_Timer = new float []
	{
		30.0F, 35.0F, 40.0F
	};
	public float Timer {get {return Difficulty_Timer[Difficulty];} }

	private int [] Difficulty_PathPoints = new int []
	{
		3, 5, 8
	};
	public int PathPoints {get {return Difficulty_PathPoints[Difficulty];} }
	public Color PathColor;

	public float MoveSpeed = 0.4F;
	public float MoveSpeed_inc = 0.1F;
	public float MoveSpeed_actual = 0.0F;
	public Vector3 Velocity;
	private Vector3 CrossVelocity;

	private Vector3 Sway_CurrentVelocity;
	private float Sway_Speed = 2;
	private float Sway_Timer;
	private float Sway_Extra;
	private Vector2 Sway_TimeBracket = new Vector2(0.2F, 0.5F);

	public Vector3 Control_Velocity;
	private float Control_Speed = 170F;
	private Vector2 BalancePoint;

	private float GameTime;

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
			Control_Velocity = new Vector3(inacc.x*1.4F, 0.0F, 0.0F);
		}    
		else
		{
			if(Input.GetKey(KeyCode.A)) Control_Velocity = new Vector3(-1,0,0);
			else if(Input.GetKey(KeyCode.D)) Control_Velocity = new Vector3(1,0,0);
			else Control_Velocity = Vector3.zero;
		}
		
		if(Stopped && !Input.GetMouseButton(0)) MoveSpeed_actual = MoveSpeed;
		Stopped = Input.GetMouseButton(0);

		MoveSpeed_actual += !Stopped ? MoveSpeed_inc : - MoveSpeed_actual/2;
		Pathway.transform.position += 
			Pathway.transform.up * MoveSpeed_actual * Time.deltaTime;

		float MoveSpeed_factor = MoveSpeed_actual / MoveSpeed;
		if(!Stopped)
		{
			BalancePoint = Safeway.GetPoint01(DistanceAlongPath());
			Sway_CurrentVelocity = (BalancePoint -  TargetGrand_Face.GetUIPosition());

			float rot = Sway_CurrentVelocity.x/4;
			rot += Sway_Extra;
			rot += Control_Velocity.x /8;
			rot = Mathf.Clamp(rot, -40, 40);
			TargetGrand_Face.transform.rotation = Quaternion.Slerp(
				TargetGrand_Face.transform.rotation,
				Quaternion.Euler(0.0F,0.0F, rot),
				Time.deltaTime * 10
			);

			if((Sway_Timer-=Time.deltaTime) < 0.0F)
			{
				Sway_Extra = (Random.value - Random.value) * 4;
				Sway_Timer = Random.Range(Sway_TimeBracket.x, Sway_TimeBracket.y);
			}

			if(Sway_CurrentVelocity == Vector3.zero) 
			{
				Sway_CurrentVelocity = CrossVelocity * (Random.value - Random.value);
			}
			Sway_CurrentVelocity.Normalize();
			Sway_Speed = Vector2.Distance(BalancePoint, 
				TargetGrand_Face.GetUIPosition())*(MoveSpeed_factor);

			Sway_Speed = Mathf.Clamp(Sway_Speed, 0.8f, Control_Speed * (0.5F + 0.05F * MoveSpeed_factor));
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

		MUI["dist"].Txt[0].text = Control_Velocity.x + "";

		
	}

	public IEnumerator Win()
	{
		Running = false;

		TargetGrand_Face.transform.position = EndPoint.position;
		WinObj.TweenActive(true);
		WinObj.Txt[0].text = "SAFE!";

		yield return new WaitForSeconds(0.7F);
		int med = 10 + (int) Mathf.Clamp(10 - GameTime, 0, 50);
		WinObj.Txt[0].text = med + " MEDS";
		Tweens.Bounce(WinObj.Txt[0].transform);
		StartCoroutine(GameManager.UI.ResourceAlert(GameManager.WorldRes.Meds, med));
		EndButton.TweenActive(true);
		TargetGrand.Data.Fitness.Add(-50);
	}

	public void Lose()
	{
		Running = false;

		TargetGrand_Face.transform.rotation = Quaternion.Slerp(
			TargetGrand_Face.transform.rotation,
			Quaternion.Euler(0.0F,0.0F, Sway_CurrentVelocity.x*90),
				Time.deltaTime * 60);

		LoseObj.TweenActive(true);
		EndButton.TweenActive(true);
	}



	private UIObj EndButton, WinObj, LoseObj;
	private Vector3 Pathway_init;
	public override void InitUI()
	{
		Pathway = MUI["pathway"];
		Pathway_init = Pathway.transform.position;

		EndButton = MUI["endgame"];
		EndButton.ClearActions();
		EndButton.AddAction(UIAction.MouseUp, ()=>
		{
			Clear();
			StartCoroutine(StartGame());
			});		

		WinObj = MUI["wingame"];
		LoseObj = MUI["losegame"];
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
	}

	IEnumerator StartGame()
	{
		EndButton.TweenActive(false);
		WinObj.TweenActive(false);
		LoseObj.TweenActive(false);

		yield return StartCoroutine(CreatePath());

		GameTime = 0.0F;
		Running = true;
	}

	IEnumerator CreatePath()
	{
		Pathway.transform.position = Pathway_init;
		MoveSpeed_actual = MoveSpeed;

		int checks = 0;
		while(TargetGrand == null || TargetGrand.Data.Fitness.Ratio < 0.3F || checks > 4)
		{
			int r = Random.Range(0, GameManager.instance.Grands.Length);
			TargetGrand = GameManager.instance.Grands[r].GrandObj;
			yield return null;
		}
		if(TargetGrand == null) TargetGrand = GameManager.instance.Generator.Generate(0);
		

		TargetGrand_Face = GameManager.instance.Generator.GenerateFace(TargetGrand);
		FaceParent.AddChild(TargetGrand_Face);

		TargetGrand_Face.transform.localPosition = Vector3.zero;// StartPoint.position;
		BalancePoint = TargetGrand_Face.GetUIPosition();

		MiddlePoints = new Transform[PathPoints];

		Velocity = EndPoint.position - StartPoint.position;
		Velocity.Normalize();
		CrossVelocity = Vector3.Cross(Velocity, -Vector3.forward).normalized;

		for(int i = 0; i < PathPoints; i++)
		{
			GameObject g = new GameObject("Path Point " + i);
			MiddlePoints[i] = g.transform;
			MiddlePoints[i].position = Vector3.Lerp(StartPoint.position, EndPoint.position,
													Mathf.Clamp(0.2F + (0.2F * i), 0.0F, 1.0F));
			MiddlePoints[i].position += CrossVelocity * ((Random.value - Random.value) * 300.0F);
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
	 
		int mvmt_segments = 2 + MiddlePoints.Length;

	 	Safeway = new VectorLine("Safeway Path", new List<Vector2>(mvmt_segments+1), Safeway_Threshold*1.5F, LineType.Continuous);
		Safeway.MakeSpline(splinepoints.ToArray(), mvmt_segments, 0, false);
		Safeway.drawTransform = MUI["pathway"].transform;
		Safeway.SetCanvas(UIObj._UICanvas);
		Safeway.rectTransform.SetParent(MUI[2].transform);
		Safeway.SetColor(PathColor);
		Safeway.joins = Joins.Fill;

		yield return null;
	}

}
