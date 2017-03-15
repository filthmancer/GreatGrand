using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Vectrosity;
using Filthworks;

public class UIManager : MonoBehaviour {
	public Canvas Canvas;
	public Canvas LineCanvas;

	public Transform ModuleTarget, ModuleLeft, ModuleRight;
	[HideInInspector]
	public UIObj ModuleCurrent;
	private int ModuleCurrent_num;
	private UIObj [] Modules{
		get{return new UIObj [] {Module_Menu, Module_Dinner};}
	}

	public UIObj Module_Menu, Module_Dinner;

	public UIObj Options;
	//public UIObj WinMenu;
	public UIObj PermUI;
	public UIObj ResUI;
	public UIObj FaceParent;
	public UIObj WorldObjects, QuoteObjects;
	public FOBJ FOBJ_World, FOBJ_UI;
	private Material QuoteMat;
	
	//public FaceObj ActiveFace;

	public ObjectContainer Sprites;
	public ObjectContainer Prefabs;
	public UIAlert UIResObj;

	public void Init()
	{
		int index = 0;
		for(int i = 0; i < Modules.Length; i++)
		{
			Modules[i].Init(index, null);
			index++;
		}
		Sprites.Init();
		Prefabs.Init();

		UIObj._UICamera = Camera.main;
		UIObj._UICanvas = Canvas;

		Options.Init(index++, null);
		PermUI.Init(index++, null);
		ResUI.Init(index++, null);
		FaceParent.Init(index++, null);
		WorldObjects.Init(index++, null);
		QuoteObjects.Init(index++, null);

		QuoteMat = QuoteObjects.Img[0].material;
		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0), 0.35F);

		PermUI["options"].AddAction(UIAction.MouseUp, ()=>
		{
			Options.TweenActive();
		});
		
		PermUI["exit"].AddAction(UIAction.MouseUp, ()=>
		{
			StartCoroutine(GameManager.instance.LoadModule("Menu"));
		});

		Options["resetintros"].AddAction(UIAction.MouseUp, ()=>
		{
			for(int i = 0 ; i < GameManager.instance.AllModules.Length; i++)
			{
				GameManager.instance.AllModules[i].SetIntro(false);
			}
		});


		Options["resetgrands"].AddAction(UIAction.MouseUp, ()=>
		{
			PlayerPrefs.SetInt("FirstTime", 0);
		});
		
		
		CheckResourcesUI();
	}

	public void SetModule(Module m)
	{
		UIObj newobj = m.MUI;
		if(GameManager.Module != null)
		{
			UIObj oldobj = GameManager.Module.MUI;

			if(GameManager.Module == m) return;
			else if(GameManager.Module.Index < m.Index)
			{
				Tweens.SwoopTo(oldobj.transform, ModuleLeft.position).OnComplete(() => oldobj.SetActive(false));
				newobj.transform.position = ModuleRight.position;
			}
			else 
			{
				Tweens.SwoopTo(oldobj.transform, ModuleRight.position).OnComplete(() => oldobj.SetActive(false));
				newobj.transform.position = ModuleLeft.position;
			}
		}
		else
		{
			newobj.transform.position = ModuleRight.position;
		}

		Sequence s = Tweens.SwoopTo(m.MUI.transform, ModuleTarget.position);
	}

	public float ModuleVelocity = 0.0F;
	public IEnumerator LoadModuleUI(Module m, IntVector v)
	{
		UIObj mui = m.MUI;
		Transform start = ModuleRight;
		Transform end = ModuleTarget;

		if(v.x == 1) start = ModuleRight;
		else if(v.x == -1) start = ModuleLeft;
		else start = ModuleRight;

		mui.transform.position = start.position;

		Sequence s = Tweens.SwoopTo(mui.transform, end.position);
		yield return s.WaitForCompletion();
	}

	public IEnumerator UnloadModuleUI(Module m, IntVector v)
	{
		UIObj mui = m.MUI;
		Transform end = ModuleRight;

		if(v.x == 1) end = ModuleRight;
		else if(v.x == -1) end = ModuleLeft;
		else end = ModuleRight;

		Sequence s = Tweens.SwoopTo(mui.transform, end.position);
		yield return s.WaitForCompletion();
	}


	public void Update()
	{
	}

	public void SetGrandUI(GreatGrand g)
	{
		/*UIObj info = GrandUI["info"];
		UIObj face = GrandUI["face"];

		info.Txt[0].text = g.Info.Name;
		info.Txt[1].text = g.Info.Age+"";
		info.Txt[2].text = "";

		if(ActiveFace != null) 
		{

			Destroy(ActiveFace.gameObject);
			face.Child = new UIObj[0];
		}

		if(g.Face != null)
		{
			ActiveFace = g.CloneFace();
			
			face.AddChild(ActiveFace);
	
			ActiveFace.transform.localPosition = Vector3.zero;
			ActiveFace.transform.localScale = Vector3.one * 0.35F;
			
			ActiveFace.transform.localRotation = Quaternion.Euler(0,0,0);
		}*/
	}

	public UIAlert UIAlertObj;
	public UIAlert StringAlert(string s, Vector3 pos, float lifetime = 2.0F, float size = 50.0F, float speed = 1.0F)
	{
		UIAlert final = Instantiate(UIAlertObj);
		WorldObjects.AddChild(final);
		final.ResetRect();
		final.transform.position = pos;

		//final.Txt[0].enabled = true;
		final.Txt[0].text = s;
		final.Txt[0].fontSize = size;
		final.Img[0].enabled = false;
		final.Init(-1, WorldObjects, lifetime, speed);
		return final;
	}

	public UIAlert ImgAlert(Sprite s, Vector3 pos, float lifetime = 2.0F, float size = 50.0F, float speed = 1.0F)
	{
		UIAlert final = Instantiate(UIAlertObj);
		WorldObjects.AddChild(final);
		final.ResetRect();
		final.transform.position = pos;

		//final.Txt[0].enabled = false;
		final.Txt[0].text = ""; 
		final.Img[0].enabled = true;
		final.Img[0].sprite = s;
		final.Init(-1, WorldObjects, lifetime, speed);
		return final;
	}

	public UIObj Quote(string s, string name)
	{
		GameObject o = Prefabs.GetObject("quote") as GameObject;

		UIObj final = Instantiate(Prefabs.GetObject("quote") as GameObject).GetComponent<UIObj>();
		QuoteObjects[0].AddChild(final);
		final.ResetRect();
		final["textbox"].Txt[0].text = s;
		final.Txt[0].text = name;
		
		final.TweenActive(true);
		return final;

	}

	public IEnumerator QuoteRoutine(UIQuote q)
	{
		int quote_num = 0;
		float rate = 1.5F;
		float rate_inc = 0.03F;
		
		GameManager.IgnoreInput = true;
		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0.8F), 0.35F);
		QuoteObjects.Img[0].raycastTarget = true;
		yield return new WaitForSeconds(0.1F);

		UIObj qobj = Quote("", q.Speaker);
		while(quote_num < q.Quote.Length)
		{
			while(Input.GetMouseButton(0)) yield return null;

			rate = 1.0F;
			UIString target = q.Quote[quote_num];
			
			/*for (float i = 0; i < (target.Value.Length+1); i = i + rate)
		     {
		     	if(Input.GetMouseButtonDown(0)) 
		     	{
		     		break;
		     	}
		         qobj["textbox"].Txt[0].text = target.Value.Substring(0, (int)i);
		         //qobj["textbox"].Txt[0].color = target.Colour;
		         qobj["textbox"].Txt[0].fontSize = target.Size;

		         rate += rate_inc;
		         yield return null;
		     }*/

		     qobj["textbox"].Txt[0].text = target.Value;
		     qobj["textbox"].Txt[0].color = Color.white;
		      qobj["textbox"].Txt[0].fontSize = 60;
		     while(Input.GetMouseButton(0)) yield return null;
		     while(!Input.GetMouseButtonDown(0)) yield return null;
		  
		     quote_num++;
		     yield return null;
		}

		qobj.PoolDestroy();
		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0), 0.35F);
		//QuoteMat.DOFloat(0, "_Size", 0.25F);
		QuoteObjects.Img[0].raycastTarget = false;
		GameManager.IgnoreInput = false;

		yield return null;
	}

	public IEnumerator ResourceAlert(Resource r, int num)
	{
		float time_start = 0.2F;
		float time_adding = 0.8F;
		float time_end_pause = 0.3F;
		float time_end = 0.2F;
		float time_total = time_start + time_adding + time_end;
		float time_curr = 0.0F;

		UIObj res = ResUI.Child[(int)r.Index];
		UIAlert alert = Instantiate(UIResObj);
		alert.Init(-1, WorldObjects, time_total);
		WorldObjects.AddChild(alert);
		
		alert.ResetRect();
		alert.transform.position = res.Txt[0].transform.position -
									res.Txt[0].transform.up*0.5F;

		Tweens.Bounce(alert.transform);
		
		int init = r.Current;

		alert.Txt[0].text = "+" + num;
		alert.Svg[0].color = r.Col;

		yield return new WaitForSeconds(time_start);

		Tweens.Bounce(res.transform);
		/*float amt_soft = num / (time_adding/Time.deltaTime);
		while((time_curr += Time.deltaTime) <= time_adding)
		{
			init += (int) amt_soft;
			
			res.Txt[0].text = r.ToString();
			
			yield return null;
		}*/
		r.Add(num);
		res.Txt[0].text = r.ToString();

		yield return new WaitForSeconds(time_end_pause);
		if(alert != null) alert.PoolDestroy();
		yield return new WaitForSeconds(time_end);
		
		yield return null;
	}

	public FIRL MeterAlert(GrandData g, AlertType t)
	{
		Face f = g.TargetFace;
		FIRL alert = Instantiate(Prefabs.GetObject("fobj_meter") as GameObject).GetComponent<FIRL>();
		alert.Init(0, null);
		alert.transform.position = f.pos + f.T.up;

		Resource res = null;
		string title = "";
		switch(t)
		{
			case AlertType.Hunger:
			res = g.Hunger;
			title = "HUNGER";
			break;
			case AlertType.Fitness:
			res = g.Fitness;
			title = "Fitness";
			break;
			case AlertType.Social:
			res = g.Social;
			title = "Social";
			break;
		}

		alert[0].transform.localScale = new Vector3(res.Ratio, 1.0F, 1.0F);
		alert.Text[0].text = title;
		alert.TweenActive(true);
		return alert;
	}


	public FIRL RewardAlert(RewardCon r)
	{
		FIRL alert = Instantiate(Prefabs.GetObject("govletter") as GameObject).GetComponent<FIRL>();
		alert.SetParent(GameManager.Data.WorldObjects);
		alert.Init(-1, GameManager.Data.WorldObjects);
	
		alert.Text[0].text = r.Title;
		alert.Text[1].text = "";
		alert.Text[2].text = r.Description;

		if(r.Rep > 0)
		{
			alert.AddAction(TouchAction.Destroy, () =>
				{
					StartCoroutine(ResourceAlert(GameManager.WorldRes.Rep, r.Rep));});
			alert.Text[1].text += "+" + r.Rep + " REP\n";
		} 
		if(r.Funds > 0)
		{
			alert.AddAction(TouchAction.Destroy, () =>
				{
					StartCoroutine(ResourceAlert(GameManager.WorldRes.Funds, r.Funds));});
			alert.Text[1].text += "+" + r.Funds + " FUNDS\n";
		} 
		if(r.Meds > 0)
		{
			alert.AddAction(TouchAction.Destroy, () =>
				{
					StartCoroutine(ResourceAlert(GameManager.WorldRes.Meds, r.Meds));});
			alert.Text[1].text += "+" + r.Meds + " MEDS\n";
		} 

		return alert;
	}

	public UIObj FrameObj;

	public FIRL ShowGrandAlert(GrandAlert g, int num = 0)
	{	
		FIRL alert = Instantiate(Prefabs.GetObject("grandletter") as GameObject).GetComponent<FIRL>();
		alert.SetParent(GameManager.Data.WorldObjects);
		alert.Init(-1, GameManager.Data.WorldObjects);
	
		alert.Text[0].text = "";
		alert.Text[1].text = "";

		string title = "";
		string desc = "";
		string pref = "A Grand is";
		//g.Count > 1 ? g.Count + " Grands are" : 
		switch(g.Type)
		{
			case AlertType.Hunger:
			title = pref + " Hungry!";
			desc = "Hungry Grands Get Grumpy!";
			break;
			case AlertType.Fitness:
			title = pref + " Out of Shape!";
			desc = "Low Fitness Grands Get Grumpy!";
			break;
			case AlertType.Social:
			title = pref + " Anti-Social!";
			desc = "Anti-Social Grands Get Grumpy!";
			break;
			case AlertType.Ageup:
			title = "A Grand had a Birthday!";
			desc = "";
			break;
		}

		alert.Text[0].text = title;
		alert.Text[1].text = desc;
		FIRL frame = Instantiate(GameManager.Data.RandomFrame()) as FIRL;
		alert.Child[0].AddChild(frame);
		Face f = GameManager.Generator.GenerateNewFace(g.Grand);
		frame[0].AddChild(f);
		frame.Text[0].text  = g.Grand.Info.Name;
		string amt = "";
		switch(g.Type)
		{
			case AlertType.Hunger:
			amt = (g.Grand.Hunger.Ratio * 100).ToString("0") + "%";
			break;
			case AlertType.Fitness:
			amt = (g.Grand.Fitness.Ratio * 100).ToString("0") + "%";
			break;
			case AlertType.Social:
			amt = (g.Grand.Social.Ratio * 100).ToString("0") + "%";
			break;
			case AlertType.Ageup:
			amt = g.Grand.Age + "";
			break;
		}
		frame.T.position = alert[0].pos;

		alert.T.rotation = Quaternion.Euler(0,0,Random.Range(-3, 3));
		alert.TweenActive(true);	
		return alert;
	}

	public IEnumerator GoThroughAlerts(List<FIRL> alerts)
	{
		GameManager.Paused = true;
		Vector3 worldoffset = GameManager.Data.WorldObjects.pos;// + new Vector3(0,0,-13.5F); 

		for(int i = 0; i < alerts.Count; i++)
		{
			alerts[i].T.position = worldoffset + Vector3.forward * ((float) (i)*2);
			//alerts[i].T.localScale = Vector3.one + Vector3.one * (0.1F * i);
		}

		for(int i = 0; i < alerts.Count; i++)
		{
			FIRL alert = alerts[i];

			bool isAlive = true;
			Vector3 finscroll = Vector3.zero;
			while(isAlive)
			{
				Vector2 scroll = Input.GetMouseButton(0) ? GameManager._Input.GetScroll()/25 : Vector2.zero;
				scroll.y = 0;
				if(scroll == Vector2.zero) 
				{
					alert.T.position = (Vector3.Lerp(alert.pos,worldoffset, Time.deltaTime *15));
					//alert.T.localScale = Vector3.Lerp(alert.T.localScale, Vector3.one, Time.deltaTime*15);
				}				
				else if(Vector3.Distance(alert.pos,worldoffset) > 7)
				{
					finscroll = new Vector3(scroll.x, 0.0F, 0.0F);
					isAlive = false;
				}
				else alert.T.position = (alert.pos + new Vector3(scroll.x, 0.0F, 0.0F));

				yield return null;
			}

			finscroll.Normalize();
			float throwtime = 0.2F;
			while((throwtime -= Time.deltaTime) > 0.0F)
			{
				alert.T.position = alert.pos + finscroll + Vector3.down;
				for(int x = i+1; x < alerts.Count; x++)
				{
					alerts[x].T.position = Vector3.Lerp(alerts[x].pos, worldoffset + Vector3.forward * (x-i-1)*2, 0.5F);
				}
				yield return null;
			}
			alert.PoolDestroy();

			yield return null;
		}
		GameManager.Paused = false;
	}



	public UIObj GrandInfo(GrandData g)
	{
		UIObj final = (Prefabs.GetObject("grandinfo") as GameObject).GetComponent<UIObj>();
		final.Init(-1, WorldObjects);
		WorldObjects.AddChild(final);
		final.ResetRect();
		final.transform.localPosition = Vector3.zero;

		SetGrandInfoObj(final, g);
		return final;
	}

	public void SetGrandInfoObj(UIObj final, GrandData g)
	{
		final[0].Txt[0].text = g.Info.Name;
		final[0].Txt[1].text = "Age " + g.Age.Value;
		final[0].Txt[2].text = g.RoleType + "";

		final[1][0].Txt[0].text = "Hungry ";
		final[1][0].Svg[0].transform.localScale = Vector3.Lerp(
			final[1][0].Svg[0].transform.localScale,
			new Vector3(g.Hunger.Ratio, 1, 1), Time.deltaTime * 10);
		final[1][0].Svg[0].color = Color.Lerp(Color.red, Color.green, g.Hunger.Ratio);

		final[1][1].Txt[0].text = "Fitness ";
		final[1][1].Svg[0].transform.localScale = Vector3.Lerp(
			final[1][1].Svg[0].transform.localScale,
			new Vector3(g.Fitness.Ratio, 1, 1), Time.deltaTime * 10);
		final[1][1].Svg[0].color = Color.Lerp(Color.red, Color.green, g.Fitness.Ratio);

		final[1][2].Txt[0].text = "Social ";
		final[1][2].Svg[0].transform.localScale = Vector3.Lerp(
			final[1][2].Svg[0].transform.localScale,
			new Vector3(g.Social.Ratio, 1, 1), Time.deltaTime * 10);
		final[1][2].Svg[0].color = Color.Lerp(Color.red, Color.green, g.Social.Ratio);

	}

	public void CheckResourcesUI()
	{
		WorldResources wres = GameManager.WorldRes;
		for(int i = 0; i < wres.Length; i++)
		{
			if(wres[i] == null) continue;
			ResUI[i].Txt[0].text = wres[i].ToString();
			ResUI[i].Svg[0].color = wres[i].Col;
		}
	}
}

