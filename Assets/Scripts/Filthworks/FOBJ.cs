using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

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

		private bool Log = false;

		public FOBJ this[string s]
		{
			get
			{
				foreach(FOBJ child in Child)
				{
					if(child == null) continue;
					if(child.Name == s) return child;
					if(child[s] != null) return child[s];
				}
				return null;
			}
		}

		private Transform _trans;
		public Transform T {get{if(_trans == null) _trans = this.transform;	return _trans;}}
		public Vector3 pos {get{return T.position;}}
		public Vector3 scale {get{return T.localScale;}}
		public Quaternion rot {get{return T.rotation;}}

		public bool SetInactiveAfterLoading = false;
		public bool isActive;

		public virtual void SetActive(bool? active = null)
		{
			bool actual = active ?? !this.gameObject.activeSelf;
			isActive = actual;
			this.gameObject.SetActive(actual);
		}

		private Vector3 activescale = Vector3.one;
		public virtual void TweenActive(bool ? active = null)
		{
			bool actual = active ?? !this.gameObject.activeSelf;
			if(actual == this.gameObject.activeSelf) return;

			isActive = actual;
			if(actual)
			{
				this.gameObject.SetActive(true);
				if(this.transform.localScale != Vector3.zero) activescale = this.transform.localScale;
				else activescale = Vector3.one;

				this.transform.localScale = Vector3.zero;

				Sequence s = Tweens.Bounce(this.transform, activescale).OnComplete(() => {activescale = this.transform.localScale;});
			}
			else
			{
				this.transform.DOScale(Vector3.zero, 0.25F).OnComplete(() =>{this.gameObject.SetActive(false);});
			}
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



		public void OnClick()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Click - " + Actions_Click.Count + " actions");

			foreach(InputAction child in Actions_Click)
			{
				child.Act();
			}
		}

		public void OnStay()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Stay - " + Actions_Click.Count + " actions");

		}

		public void OnEnter()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Enter - " + Actions_Over.Count + " actions");

			foreach(InputAction child in Actions_Over)
			{
				child.Act();
			}
		}

		public void OnExit()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Exit - " + Actions_Out.Count + " actions");

			foreach(InputAction child in Actions_Out)
			{
				child.Act();
			}
		}


		public bool PlayClickDown = true, PlayClickUp = true;
		public void OnDown()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Down - " + Actions_Down.Count + " actions");

			foreach(InputAction child in Actions_Down)
			{
				child.Act();
			}
		}

		public void OnUp()
		{
			if(GameManager.IgnoreInput) return;
			if(Log) print(this + ": Mouse Up - " + Actions_Up.Count + " actions");
			foreach(InputAction child in Actions_Up)
			{
				child.Act();
			}
		}

		public void OnDestroy()
		{
			if(Log) print(this + ": Destroy - " + Actions_Destroy.Count + " actions");
			foreach(InputAction child in Actions_Destroy)
			{
				child.Act();
			}
		}

		public void ClearActions()
		{
			Actions_Over = new List<InputAction>();
			Actions_Out = new List<InputAction>();
			Actions_Up = new List<InputAction>();
			Actions_Down = new List<InputAction>();
			Actions_Click = new List<InputAction>();
			Actions_Destroy = new List<InputAction>();
		}


		public void AddAction(TouchAction a, Action<Type[]> func, params Type [] t)
		{
			switch(a)
			{
				case TouchAction.Out:
				Actions_Out.Add(new InputAction(func, t));
				break;
				case TouchAction.Over:
				Actions_Over.Add(new InputAction(func, t));
				break;
				case TouchAction.Up:
				Actions_Up.Add(new InputAction(func, t));
				break;
				case TouchAction.Down:
				Actions_Down.Add(new InputAction(func, t));
				break;
				case TouchAction.Click:
				Actions_Click.Add(new InputAction(func, t));
				break;
				case TouchAction.Destroy:
				Actions_Destroy.Add(new InputAction(func, t));
				break;
			}
		}

		public void AddAction(TouchAction a, Action func)
		{
			switch(a)
			{
				case TouchAction.Out:
				Actions_Out.Add(new InputAction(func));
				break;
				case TouchAction.Over:
				Actions_Over.Add(new InputAction(func));
				break;
				case TouchAction.Up:
				Actions_Up.Add(new InputAction(func));
				break;
				case TouchAction.Down:
				Actions_Down.Add(new InputAction(func));
				break;
				case TouchAction.Click:
				Actions_Click.Add(new InputAction(func));
				break;
				case TouchAction.Destroy:
				Actions_Destroy.Add(new InputAction(func));
				break;
			}
		}

		private List<InputAction> Actions_Over = new List<InputAction>();
		private List<InputAction> Actions_Out = new List<InputAction>();
		private List<InputAction> Actions_Up = new List<InputAction>();
		private List<InputAction> Actions_Down = new List<InputAction>();
		private List<InputAction> Actions_Click = new List<InputAction>();
		private List<InputAction> Actions_Destroy = new List<InputAction>();

		private class InputAction
		{
			public Action<Type[]> Function;
			public Type [] Args;
			public InputAction(Action<Type[]> func, params Type [] t)
			{
				Function = func;
				Args = t;
			}

			public InputAction(Action func)
			{
				Function = delegate{func();};
				Args = new Type[0];
			}


			public void Act()
			{
				Function(Args);
			}
		}

		public void DestroyChildren()
		{
			for(int i = 0; i < Child.Length; i++)
			{
				Child[i].PoolDestroy();
			}
			Child = new FOBJ[0];
		}

		protected ObjectPoolerReference poolref;
		public ObjectPoolerReference GetPoolRef(){return poolref;}

		

		public void PoolDestroy()
		{
			if(Parent != null) Parent.RemoveChild(this);
			OnDestroy();
			if(poolref)
			{
				poolref.Unspawn();
			}
			else Destroy(this.gameObject);
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

		public void SetParent(FOBJ f)
		{
			f.AddChild(this);
		}

		public void RemoveChild(FOBJ c)
		{
			bool remove = false;
			int index = 0;
			for(int i = 0; i < Child.Length; i++)
			{
				if(Child[i] == c)
				{
					remove = true;
					index = i;
					break;
				}
			}
			if(!remove) return;
			FOBJ [] newchild = new FOBJ[Child.Length-1];
			int a =0;
			for(int i = 0; i < index; i++)
			{
				newchild[a] = Child[i];
				a++;
			}
			for(int x = index+1; x < Child.Length; x++)
			{
				newchild[a] = Child[x];
				a++;
			}
			Child = newchild;
		}

	}
	public enum TouchAction {Click, Over, Out, Up, Down, Destroy}
}	