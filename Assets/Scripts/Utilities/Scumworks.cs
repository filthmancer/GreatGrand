using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Scumworks
{

//Daily Timer/Reward Loop
	public class GoodBoy
	{
		public System.TimeSpan Wait;
		public System.DateTime Next;
		public System.TimeSpan ToNext
		{
			get{return Next.Subtract(System.DateTime.Now);}
		}

		public GoodBoy(System.DateTime n, System.TimeSpan t)
		{
			Wait = t;
			Next = n;
			Next = Next.Add(Wait);
		}

		public bool Check()
		{
			return System.DateTime.Compare(System.DateTime.Now, Next) >= 0;
		}

		public bool Claim(System.Action a)
		{
			if(!Check()) return false;
			a();
			Next = Next.Add(Wait);
			return true;
		}
	}

//Ads with rewards
	public class Showman
	{

	}
}
