using UnityEngine;
using System.Collections;

public class GreatGrand : GrumpObj {
	public GreatGrand_Data Info;
	public int Index;
	public _Seat Seat;
	public _Grump [] Grumps;

	public GreatGrand Relation;

	public SpriteRenderer Img, Emotion;
	public Sprite Happy, Sad;

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

	private _Seat drag_targ;
	public override void Drag(Vector3 pos)
	{
		base.Drag(pos);
		Face.transform.position = transform.position;
		Face.transform.localRotation = Quaternion.Euler(0,0,0);
		if(isDragging)
		{
			if(drag_targ == null) drag_targ = Seat;
			_Seat n = GameManager.Table.NearestSeat(this.transform.position);

			if(n != drag_targ)
			{
				drag_targ.Reset();
				drag_targ = n;
				drag_targ.Highlight(true);	
			}
			ShowGrumpLines(); 
		}
	}

	public override void Release()
	{
		//GameManager.UI.ShowFace(Face);
		if(isDragging)
		{
			drag_targ = GameManager.Table.NearestSeat(this.transform.position);
			if(drag_targ.CanSeat(this)) SitAt(drag_targ);
			else SitAt(Seat);
		}
		base.Release();
	}

	public override void Tap()
	{
		base.Tap();
		ShowGrumpLines();
	}

	bool lines_show = false;
	float lines_time = 0.0F;
	void Update()
	{
		if(lines_show)
		{
			lines_time -= Time.deltaTime;
			for(int i = 0; i < GrumpLines.Length; i++)
			{
				Color c = (Grumps[i].LikesIt ? Color.green : Color.red);
				c.a = Mathf.Clamp01(lines_time);
				GrumpLines[i].SetColors(c,c);
			}

			if(lines_time <= 0.0F) lines_show = false;
		}
	}

	public void SetGrumps(params _Grump [] g)
	{
		Grumps = g;
	}

	public void SitAt(_Seat s)
	{
		if(s == null) return;
		if(s.Target)
		{
			_Seat temp = Seat;
			Seat = null;
			s.Target.SitAt(temp);
		}
		Seat = s;
		Seat.SetTarget(this);
		Vector3 sitpos = Seat.Object.transform.position;
		sitpos.y = 0.5F;
		transform.position = sitpos;
		Face.transform.position = transform.position;
		Face.transform.rotation = Seat.Object.transform.rotation * Quaternion.Euler(90, 0,0);
		GameManager.instance.CheckGrumps();
	}

	public void CheckEmotion()
	{
		if(IsHappy) Emotion.sprite = Happy;
		else Emotion.sprite = Sad;
	}

	public FaceObj Face;
	//private FaceObj MiniFace;

	public void ResetFace()
	{

	}

	public void SetFace(FaceObj f)
	{
		Face = f;

		Face.transform.SetParent(GameManager.GetCanvas());
		Face.GetComponent<RectTransform>().anchorMax = Vector2.one;
		Face.GetComponent<RectTransform>().sizeDelta = Vector3.zero;

		Face.Start();
		Face.Reset(Info.Base);
		(Face[0] as FaceObj).SetInfo(Info.EyeLeft);
		(Face[1] as FaceObj).SetInfo((Info.EyeRight));
		(Face[2][0] as FaceObj).Reset((Info.EarLeft));
		(Face[3][0] as FaceObj).Reset((Info.EarRight));
		(Face[4][0] as FaceObj).Reset((Info.BrowLeft));
		(Face[5][0] as FaceObj).Reset((Info.BrowRight));
		(Face[6][0] as FaceObj).Reset((Info.Hair));
		(Face[8][0] as FaceObj).Reset((Info.Nose));
		(Face[7][0] as FaceObj).Reset((Info.Jaw));

		Face.transform.position = Emotion.transform.position;
		Face.transform.localRotation = Quaternion.Euler(0,0,0);
		Face.transform.localScale = new Vector3(0.35F, 0.2F, 0.2F);
	}


	private LineRenderer [] GrumpLines;
	public void CreateGrumpLines()
	{
		GrumpLines = new LineRenderer [Grumps.Length];
		for(int i = 0; i < Grumps.Length; i++)
		{
			GrumpLines[i] = (LineRenderer) Instantiate(GameManager.instance.GrumpLine);
			GrumpLines[i].transform.SetParent(this.transform);

			Vector3 a = this.transform.position;
			Vector3 b = Grumps[i].Target.transform.position;
			GrumpLines[i].SetPosition(0, Vector3.Lerp(a,b,0.15F));
			GrumpLines[i].SetPosition(1, Vector3.Lerp(b,a,0.15F));

			Color c = (Grumps[i].LikesIt ? Color.green : Color.red);
			GrumpLines[i].SetColors(c,c);
			GrumpLines[i].gameObject.SetActive(false);
		}

	}

	public void ShowGrumpLines()
	{
		lines_show = true;
		lines_time = 1.8F;
		for(int i = 0; i < Grumps.Length; i++)
		{
			Vector3 a = this.transform.position;
			Vector3 b = Grumps[i].Target.transform.position;
			GrumpLines[i].SetPosition(0, Vector3.Lerp(a,b,0.15F));
			GrumpLines[i].SetPosition(1, Vector3.Lerp(b,a,0.15F));

			Color c = (Grumps[i].LikesIt ? Color.green : Color.red);
			GrumpLines[i].SetColors(c,c);
			GrumpLines[i].gameObject.SetActive(true);
		}
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

