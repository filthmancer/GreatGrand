﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;

public class UIObj : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,IPointerEnterHandler, IPointerExitHandler{
	public string _Name;
	[HideInInspector]
	public int Index = 100;
	//[HideInInspector]
	public UIObj ParentObj;
	public Image [] Img;
	public SVGImage [] Svg;
	public TextMeshProUGUI [] Txt;
	public UIObj [] Child;


	public bool SetInactiveAfterLoading;
	public TextMeshProUGUI _Text
	{get{
			if(Txt.Length > 0) return Txt[0];
			else return null;
	}}
	public Image _Image
	{get{
			if(Img.Length > 0) return Img[0];
			else 
			{
				if(GetComponent<Image>()) 
				{
					Img = new Image[]{GetComponent<Image>()};
					return Img[0];
				}
				else return null;
			}
	}}

	protected ObjectPoolerReference poolref;
	public ObjectPoolerReference GetPoolRef(){return poolref;}

	public bool RaycastTarget = true;

	public void PoolDestroy()
	{
		if(ParentObj != null) ParentObj.RemoveChild(this);
		if(poolref)
		{
			poolref.Unspawn();
		}
		else Destroy(this.gameObject);
	}

	public virtual void Start()
	{
		if(_Name == string.Empty) _Name = gameObject.name;
		else gameObject.name = _Name;
		
		if(Img.Length == 0 && GetComponent<Image>()) Img = new Image[]{GetComponent<Image>()};
		if(Txt.Length == 0 && GetComponent<TextMeshProUGUI>()) Txt = new TextMeshProUGUI[]{GetComponent<TextMeshProUGUI>()};
		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i] == null) continue;
			Child[i].Index = i;
			Child[i].ParentObj = this;
		}
		activescale = transform.localScale;
		if(SetInactiveAfterLoading) SetActive(false);
		else isActive = this.gameObject.activeSelf;
	}

	public virtual void Setup(params float [] args)
	{

	}


	public virtual void SetActive(bool? active = null)
	{
		bool actual = active ?? !this.gameObject.activeSelf;
		isActive = actual;
		this.gameObject.SetActive(actual);
	}

	private Vector3 activescale = Vector3.one;
	public virtual void TweenActive(bool ? active = null)
	{
		bool actual = active ?? !this.gameObject.activeSelf;
		isActive = actual;

		if(actual)
		{
			this.gameObject.SetActive(true);
			if(this.transform.localScale != Vector3.zero) activescale = this.transform.localScale;
			else activescale = Vector3.one;

			this.transform.localScale = Vector3.zero;
			Sequence s = Tweens.Bounce(this.transform, activescale);
			for(int i = 0; i < Txt.Length; i++)
			{
				Txt[i].text = Txt[i].text;
			}
		}
		else
		{
			if(this.transform.localScale != Vector3.zero) activescale = this.transform.localScale;
			this.transform.DOScale(Vector3.zero, 0.25F).OnComplete(() =>{this.gameObject.SetActive(false);});
		}
	}

	public bool isActive;

	public UIObj this[int i]
	{
		get
		{
			foreach(UIObj obj in Child)
			{
				if(obj && obj.Index == i) return obj;
			}
			if(Child.Length > i) return Child[i];
			return null;
		}
	}

	public UIObj this[string s]
	{
		get
		{
			foreach(UIObj child in Child)
			{
				if(child == null || child._Name == string.Empty) continue;
				if(child._Name == s) return child;
				if(child[s] != null) return child[s];
			}
			return null;
		}
	}

	public int Length
	{
		get{return Child.Length;}
	}

	public UIObj GetIndex(int i)
	{
		foreach(UIObj child in Child)
		{
			if(child.Index == i) return child;
		}
		return null;
	}

	public virtual void SetParent(UIObj p)
	{
		ParentObj = p;
		transform.SetParent(p.transform);
		p.AddChild(this);
	}

	public UIObj GetChild(int i)
	{
		if(Child.Length-1 < i) return null;
		return Child[i];
	}

	public void AddChild(params UIObj [] c)
	{
		UIObj [] newchild = new UIObj[Child.Length+c.Length];
		for(int i = 0; i < Child.Length; i++)
		{
			newchild[i] = Child[i];
		}
		int x = 0;
		for(int i = Child.Length; i < Child.Length + c.Length; i++)
		{
			newchild[i] = c[x];
			newchild[i].Index = i;
			newchild[i].ParentObj = this;
			newchild[i].transform.SetParent(this.transform, false);
			newchild[i].transform.localRotation = Quaternion.identity;
			newchild[i].transform.localPosition = Vector3.zero;
			//newchild[i].GetComponent<RectTransform>().anchorMax = Vector2.zero;
			//newchild[i].GetComponent<RectTransform>().anchorMax = Vector2.one;
			//newchild[i].GetComponent<RectTransform>().sizeDelta = Vector3.zero;
			newchild[i].transform.localScale = Vector3.one;
			x++;
		}
		Child = newchild;
	}

	public void RemoveChild(UIObj c)
	{
		bool remove = false;
		int index = 0;
		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i] == c)
			{
				remove = true;
				index = i;
				break;
			}
		}
		if(!remove) return;
		UIObj [] newchild = new UIObj[Child.Length-1];
		int a =0;
		for(int i = 0; i < index; i++)
		{
			newchild[a] = Child[i];
			a++;
		}
		for(int x = index+1; x < Child.Length; x++)
		{
			newchild[a] = Child[x];
			a++;
		}
		Child = newchild;
	}

	private RectTransform _rect;
	public RectTransform RectT{
		get{
			if(_rect == null) _rect = this.GetComponent<RectTransform>();
			return _rect;
		}
	}
	public void ResetRect()
	{

		//RectT.anchorMax = Vector2.zero;
		//RectT.anchorMax = Vector2.one;
		RectT.sizeDelta = Vector3.zero;
		RectT.anchoredPosition = Vector3.zero;

		//transform.localPosition = Vector3.zero;

		transform.localRotation = Quaternion.Euler(0,0,0);
		
		transform.localScale = Vector3.one;
	}

	public void Destroy()
	{
		Destroy(this.gameObject);
	}
	public void DestroyChildren()
	{
		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i].GetPoolRef()) Child[i].GetPoolRef().Unspawn();
			else Destroy(Child[i].gameObject);
		}
		Child = new UIObj[0];
	}

	public void SetColor(int num, Color c)
	{
		if(Img.Length <= num) return;
		Img[num].color = c;
	}

	public void BooleanObjColor(bool good)
	{
		if(Img.Length == 0) return;
		Color col = good ? Color.green : Color.red;
		Img[0].color = col;
		init = col;
	}

	public virtual void LateUpdate()
	{
		if(isPressed)
		{
			time_over += Time.deltaTime;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{

	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput) return;
		foreach(Action child in Actions_MouseOver)
		{
			child();
		}
		foreach(UIAction_Method child in TypeActions_MouseOver)
		{
			child.Act();
		}
		if(Img.Length > 0) init = Img[0].color;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput) return;
		foreach(Action child in Actions_MouseOut)
		{
			child();
		}
		foreach(UIAction_Method child in TypeActions_MouseOut)
		{
			child.Act();
		}
		isPressed = false;
		time_over = 0.0F;
		if(Img.Length > 0) Img[0].color = init;
	}


	public bool PlayClickDown = true, PlayClickUp = true;
	public void OnPointerDown(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput) return;
		//if(Application.isMobilePlatform) return;
		//if(UIManager.instance.LogUIObjs) print(Actions_MouseDown.Count + ":" +  this);

		if((Actions_MouseUp.Count > 0 || TypeActions_MouseUp.Count > 0) || PlayClickDown)
		{
		//	AudioManager.instance.PlayClipOn(this.transform, "UI", "ClickDown");
		}

		foreach(Action child in Actions_MouseDown)
		{
			child();
		}
		foreach(UIAction_Method child in TypeActions_MouseDown)
		{
			child.Act();
		}
		isPressed = true;
		time_over += Time.deltaTime;
		if(Img.Length > 0) 
		{
			init = Img[0].color;
			Img[0].color = Color.Lerp(init, Color.black, 0.2F);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput) return;
		foreach(UIAction_Method child in TypeActions_MouseUp)
		{
			child.Act();
		}
		if((Actions_MouseUp.Count > 0 || TypeActions_MouseUp.Count > 0) || PlayClickUp)
		{
		//	AudioManager.instance.PlayClipOn(this.transform, "UI", "ClickUp");
		}
		//if(Application.isMobilePlatform) return;
		foreach(Action child in Actions_MouseUp)
		{
			child();
		}
		isPressed = false;
		time_over = 0.0F;
		if(Img.Length > 0) Img[0].color = init;	
	}
	List<Action>	Actions_MouseOut = new List<Action>(), 
					Actions_MouseOver = new List<Action>(),
					Actions_MouseUp = new List<Action>(),
					Actions_MouseDown = new List<Action>(),
					Actions_MouseClick = new List<Action>();
	List<UIAction_Method>	TypeActions_MouseOut = new List<UIAction_Method>(), 
						TypeActions_MouseOver = new List<UIAction_Method>(),
						TypeActions_MouseUp = new List<UIAction_Method>(),
						TypeActions_MouseDown = new List<UIAction_Method>(),
						TypeActions_MouseClick = new List<UIAction_Method>();

	protected Color init;
	public void SetInitCol(Color c) {init = c;}
	protected float time_over = 0.0F;
	public bool isPressed;

	public void AddAction(UIAction a, Action func)
	{
		switch(a)
		{
			case UIAction.MouseOut:
			Actions_MouseOut.Add(func);
			break;
			case UIAction.MouseOver:
			Actions_MouseOver.Add(func);
			break;
			case UIAction.MouseUp:
			Actions_MouseUp.Add(func);

			break;
			case UIAction.MouseDown:
			Actions_MouseDown.Add(func);
			break;
		}
	}

	public void AddAction(UIAction a, Action<string[]> func, params string [] t)
	{

		switch(a)
		{
			case UIAction.MouseOut:
			TypeActions_MouseOut.Add(new UIAction_Method(func, t));
			break;
			case UIAction.MouseOver:
			TypeActions_MouseOver.Add(new UIAction_Method(func, t));
			break;
			case UIAction.MouseUp:
			TypeActions_MouseUp.Add(new UIAction_Method(func, t));
			break;
			case UIAction.MouseDown:
			TypeActions_MouseDown.Add(new UIAction_Method(func, t));
			break;
		}
	}

	public void ClearChildActions(UIAction a = UIAction.None)
	{
		for(int i = 0; i < Child.Length; i++)
		{
			Child[i].ClearActions(a);
		}
	}

	public virtual void ClearActions(UIAction a = UIAction.None)
	{
		if(a == UIAction.None)
		{
			Actions_MouseOut.Clear();
			Actions_MouseOver.Clear();
			Actions_MouseUp.Clear();
			Actions_MouseDown.Clear();
			TypeActions_MouseOut.Clear();
			TypeActions_MouseOver.Clear();
			TypeActions_MouseUp.Clear();
			TypeActions_MouseDown.Clear();
		}
		switch(a)
		{
			case UIAction.MouseOut:
			Actions_MouseOut.Clear();
			TypeActions_MouseOut.Clear();
			break;
			case UIAction.MouseOver:
			Actions_MouseOver.Clear();
			TypeActions_MouseOver.Clear();
			break;
			case UIAction.MouseUp:
			Actions_MouseUp.Clear();
			TypeActions_MouseUp.Clear();
			break;
			case UIAction.MouseDown:
			Actions_MouseDown.Clear();
			TypeActions_MouseDown.Clear();
			break;
			case UIAction.MouseClick:
			Actions_MouseClick.Clear();
			TypeActions_MouseClick.Clear();
			break;
		}
	}
}

public enum UIAction
{
	MouseOut,
	MouseOver,
	MouseUp,
	MouseDown,
	MouseClick,
	None
}

public class UIAction_Method
{
	public string [] Values;
	public Action<string[]> Method;
	public void Act()
	{
		Method(Values);
	}

	public UIAction_Method(Action<string[]> m, params string [] v)
	{
		Values = v;
		Method = m;
	}
}