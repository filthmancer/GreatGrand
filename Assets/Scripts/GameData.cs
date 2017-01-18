using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}



	
}


[System.Serializable]
public class WorldResources
{
	public Resource Rep, Funds, Meds;
	public Resource [] AllRes
	{
		get{return new Resource[]{Rep, Funds, Meds};}
	}
	public WorldResources()
	{
		Rep = new Resource(0);
		Funds = new Resource(1);
		Meds = new Resource(2);
	}
	public void Init()
	{
		Rep.Index = 0;
		Rep.Name = "Rep";
		Funds.Index = 1;
		Funds.Name = "Funds";
		Meds.Index = 2;
		Meds.Name = "Meds";
	}
	public Resource this[int v]
	{
		get{return AllRes[v];}
	}
}

[System.Serializable]
public class GrandResources
{
	public Resource Smiles, Grumps;
	public Resource Fit, Slob;
}

[System.Serializable]
public class Resource
{
	public int Value
	{
		get{
			return (int)((float)Current * Multiplier);
		}
	}

	public float Multiplier;
	public int Current;

	public Color Col;
	[HideInInspector]
	public int Index;
	public string Name;


	public Resource(int ind)
	{
		Current = 1;
		Multiplier = 1.0F;
		Col = Color.white;
		Index = ind;
	}
}

