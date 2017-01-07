using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour {
	public Transform Canvas;
	public Canvas LineCanvas;

	public UIObj Menu;
	public UIObj DinnerUI;
	public UIObj WinMenu;
	public UIObj GrandUI;
	public UIObj FaceParent;
	public UIObj WorldObjects;
	
	public FaceObj ActiveFace;
	public Sprite Angry, Bored, Happy;

	public ObjectContainer Sprites;

	public void Init()
	{
		Menu.SetActive(true);

		DinnerUI[1].AddAction(UIAction.MouseDown, () =>
		{
			for(int i = 0; i < GameManager.instance.GG.Length; i++)
			{
				GameManager.instance.GG[i].ShowGrumpLines();
			}
		});
		DinnerUI.SetActive(false);
		Menu[0].ClearActions();
		Menu[0].AddAction(UIAction.MouseUp, () => 
		{
			GameManager.instance.Clear();
			GameManager.instance.LoadMinigame("Dinner");
		});
	}

	public void ShowEndGame(bool active)
	{
		WinMenu.SetActive(active);
		if(!active)return;
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
		if(Input.GetKeyDown(KeyCode.S)) 
		{
			UIAlert a = StringAlert("YO", Canvas.transform.position);
			a.AddStep(Canvas.transform.position + Canvas.transform.up * 4, 1.0f ,(string [] s) =>{
				Debug.Log(s[0]);
		 }, "HI");
			a.AddStep(Canvas.transform.position - Canvas.transform.up * 10, 1.0f, (string [] s) =>{});
		}
	}



	public void SetGrandUI(GreatGrand g)
	{
		UIObj info = GrandUI["info"];
		UIObj face = GrandUI["face"];

		info.Txt[0].text = g.Info.Name;
		info.Txt[1].text = g.Info.Age+"";
		info.Txt[2].text = "";

		if(ActiveFace != null) Destroy(ActiveFace.gameObject);

		if(g.Face != null)
		{
			ActiveFace = g.CloneFace();
			
			face.AddChild(ActiveFace);
			//g.ResetFace(ActiveFace);

			//ActiveFace.GetComponent<RectTransform>().anchorMin = Vector3.one*0.5F;
			//ActiveFace.GetComponent<RectTransform>().anchorMax = Vector3.one*0.5F;
			ActiveFace.transform.localPosition = Vector3.zero;
			ActiveFace.transform.localScale = Vector3.one * 0.35F;
			
			ActiveFace.transform.localRotation = Quaternion.Euler(0,0,0);
			ActiveFace.GetComponent<Animator>().enabled = false;
			
		}
	}

	public UIAlert UIAlertObj;
	public UIAlert StringAlert(string s, Vector3 pos, float lifetime = 2.0F, float size = 50.0F, float speed = 1.0F)
	{
		UIAlert final = Instantiate(UIAlertObj);
		WorldObjects.AddChild(final);
		final.ResetRect();
		final.transform.position = pos;

		final.Txt[0].enabled = true;
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

		final.Txt[0].enabled = false;
		final.Img[0].enabled = true;
		final.Img[0].sprite = s;
		final.Setup(lifetime, speed);
		return final;
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
