using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class GreatGrand : GrumpObj {
	public GreatGrand_Data Info;
	public int Index;
	public _Seat Seat;
	public _Grump [] Grumps;

	public GreatGrand Relation;

	public SpriteRenderer Emotion;

	public bool IsHappy
	{
		get{
			for(int i = 0; i < Grumps.Length; i++)
			{
				if(!Grumps[i].Resolved) return false;
			}
			return true;
		}
	}
	public bool isSeated = false;

	private _Seat drag_targ;
	public override void Drag(Vector3 pos)
	{
		base.Drag(pos);
		GameManager.instance.FocusOn(this);
		
		if(isDragging)
		{
			if(drag_targ == null) drag_targ = Seat;
			_Seat n = GameManager.Table.NearestSeat(pos);

			if(n != drag_targ)
			{
				drag_targ.Reset();
				drag_targ = n;
				drag_targ.Highlight(true);	
			}

			Vector3 dragpos = drag_targ.transform.position;
			dragpos.y = pos.y;
			if(Vector3.Distance(pos, dragpos) > 0.6F) GameManager.instance.TargetLine(this.transform.position, pos);
			else GameManager.instance.TargetLine(this.transform.position, dragpos);

			ShowGrumpLines(); 
		}
		else 
		{
			Face.transform.position = Seat.Position;
			Face.transform.rotation = Seat.Rotation * Quaternion.Euler(65, 0,0);
			CheckEmotion();
		}
	}

	public override void Release(Vector3 pos)
	{
		GameManager.instance.TargetLine(Vector3.zero, Vector3.zero);
		if(isDragging)
		{
			drag_targ = GameManager.Table.NearestSeat(pos);
			if(drag_targ.CanSeat(this) && drag_targ != Seat) StartCoroutine(SitAt(drag_targ, true));
			else StartCoroutine(SitAt(Seat));
			//lines_time = 0.01F;
			
		}
		base.Release(pos);
	}

	public override void Tap(Vector3 pos)
	{
		base.Tap(pos);
		ShowGrumpLines();
		GameManager.instance.FocusOn(this);
	}

	bool lines_show = false;
	float lines_time = 0.0F;
	void Update()
	{
		if(lines_show)
		{
			lines_time -= Time.deltaTime;
			for(int i = 0; i < GrumpL.Length; i++)
			{
				Color c = (Grumps[i].LikesIt ? Color.green : Color.red);
				c.a = Mathf.Clamp01(lines_time);
				GrumpL[i].SetColor(c);
				GrumpL[i].Draw();
			}

			if(lines_time <= 0.0F) lines_show = false;
		}

	}

	public void Destroy()
	{
		for(int i = 0; i < GrumpL.Length; i++)
		{
			VectorLine.Destroy(ref GrumpL[i]);
		}
		Destroy(Face.gameObject);
		Destroy(this.gameObject);
	}

	public void SetGrumps(params _Grump [] g)
	{
		Grumps = g;
	}

	public void SitImmediate(_Seat s)
	{
		if(s == null) return;

		if(s.Target)
		{
			_Seat temp = Seat;
			Seat = null;
			s.Target.SitImmediate(temp);
		}

		Seat = s;
		Seat.SetTarget(this);

		Vector3 sitpos = Seat.transform.position;
		sitpos.y += 0.5F;
		transform.position = sitpos;

		Face.transform.position = Seat.Position;
		Face.transform.rotation = Seat.Rotation * Quaternion.Euler(65, 0,0);
		Face.transform.localScale = new Vector3(0.45F, 0.45F, 1.0F);
		CheckEmotion(false);
		isSeated = true;
		GameManager.instance.CheckGrumps();
	}

	public IEnumerator SitAt(_Seat s, bool alert = false)
	{
		if(s == null) yield break;

		if(s.Target && s.Target != this)
		{
			Seat.Target = null;
			StartCoroutine(s.Target.SitAt(Seat, alert));
		}

		if(alert)
		{
			Emotion.enabled = false;
			yield return StartCoroutine(GameManager.Table.MoveSeat(this, Seat.Index, s.Index, 0.6F));
			Emotion.enabled = true;
		}

		Seat = s;
		Seat.SetTarget(this);

		Vector3 sitpos = Seat.transform.position;
		sitpos.y += 0.5F;
		transform.position = sitpos;

		Face.transform.position = Seat.Position;
		Face.transform.rotation = Seat.Rotation * Quaternion.Euler(65, 0,0);
		Face.transform.localScale = new Vector3(0.45F, 0.45F, 1.0F);
		
		CheckEmotion(alert);

		GameManager.instance.CheckGrumps();
	}

	public void CheckEmotion(bool seatingalert= false)
	{
		if(IsHappy) 
		{
			Emotion.sprite = GameManager.UI.Happy;
			Emotion.color = Color.green;
		}
		else 
		{
			Emotion.sprite = GameManager.UI.Angry;
			Emotion.color = Color.red;
		}

		if(Face != null) Emotion.transform.position = Face.transform.position + Face.transform.up * 1.3F;

		if(seatingalert)
		{
			StartCoroutine(EmotionRoutine());
		}
	}

	IEnumerator EmotionRoutine()
	{
		ShowGrumpLines(0.5F);	

		Object o = GameManager.UI.Sprites.GetObject("Correct");
		Sprite s = IsHappy ? GameManager.UI.Sprites.GetObject("Correct") as Sprite :  GameManager.UI.Sprites.GetObject("Incorrect") as Sprite;
		UIAlert a = GameManager.UI.ImgAlert(s, Face.transform.position);
		a.transform.localScale = Vector3.zero;

		Utilities.ActionSingle b = a.AddStep(Vector3.zero, 0.4F);
		b.Scale = Vector3.one * 1.0F;
		a.AddStep(Vector3.zero, 0.3F);
		b = a.AddStep(Vector3.zero, 0.2F);
		b.Scale = -Vector3.one;
		yield return null;
	}

	public FaceObj Face;
	//private FaceObj MiniFace;

	public void ResetFace(FaceObj f)
	{
		f.SetSkinColor(Info.Color_Skin);
		f.SetHairColor(Info.Color_Hair);
		f.SetOffsetColor(Info.Color_Offset);

		f.Reset(Info.Base);
		(f[0] as FaceObj).SetInfo(Info.EyeLeft);
		(f[1] as FaceObj).SetInfo((Info.EyeRight));
		(f[2][0] as FaceObj).Reset((Info.EarLeft));
		(f[3][0] as FaceObj).Reset((Info.EarRight));
		(f[4][0] as FaceObj).Reset((Info.BrowLeft));
		(f[5][0] as FaceObj).Reset((Info.BrowRight));
		(f[6][0] as FaceObj).Reset((Info.Hair));
		(f[8][0] as FaceObj).Reset((Info.Nose));
		(f[7][0] as FaceObj).Reset((Info.Jaw));
	}

	public void SetFace(FaceObj f)
	{
		Face = f;
		GameManager.GetFaceParent().AddChild(Face);

		Face.Start();
		Face.Reset(Info.Base);
		(Face[0] as FaceObj).SetInfo(Info.EyeLeft);
		(Face[1] as FaceObj).SetInfo((Info.EyeRight));
		(Face[2] as FaceObj).SetInfo((Info.EarLeft));
		(Face[3] as FaceObj).SetInfo((Info.EarRight));
		(Face[4] as FaceObj).SetInfo((Info.BrowLeft));
		(Face[5] as FaceObj).SetInfo((Info.BrowRight));
		(Face[6] as FaceObj).SetInfo((Info.Hair));
		(Face[8] as FaceObj).SetInfo((Info.Nose));
		(Face[7] as FaceObj).SetInfo((Info.Jaw));

		Emotion.transform.position = Face.transform.position + Face.transform.up;

	}

	public FaceObj CloneFace()
	{
		FaceObj final = (FaceObj) Instantiate(Face);

		final.Start();

		final.SetSkinColor(Info.Color_Skin);
		final.SetHairColor(Info.Color_Hair);
		final.SetOffsetColor(Info.Color_Offset);

		/*final.Reset(Info.Base);
		(final[0] as FaceObj).SetInfo(Info.EyeLeft);
		(final[1] as FaceObj).SetInfo((Info.EyeRight));
		(final[2] as FaceObj).SetInfo((Info.EarLeft));
		(final[3] as FaceObj).SetInfo((Info.EarRight));
		(final[4] as FaceObj).SetInfo((Info.BrowLeft));
		(final[5] as FaceObj).SetInfo((Info.BrowRight));
		(final[6] as FaceObj).SetInfo((Info.Hair));
		(final[8] as FaceObj).SetInfo((Info.Nose));
		(final[7] as FaceObj).SetInfo((Info.Jaw));*/
		return final;
	}



	//private LineRenderer [] GrumpLines;
	private VectorLine [] GrumpL;
	public void CreateGrumpLines()
	{
		//GrumpLines = new LineRenderer [Grumps.Length];
		GrumpL = new VectorLine[Grumps.Length];
		for(int i = 0; i < Grumps.Length; i++)
		{
			//GrumpLines[i] = (LineRenderer) Instantiate(GameManager.instance.GrumpLine);
			//GrumpLines[i].transform.SetParent(this.transform);

			GrumpL[i] = new VectorLine("Grump: " + Grumps[i].Target, new List<Vector3>(), 12.0F, LineType.Discrete); //(VectorObject2D) Instantiate(GameManager.instance.GrumpLine);

			Vector3 a = this.transform.position;
			Vector3 b = Grumps[i].Target.transform.position;

			float d = Vector3.Distance(a,b);
			int steps = (int) (d/0.3F);
			
				GrumpL[i].points3.Add(Vector3.Lerp(a, b, 0.15F));
				GrumpL[i].points3.Add(Vector3.Lerp(a, b, 0.85F));


			GrumpL[i].SetColor(new Color(0,0,0,0));
		}

	}

	public void ShowGrumpLines(float time = 1.8F)
	{
		StartCoroutine(GrumpLineRoutine(time));
	}

	IEnumerator GrumpLineRoutine(float time)
	{
		float curr = 0.0F;
		float spawntime = 0.25F;
		while((curr+= Time.deltaTime) < spawntime)
		{
			for(int i = 0; i < Grumps.Length; i++)
			{
				Color c = (Grumps[i].LikesIt ? Color.green : Color.red);
				c.a = 1.0F;
				GrumpL[i].SetColor(c);

				Vector3 finalpos = Grumps[i].Target.transform.position;
				float ratio = 0.15F + (curr/spawntime * 0.7F);
				GrumpL[i].points3[0] = Vector3.Lerp(Face.transform.position, finalpos, 0.15F);
				GrumpL[i].points3[1] = Vector3.Lerp(Face.transform.position, finalpos, ratio);
				GrumpL[i].Draw();
			}
			yield return null;
		}
		
		lines_show = true;
		lines_time = time;
	}

	public void Generate(int _index)
	{
		Index = _index;
		bool gender = Index % 2 == 0;
		if(Random.value < 0.05F) gender = !gender;

		Info.Gender = gender;
		Info.Name = gender ? GreatGrand_Data.Names_Male_Random : GreatGrand_Data.Names_Female_Random;
		Info.GFactor = 0.55F;

		Info.Age = Random.Range(80, 115);
		//Add grump based on age if male, remove if female
		float agefactor = Mathf.Clamp((float)Info.Age/100.0F, 0.0F, 0.2F);
		Info.GFactor += gender ? agefactor : -agefactor;

		//Info.MStat = Random.value > 0.65F ? MaritalStatus.Married : (Random.value > 0.8F ? MaritalStatus.Divorced : MaritalStatus.Donor);
		//Add grump if divorced, remove if married
		switch(Info.MStat)
		{
			case MaritalStatus.Married:
			Info.GFactor -= 0.15F;
			break;
			case MaritalStatus.Divorced:
			Info.GFactor += 0.15F;
			break;
			case MaritalStatus.Donor:

			break;
		}

		Info.Military = Random.value > 0.95F;
		transform.name = Info.Name;
		//Emotion.color = GameManager.Data.GG_Colours[Index];
	}

	public void SetMaritalStatus(MaritalStatus m)
	{
		Info.MStat = m;
	}
}

	public enum MaritalStatus 
	{
		Married, Divorced, Donor
	}

	public enum NationStatus
	{
		Australian, British, American, Japanese, Sudanese, Chinese, Greek, Vietnamese
	}

	[System.Serializable]
	public class GreatGrand_Data
	{
		public bool Gender;
		public int Age;
		public string Name;
		public bool Military;

		public MaritalStatus MStat;
		public NationStatus Nationality;

		public float GFactor = 0.75F;

		public static string [] Names_Male = new string []
		{
			"Ralph",
			"Wally",
			"Ed",
			"Thomas",
			"Max",
			"Luton"
		};

		public static string Names_Male_Random
		{
			get{return Names_Male[Random.Range(0, Names_Male.Length)];}
		}

		public static string [] Names_Female = new string [] 
		{
			"Lucille",
			"Sandy",
			"Meryl",
			"Barb",
			"Louise"
		};

		public static string Names_Female_Random
		{
			get{return Names_Female[Random.Range(0, Names_Female.Length)];}
		}

		public FaceObjInfo  EyeLeft, EyeRight,
							EarLeft, EarRight,
							BrowLeft, BrowRight,
							Base, Hair, Jaw, Nose;

		public Color Color_Skin, Color_Hair, Color_Offset;

	}
