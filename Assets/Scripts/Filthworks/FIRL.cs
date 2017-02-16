using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SVGImporter;
using TMPro;
using Filthworks;

namespace Filthworks
{

	
	public class FIRL : FOBJ {
		
		public SVGRenderer [] SVG;
		public TextMeshPro [] Text;

		private Transform _trans;
		public Transform T {get{if(_trans == null) _trans = this.transform;	return _trans;}}
		


		
		public override void Init(int ind, FOBJ p, params float [] args)
		{
			base.Init(ind, p, args);
			if(SVG.Length == 0 && GetComponent<SVGRenderer>()) SVG = new SVGRenderer[]{GetComponent<SVGRenderer>()};
			if(Text.Length == 0 && GetComponent<TextMeshPro>()) Text = new TextMeshPro[]{GetComponent<TextMeshPro>()};
		}

		

	}
}

