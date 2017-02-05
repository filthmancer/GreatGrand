using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Vectrosity;

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

	public UIObj FrameObj;
	public IEnumerator HungerAlert(List<GrandAlert> g)
	{
		UIObj alert = PermUI["grandalert"];
	
		GameManager.IgnoreInput = true;
		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0.8F), 0.35F);
		QuoteObjects.Img[0].raycastTarget = true;
		alert.TweenActive(true);

		for(int i = 0; i < g.Count; i++)
		{
			UIObj frame = (UIObj) Instantiate(FrameObj);
			alert.Child[0].AddChild(frame);
			FaceObj f = GameManager.instance.Generator.GenerateFace(g[i].Grand.GrandObj);
			frame.AddChild(f);
			f.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;//alert.Child[0][0].transform.position;
			f.transform.localScale = Vector3.one * 0.15F;
			Tweens.Bounce(f.Txt[0].transform);
			f.Txt[0].text = g[i].Grand.Hunger.Ratio*100 + "%";
			f.Txt[0].color = Color.red;
			f.Txt[0].fontSize = 130;

			alert.Txt[0].text = g[i].Grand.Info.Name + " is Hungry!";
			yield return new WaitForSeconds(0.45F);		

		}		
		alert.Txt[2].text = "Hunger makes Grands Grumpy!";	
		yield return new WaitForSeconds(1.5F);
		alert.TweenActive(false);
		yield return new WaitForSeconds(0.3F);

		alert.Child[0].DestroyChildren();

		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0), 0.35F);
		//QuoteMat.DOFloat(0, "_Size", 0.25F);
		QuoteObjects.Img[0].raycastTarget = false;
		GameManager.IgnoreInput = false;
		yield return null;
	}

	public IEnumerator AgeAlert(List<GrandAlert> g)
	{
		UIObj alert = PermUI["grandalert"];
	
		GameManager.IgnoreInput = true;
		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0.8F), 0.35F);
		QuoteObjects.Img[0].raycastTarget = true;
		alert.TweenActive(true);	

		for(int i = 0; i < g.Count; i++)
		{
			UIObj frame = (UIObj) Instantiate(FrameObj);
			alert.Child[0].AddChild(frame);
			FaceObj f = GameManager.instance.Generator.GenerateFace(g[i].Grand.GrandObj);
			frame.AddChild(f);
			f.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;//alert.Child[0][0].transform.position;
			f.transform.localScale = Vector3.one * 0.15F;
			f.Txt[0].text = g[i].Grand.Info.Age-g[i].Values[0] + "";

			alert.Txt[0].text = g[i].Grand.Info.Name + " Aged up!";

			yield return new WaitForSeconds(0.45F);		
			Tweens.Bounce(f.Txt[0].transform);
			f.Txt[0].text = g[i].Grand.Info.Age + "";
		}		
		alert.Txt[2].text = "";	
		yield return new WaitForSeconds(1.5F);
		alert.TweenActive(false);
		yield return new WaitForSeconds(0.3F);

		alert.Child[0].DestroyChildren();

		QuoteObjects.Img[0].DOColor(new Color(1,1,1,0), 0.35F);
		//QuoteMat.DOFloat(0, "_Size", 0.25F);
		QuoteObjects.Img[0].raycastTarget = false;
		GameManager.IgnoreInput = false;
		yield return null;
	}

	public UIObj GrandInfo(GreatGrand g)
	{
		UIObj final = Instantiate(Prefabs.GetObject("grandinfo") as GameObject).GetComponent<UIObj>();
		final.Init(-1, WorldObjects);
		WorldObjects.AddChild(final);
		
		final.ResetRect();
		final.transform.localPosition = Vector3.zero;

		final[0].Txt[0].text = g.Info.Name;
		final[0].Txt[1].text = "Age " + g.Info.Age;
		final[0].Txt[2].text = g.Data.RoleType + "";

		final[1][0].Txt[0].text = "Hungry ";
		final[1][0].Svg[0].transform.localScale = new Vector3(g.Data.Hunger.Ratio, 1, 1);
		final[1][0].Svg[0].color = Color.Lerp(Color.green, Color.red, g.Data.Hunger.Ratio);

		final[1][1].Txt[0].text = "Fitness ";
		final[1][1].Svg[0].transform.localScale = new Vector3(g.Data.Fitness.Ratio, 1, 1);
		final[1][1].Svg[0].color = Color.Lerp(Color.green, Color.red, g.Data.Fitness.Ratio);

		final[1][2].Txt[0].text = "Social ";
		final[1][2].Svg[0].transform.localScale = new Vector3(g.Data.Social.Ratio, 1, 1);
		final[1][2].Svg[0].color = Color.Lerp(Color.green, Color.red, g.Data.Social.Ratio);

		return final;
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
	public Object GetObject(string s)
	{
		s = s.ToLower();
		for(int i = 0; i < Objects.Length; i++)
		{
			if(Objects[i].Name == s) return Objects[i].Obj;
		}
		return null;
	}

	[System.Serializable]
	public class OCon
	{
		public string Name;
		public Object Obj;
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
		s.Append(t.DOMove(t.position - vel * 100.0F, 0.2F));
		s.Append(t.DOMove(target + vel * 25.0F, 0.2F));
		s.Append(t.DOMove(target + vel * 50.0F, 0.2F));
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
