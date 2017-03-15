using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SVGImporter;

public class Face_Obj : MonoBehaviour {

	public SVGRenderer [] Images;
	public Face _Face;
	public Face_Obj _Parent;
	public Animator Anim;

	public void Setup(Face f, Face_Obj p)
	{
		_Face = f;
		_Parent = p;
		transform.SetParent(_Parent.transform);
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		transform.localRotation =  Quaternion.identity;
	}

	public void Create(FaceInfo f)
	{
		//Vector3 randomised_pos = Vector3.zero;
		//randomised_pos.x = Mathf.Lerp(ele_rect.xMin, ele_rect.xMax, Element.GetComponent<RectTransform>().pivot.x + Info._Position.x);
		//randomised_pos.y =  Mathf.Lerp(ele_rect.yMin, ele_rect.yMax, Element.GetComponent<RectTransform>().pivot.y + Info._Position.y);

		//Element.transform.localPosition = randomised_pos;
		
		f._Position.z = 0;
		transform.localPosition = f._Position;
		transform.localRotation = Quaternion.Euler(f._Rotation);
		transform.localScale = f._Scale;
		
		if(_Face != null)
		{
			Color final = Color.white;
	
			switch(f.Colour)
			{
				case ColorType.Skin:
					final = _Face.SkinCol;
				break;
				case ColorType.Hair:
					final = _Face.HairCol;
				break;
				case ColorType.Offset:
					final = _Face.OffsetCol;
				break;
				case ColorType.Nose:
					final = _Face.NoseCol;
				break;
				case ColorType.Feature:
					final = Color.black;
				break;
			}

			Images[0].color = final;
		}
	}
}
