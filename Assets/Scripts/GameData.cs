using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {
	public Color [] GG_Colours;
	public Color [] Skin_Colours, Hair_Colours;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Color CycleSkin(int i)
	{
		if(Skin_Colours.Length == 0) return Color.white;
		int actual = Mathf.Abs(i) % Skin_Colours.Length;
		return Skin_Colours[actual];
	}
	public Color RandomSkin(){return Skin_Colours[Random.Range(0, Skin_Colours.Length)];}
	public Color CycleHair(int i)
	{
		if(Hair_Colours.Length == 0) return Color.white;
		int actual = Mathf.Abs(i) % Hair_Colours.Length;
		return Hair_Colours[actual];
	}
	public Color RandomHair(){return Hair_Colours[Random.Range(0, Hair_Colours.Length)];}
}

