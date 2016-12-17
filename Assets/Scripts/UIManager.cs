using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour {
	public Transform Canvas;

	public UIObj WinMenu;
	public UIObj GrandUI;
	public UIObj FaceParent;
	
	public FaceObj ActiveFace;

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
	}

	public void SetGrandUI(GreatGrand g)
	{
		UIObj info = GrandUI["info"];
		UIObj face = GrandUI["face"];

		info.Txt[0].text = g.Info.Name;
		info.Txt[1].text = g.Info.Age+"";
		info.Txt[2].text = "";
	}

	

}
