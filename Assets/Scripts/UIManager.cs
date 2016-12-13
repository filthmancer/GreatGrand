using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour {

	public UIObj WinMenu;

	public Transform Canvas;

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

	public FaceObj ActiveFace;

}