[System.Serializable]
public class ObjectContainer
{
	public OCon [] Objects;

	public void Init()
	{
		for(int i =0 ; i < Objects.Length; i++)
		{
			Objects[i].Parent = new GameObject(Objects[i].Name).transform;
			Objects[i].Parent.SetParent(GameManager.UI.transform);
			Objects[i].Init(0);
		}
	}
	public Object GetObject(string s)
	{
		s = s.ToLower();
		for(int i = 0; i < Objects.Length; i++)
		{
			if(Objects[i].Name == s) return Objects[i].Spawn();
		}
		return null;
	}

	[System.Serializable]
	public class OCon : ObjectPooler
	{
		
		public OCon(GameObject u, int num, Transform _parent):base(u, num, _parent)
		{
			_Obj = u;
			Parent = _parent;
			Available = new Stack<GameObject>();
			All = new ArrayList(num);
			for(int i = 0; i < num; i++)
			{
				GameObject objtemp = InstantiateObj();
				Available.Push(objtemp);
				All.Add(objtemp);
				objtemp.SetActive(false);
			}
		}
	}
}

public class Tweens
{
	public static Sequence Bounce(Transform t, Vector3? sc = null)
	{
		Vector3 fin = Vector3.one;
		if(sc.HasValue) fin = sc.Value;

		Sequence s = DOTween.Sequence();
		s.Append(t.DOScale(fin * 1.08F, 0.2F));
		s.Append(t.DOScale(fin * 0.9F, 0.08F));
		s.Append(t.DOScale(fin, 0.1F));
		return s;
	}

