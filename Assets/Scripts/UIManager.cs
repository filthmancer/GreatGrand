using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour {
	public Transform Canvas;
	public Canvas LineCanvas;

	public Transform ModuleTarget, ModuleLeft, ModuleRight;
	[HideInInspector]
	public UIObj ModuleCurrent;
	private int ModuleCurrent_num;
	private UIObj [] Modules{
		get{return new UIObj [] {Module_Menu, Module_Dinner};}
	}

	public UIObj Module_Menu, Module_Dinner;

	public UIObj Menu;
	public UIObj DinnerUI;
	public UIObj WinMenu;
	public UIObj GrandUI, ResUI;
	public UIObj FaceParent;
	public UIObj WorldObjects;
	
	public FaceObj ActiveFace;
	public Sprite Angry, Bored, Happy;

	public ObjectContainer Sprites;
	public UIAlert UIResObj;

	public void Init()
	{
		Module_Menu.Index = 0;
		Module_Dinner.Index = 1;
		
	//MENU
		Menu.SetActive(true);
		for(int i = 0; i < Menu[1].Child.Length; i++)
		{
			GreatGrand f = GameManager.instance.GGGen.Generate(i);
			FaceObj face = GameManager.instance.GGGen.GenerateFace(f);
			Menu[1].Child[i][0].AddChild(face);
			face.transform.localRotation = Quaternion.identity;
			face.transform.position = Menu[1].Child[i][0].transform.position;
			face.transform.localScale = Vector3.one * 0.65F;
		}

		Menu[0].ClearActions();
		Menu[0].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Dinner"));
		});

	//DINNER
		DinnerUI[1].AddAction(UIAction.MouseDown, () =>
		{
			for(int i = 0; i < GameManager.instance.GG.Length; i++)
			{
				GameManager.instance.GG[i].ShowGrumpLines(1.5F);
			}
		});
		DinnerUI[2].AddAction(UIAction.MouseDown, ()=>
		{
			GameManager.instance.CompleteDinner();
		});
		DinnerUI[3].AddAction(UIAction.MouseDown, ()=>
		{
			GameManager.instance.ExitMinigame();
		});
		DinnerUI.SetActive(false);
		
		CheckResourcesUI();
	}

	public void SetModule(UIObj m)
	{
		m.SetActive(true);
		if(ModuleCurrent != null)
		{
			UIObj temp = ModuleCurrent;
			if(ModuleCurrent == m) return;
			else if(ModuleCurrent.Index < m.Index)
			{
				Tweens.SwoopTo(temp.transform, ModuleLeft.position).OnComplete(() => temp.SetActive(false));
				m.transform.position = ModuleRight.position;
			}
			else 
			{
				Tweens.SwoopTo(temp.transform, ModuleRight.position).OnComplete(() => temp.SetActive(false));
				m.transform.position = ModuleLeft.position;
			}
		}
		else
		{
			m.transform.position = ModuleRight.position;
		}

		
		

		Sequence s = Tweens.SwoopTo(m.transform, ModuleTarget.position);
		ModuleCurrent = m;
	}

	public void ShowEndGame()
	{
		//WinMenu.TweenActive(active);
		WinMenu[0].TweenActive(true);
		WinMenu[0].ClearActions();
		WinMenu[0].AddAction(UIAction.MouseUp, () => 
		{
			GameManager.instance.DestroyGame();
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
		UIObj info = GrandUI["info"];
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
		}
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

	public IEnumerator ResourceAlert(Resource r, int num)
	{
		UIAlert alert = Instantiate(UIResObj);
		WorldObjects.AddChild(alert);
		alert.ResetRect();
		alert.transform.position = ResUI[(int)r.Index].Txt[0].transform.position;

		float time_start = 0.2F;
		float time_adding = 1.2F;
		float time_end_pause = 0.4F;
		float time_end = 0.5F;
		float time_total = time_start + time_adding + time_end;

		float time_curr = 0.0F;

		alert.Setup(time_total);
		Tweens.Bounce(alert.transform);

		UIObj res = ResUI[(int)r.Index];

		float amt_soft = 0.0F;
		int init = r.Current;

		alert.Txt[0].text = "+" + num;

		yield return new WaitForSeconds(time_start);

		Tweens.Bounce(res.transform);
		while((time_curr += Time.deltaTime) <= time_adding)
		{
			amt_soft = Mathf.Lerp(0.0F, num, time_curr/time_adding);
			res.Txt[0].text = "" + (init + (int) amt_soft);

			Tweens.Bounce(res.transform);
			yield return null;
		}

		res.Txt[0].text = "" + (init + num);
		yield return new WaitForSeconds(time_end_pause);
		alert.PoolDestroy();
		yield return new WaitForSeconds(time_end);
		r.Current += num;
		CheckResourcesUI();
		yield return null;
	}

	public void CheckResourcesUI()
	{
		WorldResources wres = GameManager.WorldRes;
		for(int i = 0; i < ResUI.Length; i++)
		{
			if(wres[i] == null) continue;
			ResUI[i].Txt[0].text = wres[i].Value.ToString();
			ResUI[i].Img[0].color = wres[i].Col;
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
			//Debug.Log(Objects[i].Name);
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
		s.Append(t.DOMove(t.position - vel * 0.4F, 0.2F));
		s.Append(t.DOMove(target + vel * 0.2F, 0.2F));
		s.Append(t.DOMove(target, 0.1F));
		return s;
	}

	public static Sequence PictureSway(Transform t, float intensity = 1.0F)
	{
		Vector3 rotamt = new Vector3(0.0F, 0.0F, 20.0F * intensity);
		Sequence s = DOTween.Sequence();
		s.Append(t.DOLocalRotate(rotamt, 0.2F));
		s.Append(t.DOLocalRotate(-rotamt*0.6F, 0.2F));
		s.Append(t.DOLocalRotate(rotamt*0.3F, 0.2F));
		s.Append(t.DOLocalRotate(Vector3.zero, 0.2F));


		return s;
	}
}
