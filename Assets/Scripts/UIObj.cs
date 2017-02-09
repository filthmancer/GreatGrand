using UnityEngine;
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
	[HideInInspector]
	public UIObj ParentObj;

	private static bool LogUIObjs = false;

	public Image [] Img;
	public SVGImage [] Svg;
	public TextMeshProUGUI [] Txt;
	public UIObj [] Child;

	public UIState StateWhenPressed = new UIState(Vector2.zero, Vector3.one, Color.white);
	
// REMEMBER THAT THIS STATE SHIT IS SET UP FOR MY STUPID ASS IDEA TO MAKE GG A GAME ON
// X AND Z AXIS INSTEAD OF X AND Y YOU DUMB FUCK
	private UIState State_init;

	public bool SetInactiveAfterLoading;
	public bool RaycastTarget = false;
	public bool isActive;

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

	public SVGImage _SVG
	{
		get{
			if(Svg.Length > 0 && Svg[0] != null) return Svg[0];
			else 
			{
				if(GetComponent<SVGImage>()) 
				{
					Svg = new SVGImage[]{GetComponent<SVGImage>()};
					return Svg[0];
				}
				else return null;
			}
		}
	}


	protected ObjectPoolerReference poolref;
	public ObjectPoolerReference GetPoolRef(){return poolref;}

	

	public void PoolDestroy()
	{
		if(ParentObj != null) ParentObj.RemoveChild(this);
		if(poolref)
		{
			poolref.Unspawn();
		}
		else Destroy(this.gameObject);
	}

	public virtual void Init(int ind, UIObj p, params float [] args)
	{
		if(ind != -1) Index = ind;
		ParentObj = p;

		if(_Name == string.Empty) _Name = gameObject.name;
		else gameObject.name = _Name;
		
		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i] == null) continue;
			Child[i].Init(i, this);
		}

		if(Img.Length == 0 && GetComponent<Image>()) Img = new Image[]{GetComponent<Image>()};
		if(Svg.Length == 0 && GetComponent<SVGImage>()) Svg = new SVGImage[]{GetComponent<SVGImage>()};
		if(Txt.Length == 0 && GetComponent<TextMeshProUGUI>()) Txt = new TextMeshProUGUI[]{GetComponent<TextMeshProUGUI>()};
		
		State_init = new UIState(this);
		if(_Image != null) StateWhenPressed.Col = _Image.color;
		else if(_SVG) StateWhenPressed.Col = _SVG.color;
	
		if(SetInactiveAfterLoading) SetActive(false);
		else isActive = this.gameObject.activeSelf;
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
		if(actual == this.gameObject.activeSelf) return;

		isActive = actual;
		if(actual)
		{
			this.gameObject.SetActive(true);
			if(this.transform.localScale != Vector3.zero) activescale = this.transform.localScale;
			else activescale = Vector3.one;

			this.transform.localScale = Vector3.zero;

			Sequence s = Tweens.Bounce(this.transform, activescale).OnComplete(() => {activescale = this.transform.localScale;});
			for(int i = 0; i < Txt.Length; i++)
			{
				Txt[i].text = Txt[i].text;
			}
		}
		else
		{
			this.transform.DOScale(Vector3.zero, 0.25F).OnComplete(() =>{this.gameObject.SetActive(false);});
		}
	}

	public void SetUIPositionFromWorld(Vector3 wpos)
	{
		Vector2 ViewportPosition= _UICamera.WorldToViewportPoint(wpos);

		Vector2 WorldObject_ScreenPosition=new Vector2(
		((ViewportPosition.x*_UICanvasRect.sizeDelta.x)-(_UICanvasRect.sizeDelta.x*0.5f)),
		((ViewportPosition.y*_UICanvasRect.sizeDelta.y)-(_UICanvasRect.sizeDelta.y*0.5f)));
		
		//now you can set the position of the ui element
		RectT.anchoredPosition=WorldObject_ScreenPosition;
	}

	public void SetUIPosition(Vector2 spos){
		transform.position = spos;
	}

	public void FitUIPosition(Vector2 spos, Rect? f = null, float ratio = 0.5F)
	{
		Rect r = RectT.rect;
		if(f.HasValue) r = f.Value;
		spos.x = Mathf.Clamp(spos.x, r.width*ratio, Screen.width - r.width*ratio);
		spos.y = Mathf.Clamp(spos.y, r.height*ratio, Screen.height - r.height*ratio);
		transform.position = spos;
	}

	public Vector2 GetUIPosition(){return RectT.position;}


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
			newchild[i].Init(i, this);
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
		RectT.sizeDelta = Vector3.zero;
		RectT.anchoredPosition = Vector3.zero;
		transform.localRotation = Quaternion.Euler(0,0,0);
		transform.localScale = Vector3.one;
	}

	public void Destroy()
	{
		PoolDestroy();
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
		if(GameManager.IgnoreInput || !RaycastTarget) return;
		if(LogUIObjs) print(this + ": Mouse Enter - " + Actions_MouseOver.Count + " actions");
		foreach(Action child in Actions_MouseOver)
		{
			child();
		}
		foreach(UIAction_Method child in TypeActions_MouseOver)
		{
			child.Act();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput || !RaycastTarget) return;
		if(LogUIObjs) print(this + ": Mouse Exit - " + Actions_MouseOut.Count + " actions");

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
		Tweens.SetToState(this, State_init, 0.1F);
	}


	public bool PlayClickDown = true, PlayClickUp = true;
	public void OnPointerDown(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput || !RaycastTarget) return;
		if(LogUIObjs) print(this + ": Mouse Down - " + Actions_MouseDown.Count + " actions");

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
		Tweens.SetToState(this, StateWhenPressed);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if(GameManager.IgnoreInput || !RaycastTarget) return;
		if(LogUIObjs) print(this + ": Mouse Up - " + Actions_MouseUp.Count + " actions");
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
		Tweens.SetToState(this, State_init, 0.1F);
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

	protected Color col_init;
	protected Color col_when_pressed;
	public void SetInitCol(Color c) {col_init = c;}
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


	public static Canvas _UICanvas;
	public static Camera _UICamera;
	private static RectTransform _UICanvas_rect;
	public static RectTransform _UICanvasRect
	{
		get{
			if(_UICanvas_rect == null) _UICanvas_rect = _UICanvas.GetComponent<RectTransform>();
			return _UICanvas_rect;}
	}
	private static CanvasScaler _UICanvas_scaler;
	public static CanvasScaler _UICanvasScaler
	{
		get{
			if(_UICanvas_scaler == null) _UICanvas_scaler = _UICanvas.GetComponent<CanvasScaler>();
			return _UICanvas_scaler;}
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

[System.Serializable]
public class UIState
{
	public Vector2 Position;
	public Vector3 Scale;
	public Color Col;

	public UIState(UIObj u)
	{
		Position = new Vector2(u.transform.localPosition.x, u.transform.localPosition.z);
		Scale = u.transform.localScale;

		Color c = Color.white;
		if(u.Img.Length > 0 && u.Img[0] != null) c = u.Img[0].color;
		else if (u.Svg.Length > 0 && u.Svg[0] != null) c = u.Svg[0].color;
	
		Col = c;
	}

	public UIState(Vector2 p, Vector3 s, Color c)
	{
		Position = p;
		Scale = s;
		Col = c;
	}
}