	public static Sequence SwoopTo(Transform t, Vector3 target)
	{
		Vector3 vel = target - t.position;
		vel.y = 0;
		vel.Normalize();

		Sequence s = DOTween.Sequence();
		s.Append(t.DOMove(t.position - vel * 7.0F, 0.2F));
		s.Append(t.DOMove(target + vel * 3.0F, 0.2F));
		s.Append(t.DOMove(target + vel * 5.0F, 0.2F));
		s.Append(t.DOMove(target, 0.1F));
		return s;
	}

	public static Sequence PictureSway(Transform t, Vector3 rot)
	{
		Vector3 init = t.localRotation.eulerAngles;

		Sequence s = DOTween.Sequence();
		s.Append(t.DOLocalRotate(init + -rot, 0.2F));
		s.Append(t.DOLocalRotate(init + rot*0.6F, 0.2F));
		s.Append(t.DOLocalRotate(init + -rot*0.3F, 0.2F));
		s.Append(t.DOLocalRotate(init, 0.2F));

		return s;
	}

	public static Sequence SetToState(UIObj u, UIState s, float time = 0.15F)
	{

		Sequence seq = DOTween.Sequence();
		if(!u.isActive)return seq;
		//seq.Append(u.transform.DOLocalMove(s.Position, time))
		   seq.Append(u.transform.DOScale(s.Scale, time));
		if(u.Img.Length > 0) 
			seq.Append(DOTween.To(() => u.Img[0].color, x => u.Img[0].color = x, s.Col, time));
		else if (u.Svg.Length > 0) 
			seq.Append(DOTween.To(() => u.Svg[0].color, x => u.Img[0].color = x, s.Col, time));
		return seq;
	}

}

public class UIQuote
{
	public string Speaker;
	public UIString [] Quote;
	public UIQuote(string sp, params string [] t)
	{
		Speaker = sp;
		Quote = new UIString[t.Length];
		for(int i = 0; i < t.Length; i++)
		{
			Quote[i] = new UIString(t[i]);
		}
	}
}

public class UIString
{
	public string Value;
	public float Size = 20;
	public Color Colour = Color.white;
	public bool NewLine = false;
	public UIString(string v, float s = 20, Color? c = null, bool nline = false)
	{
		Value = v;
		Size = s;
		Colour = c ?? Color.white;
		NewLine = nline;
	}
}
