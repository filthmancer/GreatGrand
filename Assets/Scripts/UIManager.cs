using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour {

	public UIObj WinMenu;

	public Transform Canvas;
	public UIObj FaceParent;

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

	public void ShowFace(FaceObj f, bool? active = null)
	{
		bool actual = active ?? !f.isActive;
		ActiveFace = actual ? f : null;
		for(int i = 0; i < FaceParent.Length; i++)
		{
			if(FaceParent[i] == f) 
			{
				FaceParent[i].SetActive(actual);
			}
			else FaceParent[i].SetActive(false);
		}
	}
}
