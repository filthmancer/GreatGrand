using UnityEngine;
using System.Collections;

public class GrumpObj : InputTarget {

	public bool Draggable;
	public float DragDistanceThreshold = 0.5F;
	public bool isDragging;

	private Transform trans;

	void Start()
	{
		trans = this.transform;
	}

	public virtual void Tap(Vector3 pos)
	{
		
	}

	public override void Release(Vector3 pos)
	{
		if(!isDragging) Tap(pos);
		isDragging = false;
	}

	public override void Drag(Vector3 pos)
	{
		if(!isDragging)
		{
			float d = Vector3.Distance(pos, trans.position);
			if(d > DragDistanceThreshold) isDragging = true;
		}

		if(isDragging)
		{
			//trans.position = new Vector3(pos.x, 0.5F, pos.z);
		}

		
	}


}
