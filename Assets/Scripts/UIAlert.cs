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
	public override void Setup(params float [] args)
	{
		Stepper.Setup(this.transform);
		_lifetimeoverride = args[0];
		played_startaction = false;
	}

	public ActionSingle AddStep(Vector3 targ, float t, Action<string []> act= null, params string [] s)
	{
		return Stepper.AddStep(targ, t, act, s);
	}

	public Action<string []> StartAction;
	public Action<string []> EndAction;
	public string [] EndArgs, StartArgs;

	private bool played_startaction= false;

	public void Update()
	{
		if(!played_startaction)
		{
			played_startaction = true;
			if(Stepper.Empty) 
			{
				Stepper.AddStep(transform.position, _lifetimeoverride);
			}
			Stepper.AdvanceStep();
			if(StartAction != null) StartAction(StartArgs);
			return;
		}

		if(Stepper.Empty)
		{
			return;
		}

		if(Stepper.Update())
		{
			
		}	
		else
		{
			if(EndAction != null) EndAction(EndArgs);
			PoolDestroy();
		}
	}



}
