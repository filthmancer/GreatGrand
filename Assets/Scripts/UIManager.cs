using UnityEngine;
using System.Collections;
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

	public UIObj GrandAlertObj;
	public UIObj Options;
	public UIObj WinMenu;
	public UIObj PermUI;
	public UIObj ResUI;
	public UIObj FaceParent;
	public UIObj WorldObjects, QuoteObjects;

	private Material QuoteMat;
	
	public FaceObj ActiveFace;

	public ObjectContainer Sprites;
	public ObjectContainer Prefabs;
	public UIAlert UIResObj;

	public void Init()
	{
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



	public void ShowEndGame()
	{
		WinMenu[0].TweenActive(true);
		WinMenu[0].ClearActions();
		WinMenu[0].AddAction(UIAction.MouseUp, () => 
		{
			GameManager.Module.Clear();
			StartCoroutine(GameManager.Module.Enter(false, new IntVector(0,0)));
		});
	}

	public void Update()
	{
		if(ActiveFace)
		{
			ActiveFace.CheckAnims();
		}
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
		final.Setup(lifetime, speed);
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
		final.Setup(lifetime, speed);
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
		      qobj["textbox"].Txt[0].fontSize = 20;
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
		UIObj res = ResUI.Child[(int)r.Index];
		UIAlert alert = Instantiate(UIResObj);
		WorldObjects.AddChild(alert);
		alert.ResetRect();

		alert.transform.position = res.Txt[0].transform.position -
									res.Txt[0].transform.up*0.5F;

		float time_start = 0.2F;
		float time_adding = 0.8F;
		float time_end_pause = 0.3F;
		float time_end = 0.2F;
		float time_total = time_start + time_adding + time_end;

		float time_curr = 0.0F;

		alert.Setup(time_total);
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

		Tweens.Bounce(res.transform);
		r.Add(num);
		res.Txt[0].text = r.ToString();

		yield return new WaitForSeconds(time_end_pause);
		if(alert != null) alert.PoolDestroy();
		yield return new WaitForSeconds(time_end);
		
		yield return null;
	}

	public IEnumerator ShowGrandAlert(GrandAlert g)
	{
		UIObj alert = PermUI["grandalert"];
		GrandData grand = g.Grand;

		switch(g.Type)
		{
			case AlertType.Ageup:
				alert.Txt[0].text = grand.Info.Name + " Aged Up!";
				alert.Txt[1].text = grand.Info.Age - g.Values[0] + "";
			break;
			case AlertType.Hungry:
				alert.Txt[0].text = grand.Info.Name + " is Hungry!";
				alert.Txt[1].text = "";
			break;
		}

		FaceObj f = GameManager.instance.Generator.GenerateFace(grand.GrandObj);
		alert.Child[0][0].AddChild(f);
		f.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;//alert.Child[0][0].transform.position;
		f.transform.localScale = Vector3.one * 0.15F;

		alert.TweenActive(true);

		switch(g.Type)
		{
			case AlertType.Ageup:
				yield return new WaitForSeconds(0.8F);
				int init = grand.Info.Age - g.Values[0];
				for(int i = 0; i < g.Values[0]; i++)
				{
					init ++;
					alert.Txt[1].text = init + "";
					Tweens.Bounce(alert.Txt[1].transform);
					yield return new WaitForSeconds(0.07F);
				}
				alert.Txt[1].text = grand.Info.Age + "";
				
				yield return new WaitForSeconds(1.0F);
			break;
			case AlertType.Hungry:
				yield return new WaitForSeconds(2.0F);
			break;
		}

		
		alert.TweenActive(false);
		yield return new WaitForSeconds(0.3F);

		alert.Child[0][0].DestroyChildren();
		yield return null;
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
		s.Append(t.DOMove(t.position - vel * 0.3F, 0.2F));
		s.Append(t.DOMove(target + vel * 0.05F, 0.2F));
		s.Append(t.DOMove(target + vel * 0.25F, 0.2F));
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
