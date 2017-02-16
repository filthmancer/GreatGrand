using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Filthworks
{
	public class FOBJ : MonoBehaviour
	{
		public int Index;
		public string Name;
		public FOBJ Parent;
		public FOBJ [] Child;
		public virtual FOBJ this[int i]
		{
			get{return Child[i];}
			set{Child[i] = value;}
		}
		public bool SetInactiveAfterLoading = false;
		public bool isActive;

		public virtual void SetActive(bool? active = null)
		{
			bool actual = active ?? !this.gameObject.activeSelf;
			isActive = actual;
			this.gameObject.SetActive(actual);
		}

		public void AddChild(params FOBJ [] c)
		{
			FOBJ [] newchild = new FOBJ[Child.Length+c.Length];
			for(int i = 0; i < Child.Length; i++)
			{
				newchild[i] = Child[i];
			}
			int x = 0;
			for(int i = Child.Length; i < Child.Length + c.Length; i++)
			{
				newchild[i] = c[x];
				newchild[i].Init(i, this);
				newchild[i].transform.SetParent(this.transform, false);
				newchild[i].transform.localRotation = Quaternion.identity;
				newchild[i].transform.localPosition = Vector3.zero;
				newchild[i].transform.localScale = Vector3.one;
				x++;
			}
			Child = newchild;
		}

		public virtual void Init(int ind, FOBJ p, params float [] args)
		{
			if(ind != -1) Index = ind;
			Parent = p;

			if(Name == string.Empty) Name = gameObject.name;
			else gameObject.name = Name;
			
			for(int i = 0; i < Child.Length; i++)
			{
				if(Child[i] == null) continue;
				Child[i].Init(i, this);
			}
		
			if(SetInactiveAfterLoading) SetActive(false);
			else isActive = this.gameObject.activeSelf;
		}

	}

}	