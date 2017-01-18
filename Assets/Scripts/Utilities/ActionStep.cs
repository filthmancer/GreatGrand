using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;

namespace Utilities
{
	[System.Serializable]
	public class ActionStep {

		private List<ActionSingle> Steps = new List<ActionSingle>();
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
			scalefinal = new Vector3(Mathf.Clamp(scalefinal.x, 0.0F, Mathf.Infinity),
									Mathf.Clamp(scalefinal.y, 0.0F, Mathf.Infinity),
									Mathf.Clamp(scalefinal.z, 0.0F, Mathf.Infinity));

			SeqCurr.Append(trans.DOMove(posfinal, Current.Time));
			SeqCurr.Join(trans.DOScale(scalefinal, Current.Time)).OnComplete(() => 
				{
					AdvanceStep(1);
					Current.Complete();
				});


			//SeqCurr.Join(trans.DORotate(Current.Rotation, Current.Time));
			
		}

		public void RestartSteps(){
			current = 0;
			AdvanceStep();
		}
		public void ClearSteps()
		{
			current = 0;
			Steps.Clear();
		}

		public void Setup(Transform t)
		{
			trans = t;
			Steps = new List<ActionSingle>();
			current  = 0;
		}


		public ActionSingle AddStep(float t, params Vector3 [] v)
		{
			ActionSingle a = new ActionSingle(t, v);
			Steps.Add(a);
			return a;
		}


	}


	public class ActionSingle
	{
		public Vector3 Position;
		public Vector3 Scale;
		public Vector3 Rotation;

		public bool Relative = true;
		public float Time = 1.0F;

		public Action<string []> Act;
		public string [] Args;


		public void Complete()
		{
			Act(Args);
		}

		public ActionSingle(float _time, params Vector3 [] d)
		{
			Position = d.Length >=1 ? d[0] : Vector3.zero;
			Scale =  d.Length >=2 ? d[1] : Vector3.zero;
			Rotation = d.Length >=3 ? d[3] : Vector3.zero;
			Time = _time;

			Relative = true;
		}

		public void AddAction(Action<string []> act= null, params string [] s)
		{
			Act = act;
			Args = s;
		}
	}
}

