using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class GreatGrand : GrumpObj {
	public GrandData Data;
	public GrandInfo Info {
		get{return Data.Info;}
	}
	//public GreatGrand_Data Info;
	public int Index;
	public _Seat Seat;
	public _Grump [] Grumps;
	public _Grump [] AllGrumps
	{
		get{
			List<_Grump> fin = new List<_Grump>();
			fin.AddRange(Grumps);
			fin.AddRange(GameManager.Module.GetRelatedGrumps(this));
			return fin.ToArray();
		}
	}

	public GreatGrand Relation;
	public override Vector3 Position
	{
		get{
			return Face.GetUIPosition();
		}
	}

	public override UIObj UIObject
	{
		get{return Face;}
	}

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

	public int GrumpMeter
	{
		get
		{
			int fin = 0;
			for(int i = 0; i < Grumps.Length; i++)
			{
				fin += (Grumps[i].Resolved) ? 1 : -1;
			}
			return fin;
		}
	}
	public bool isSeated = false;

	public override void Drag(Vector3 pos)
	{
		base.Drag(pos);
		GameManager.instance.FocusOn(this);
	}

	public override void Release(Vector3 pos)
	{
		lines_drawing = false;
		base.Release(pos);
	}

	public override void Tap(Vector3 pos)
	{
		base.Tap(pos);
		//ShowGrumpLines();
		GameManager.instance.FocusOn(this);
	}

	bool lines_show = false;
	float lines_time = 0.0F;
	void Update()
	{
		for(int i = 0; i < Grumps.Length; i++)
		{
			Grumps[i].Update();
		}
	}

	public void Destroy()
	{
		for(int i = 0; i < Grumps.Length; i++)
		{
			Grumps[i].Destroy();
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
		sitpos.y -= 0.5F;
		transform.position = sitpos;

		Face.transform.position = Seat.Position;
		Face.transform.rotation = Seat.Rotation;// * Quaternion.Euler(65, 0,0);
		//Face.transform.localScale = new Vector3(0.35F, 0.35F, 1.0F);
		//CheckEmotion(false);
		isSeated = true;
		//GameManager.instance.CheckGrumps();
	}

	public void DragSit(_Seat s)
	{
		if(s == null) return;

		if(s.Target && s.Target != this)
		{
			Seat.Target = null;
			StartCoroutine(s.Target.SitAt(Seat, false));
		}

		Seat = s;
		Seat.SetTarget(this);

		Vector3 sitpos = Seat.transform.position;
		sitpos.y += 0.5F;
		transform.position = sitpos;

		Face.transform.position = Seat.Position;
		Face.transform.rotation = Seat.Rotation * Quaternion.Euler(65, 0,0);
		Face.transform.localScale = new Vector3(0.35F, 0.35F, 1.0F);
		isSeated = true;
	}

	public IEnumerator SitAt(_Seat s, bool alert = false)
	{
		if(s == null) yield break;

		lines_time = 0.0F;
		lines_show = true;
		if(s.Target && s.Target != this)
		{
			Seat.Target = null;
			StartCoroutine(s.Target.SitAt(Seat, alert));
		}

		yield return StartCoroutine(GameManager.Table.MoveSeat(this, Seat.Index, s.Index, 0.6F));

		Seat = s;
		Seat.SetTarget(this);

		Vector3 sitpos = Seat.transform.position;
		sitpos.y -= 0.5F;
		transform.position = sitpos;

		Face.transform.position = Seat.Position;
		Face.transform.rotation = Seat.Rotation;
		
		if(alert) yield return StartCoroutine(EmotionRoutine());
		isSeated = true;
	}

	public int GetGrumps(bool allgrumps = true)
	{
		//GrumpLines(0.55F, allgrumps);
		return GrumpMeter;
	}

	public IEnumerator EmotionRoutine(bool allgrumps = true)
	{
		yield return StartCoroutine(GrumpLineRoutine(0.8F, allgrumps));

		Sprite s = IsHappy ? GameManager.UI.Sprites.GetObject("Correct") as Sprite :  
							 GameManager.UI.Sprites.GetObject("Incorrect") as Sprite;

		UIAlert a = GameManager.UI.ImgAlert(s, Face.transform.position, -1);
		a.transform.localScale = Vector3.zero;
		a.AddStep(0.4F, Vector3.up*0.5F, Vector3.one*0.6F);
		a.AddStep(0.7F);
		a.AddStep(0.2F, -Vector3.up*0.5F, -Vector3.one*0.6F);


		yield return null;
	}

	public FaceObj Face;

	public void ResetFace(FaceObj f)
	{
		f.SetSkinColor(Info.C_Skin);
		f.SetHairColor(Info.C_Hair);
		f.SetOffsetColor(Info.C_Offset);

		f.Reset(Info.Base);
		(f[0][0] as FaceObj).Reset(Info.Eye);
		(f[1][0] as FaceObj).Reset(Info.Eye);
		(f[2][0] as FaceObj).Reset(Info.Ear);
		(f[3][0] as FaceObj).Reset(Info.Ear);
		(f[4][0] as FaceObj).Reset(Info.Brow);
		(f[5][0] as FaceObj).Reset(Info.Brow);
		(f[6][0] as FaceObj).Reset(Info.Hair);
		(f[8][0] as FaceObj).Reset(Info.Nose);
		(f[7][0] as FaceObj).Reset(Info.Jaw);
	}

	public void SetFace(FaceObj f)
	{
		Face = f;

		Face.Init(0, null);
		Face.Reset(Info.Base);
		(Face.Child[0] as FaceObj).SetInfo( Info.Eye, GameManager.GetGenerator().Eye[Info.Eye.Index].Prefab);
		(Face.Child[1] as FaceObj).SetInfo( Info.Eye, GameManager.GetGenerator().Eye[Info.Eye.Index].Prefab);
		(Face.Child[2] as FaceObj).SetInfo( Info.Ear, GameManager.GetGenerator().Ear[Info.Ear.Index].Prefab);
		(Face.Child[3] as FaceObj).SetInfo( Info.Ear, GameManager.GetGenerator().Ear[Info.Ear.Index].Prefab);
		(Face.Child[4] as FaceObj).SetInfo( Info.Brow, GameManager.GetGenerator().Brow[Info.Brow.Index].Prefab);
		(Face.Child[5] as FaceObj).SetInfo( Info.Brow, GameManager.GetGenerator().Brow[Info.Brow.Index].Prefab);
		(Face.Child[6] as FaceObj).SetInfo( Info.Hair, GameManager.GetGenerator().Hair[Info.Hair.Index].Prefab);
		(Face.Child[8] as FaceObj).SetInfo( Info.Nose, GameManager.GetGenerator().Nose[Info.Nose.Index].Prefab);
		(Face.Child[7] as FaceObj).SetInfo( Info.Jaw, GameManager.GetGenerator().Jaw[Info.Jaw.Index].Prefab);
		
		Face.Child[0][0].Svg[1].transform.localScale = Info.PupilScale;
		Face.Child[1][0].Svg[1].transform.localScale = Info.PupilScale;
		Face.Child[0][0].Svg[1].color = Info.C_Eye;
		Face.Child[1][0].Svg[1].color = Info.C_Eye;

		Face.Child[7][0].Svg[1].transform.SetParent(Face.Svg[1].transform);
		Face.Child[2][0].Svg[1].color = Info.C_Offset;
		Face.Child[3][0].Svg[1].color = Info.C_Offset;
	}

	public FaceObj CloneFace()
	{
		FaceObj final = (FaceObj) Instantiate(Face);

		final.Init(0, null);

		final.SetSkinColor(Info.C_Skin);
		final.SetHairColor(Info.C_Hair);
		final.SetOffsetColor(Info.C_Offset);

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

	bool lines_drawing = false;
	public void ShowGrumpLines(float time = 0.8F)
	{
		//GrumpLines(time, true);
	}

	public void GrumpLines(float time, bool allgrumps)
	{
		_Grump [] toshow = (allgrumps) ? AllGrumps : Grumps;
		for(int i = 0; i < toshow.Length; i++)
		{
			toshow[i].Trace(0.4F, time);
		}
	}

	IEnumerator GrumpLineRoutine(float time, bool allgrumps)
	{
		float curr = 0.0F;
		float spawntime = 0.15F;
		_Grump [] toshow = (allgrumps) ? AllGrumps : Grumps;
		while((curr+= Time.deltaTime) < spawntime)
		{
			lines_drawing = true;
			for(int i = 0; i < toshow.Length; i++)
			{
				toshow[i].Trace(time);
			}
			yield return null;
		}
	}

	public void Generate(int _index)
	{
		Index = _index;
		bool gender = Index % 2 == 0;
		if(Random.value < 0.05F) gender = !gender;

		Data.Info.Gender = gender;
		Data.Info.Name = gender ? GrandData.Names_Male_Random : GrandData.Names_Female_Random;
		//Add grump based on age if male, remove if female
		//float agefactor = Mathf.Clamp((float)Info.Age/100.0F, 0.0F, 0.2F);
		//Data.Info.GFactor += gender ? agefactor : -agefactor;

		Data.Info.MStat = Random.value > 0.65F ? MaritalStatus.Married : (Random.value > 0.8F ? MaritalStatus.Divorced : MaritalStatus.Donor);
		//Add grump if divorced, remove if married
		switch(Info.MStat)
		{
			case MaritalStatus.Married:
			//Info.GFactor -= 0.15F;
			break;
			case MaritalStatus.Divorced:
			//Info.GFactor += 0.15F;
			break;
			case MaritalStatus.Donor:

			break;
		}

		Data.Info.Military = Random.value > 0.95F;

		Data.Info.PupilScale = Vector3.one;

		transform.name = Info.Name;
		//Emotion.color = GameManager.Data.GG_Colours[Index];
	}

	public void SetMaritalStatus(MaritalStatus m)
	{
		Data.Info.MStat = m;
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

	/*[System.Serializable]
	public class GreatGrand_Data
	{
		public int Hex;
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

		public FaceInfo  EyeLeft, EyeRight,
							EarLeft, EarRight,
							BrowLeft, BrowRight,
							Base, Hair, Jaw, Nose;
		public Vector3 PupilScale;

		public Color Color_Skin, Color_Hair, Color_Offset;

		public FaceInfo GetFaceInfo(string s)
		{
			switch(s)
			{
				case "Eye": return EyeLeft;
				case "Ear": return EarLeft;
				case "Brow": return BrowLeft;
				case "Base": return Base;
				case "Hair": return Hair;
				case "Jaw": return Jaw;
				case "Nose": return Nose;
			}
			return null;
		}

	}*/
