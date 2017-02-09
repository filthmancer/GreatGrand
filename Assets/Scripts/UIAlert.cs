using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utilities;
using DG.Tweening;

public class UIAlert : UIObj {

	private bool FadeOut;
	private float FadeOut_Ratio = 0.8F;

	public ActionStep Stepper;
	private float _lifetimeoverride;
	private bool DestroyOnDeath = true;
	public override void Init(int index, UIObj p, params float [] args)
	{
		base.Init(index, p, args);
		Stepper.Setup(this.transform);
		if(args.Length == 0) return;

		_lifetimeoverride = args[0];
		if(_lifetimeoverride < 0.0F) DestroyOnDeath = false;
		played_startaction = false;
	}

	public ActionSingle AddStep(float t, params Vector3 [] a)
	{
		return Stepper.AddStep(t, a);
	}

	public Action<string []> StartAction;
	public Action<string []> EndAction;
	public string [] EndArgs, StartArgs;

	private bool _Completed;
	private Sequence _Sequence;
	private bool played_startaction= false;

	public void Update()
	{
		if(!played_startaction)
		{
			if(_Sequence == null && _lifetimeoverride > 0.0F) 
			{
				_Sequence = DOTween.Sequence();
				_Sequence.AppendInterval(_lifetimeoverride);
				//_Sequence.Append(transform.DOLocalMove(transform.position, _lifetimeoverride));
				_Sequence.OnComplete(() =>
				{
					_Completed = true;
				});
			}
			played_startaction = true;
			if(StartAction != null) StartAction(StartArgs);
		}

		if(!_Completed)
		{
			
		}	
		else if(DestroyOnDeath)
		{
			if(EndAction != null) EndAction(EndArgs);
			PoolDestroy();
		}
	}

	public void RestartSteps()
	{
		Stepper.RestartSteps();
	}

	public void ClearSteps()
	{
		Stepper.ClearSteps();
	}



}
