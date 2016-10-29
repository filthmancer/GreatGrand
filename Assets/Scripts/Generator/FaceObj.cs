using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FaceObj : UIObj {

	//public RectTransform [] Anchors;

	public FaceObj FaceParent;
	public bool GetSkinColor, GetHairColor;

	private RectTransform _anchorpoint;
	private FaceObj [] _obj;
	private RectTransform trans;

	public List<AnimTrigger> Anims = new List<AnimTrigger>();
	

	public void CheckAnims()
	{
		if(Anims != null)
		{
			for(int i = 0; i < Anims.Count; i++)
			{
				if(Anims[i].Update()) 
				{
					Anims[i].Trigger();
				}
					
			}
		}
		if(Child != null)
		{
			for(int i = 0; i < Child.Length; i++)
			{
				if(Child[i] is FaceObj) (Child[i] as FaceObj).CheckAnims();
			}
		}
	}

	public void CreateAnchor(int num, FaceObj o)
	{
		if(_obj == null || _obj.Length == 0) _obj = new FaceObj[Child.Length];
		_obj[num] = (FaceObj) Instantiate(o);
		_obj[num].SetAnchorPoint(Child[num] as FaceObj);
	}


	public void SetAnchorPoint(FaceObj t)
	{
		SetParent(t);

		_anchorpoint = t.transform as RectTransform;

		if(!trans) trans = this.transform as RectTransform;

		trans.anchorMax = Vector2.one;//_anchorpoint.anchorMax;
		trans.anchorMin = Vector2.zero;//_anchorpoint.anchorMin;
		trans.anchoredPosition = Vector2.zero;//_anchorpoint.anchoredPosition;
		trans.localScale = Vector3.one;
		trans.sizeDelta = Vector2.zero;//_anchorpoint.sizeDelta;
	}


	private Color	_skincol;
	public Color SkinCol
	{get{return _skincol;}}
	public void SetSkinColor(Color c)
	{
		_skincol = c;
	}

	private Color	_haircol;
	public Color HairCol
	{get{return _haircol;}}
	public void SetHairColor(Color c)
	{
		_haircol = c;
	}

	public override void SetParent(UIObj p)
	{
		ParentObj = p;
		transform.SetParent(p.transform);
		p.AddChild(this);

		if(p is FaceObj)
		{
			FaceObj f = p as FaceObj;
			if(f.FaceParent) 
			{
				SetFaceParent(f.FaceParent);
			}
		}
	}

	public void SetFaceParent(FaceObj f)
	{
		FaceParent = f;
		if(GetSkinColor && _Image) _Image.color = FaceParent.SkinCol;
		if(GetHairColor && _Image) _Image.color = FaceParent.HairCol;

		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i] is FaceObj)
			{
				(Child[i] as FaceObj).SetFaceParent(FaceParent);
			}
		}
	}

	public void AddAnimTrigger(string title, Vector2 times, params Animator [] anims)
	{
		AnimTrigger a = new AnimTrigger(title, times, anims);
		Anims.Add(a);
	}

}
	
[System.Serializable]
public class AnimTrigger
{
	public string Title;
	public Vector2 TimeConstants;
	public float TimeActual, Current;
	public Animator [] Anims;

	public AnimTrigger(string t, Vector2 c, params Animator [] a)
	{
		Title = t;
		TimeConstants = c;
		Anims = a;
		Create();

	}
	public void Create()
	{
		TimeActual = Random.Range(TimeConstants.x, TimeConstants.y);
	}

	public bool Update()
	{
		Current += Time.deltaTime;
		if(Current > TimeActual)
		{
			Create();
			Current = 0.0F;
			return true;
		}
		return false;
	}

	public void Trigger()
	{
		for(int i = 0; i < Anims.Length; i++)
		{
			Anims[i].SetTrigger(Title);
		}
	}
}
