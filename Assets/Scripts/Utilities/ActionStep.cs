using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;

namespace Utilities
{
	[System.Serializable]
	public class ActionStep {

		private List<ActionSingle> Steps;
		private int current = 0;

		private Transform trans;
		private Sequence SeqCurr;
		public ActionSingle Current
		{
			get{
				if(Steps == null || Steps.Count == 0) return null;
				return Steps[Mathf.Clamp(current, 0, Steps.Count-1)];
			}
		}
		public bool Empty
		{
			get{return Current == null;}
		}

		public bool Complete
		{
			get{return current >= Steps.Count;}
		}

		public bool Update()
		{
			if(Empty || Complete) return false;
			return true;
		}

		public void AdvanceStep(int i = 0)
		{
			current += i;
			if(Complete) return;

			SeqCurr = DOTween.Sequence();
			Vector3 posfinal = Current.Position + (Current.Relative ? trans.position: Vector3.zero);
			Vector3 scalefinal = Current.Scale + (Current.Relative ? trans.localScale: Vector3.zero);
			SeqCurr.Append(trans.DOMove(posfinal, Current.Time));
			//SeqCurr.Join(trans.DORotate(Current.Rotation, Current.Time));
			SeqCurr.Join(trans.DOScale(scalefinal, Current.Time)).OnComplete(() => 
				{
					AdvanceStep(1);
					Current.Complete();
				});
			
		}

		public void Setup(Transform t)
		{
			trans = t;
			Steps = new List<ActionSingle>();
			current  = 0;
		}

		public ActionSingle AddStep(Vector3 targ, float t, Action<string []> act= null, params string [] s)
		{
			ActionSingle a = new ActionSingle(targ,t, act, s);
			Steps.Add(a);
			return a;
		}


	}


	public class ActionSingle
	{
		public Vector3 Position;
		public Vector3 Scale;

		public bool Relative = true;
		public float Time = 1.0F;

		public Action<string []> Act;
		public string [] Args;


		public void Complete()
		{
			Act(Args);
		}

		public ActionSingle(Vector3 t, float _time, Action<string[]> a, params string [] s)
		{
			Position = t;
			Scale = Vector3.zero;
			Time = _time;

			Relative = true;
			Args = s;
			Act = a;
			Time = _time;
		}
	}
}

