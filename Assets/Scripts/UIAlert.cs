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

	private bool played_startaction= false;

	public void Update()
	{
		if(!played_startaction)
		{
			played_startaction = true;
			if(Stepper.Empty) 
			{
				if(DestroyOnDeath) Stepper.AddStep(_lifetimeoverride);
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
