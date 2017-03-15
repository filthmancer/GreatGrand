using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SVGImporter;
using Filthworks;

public class Face : FOBJ {
	public FaceGroup FaceParents, FaceChildren;
	public Face_Obj Glasses_Left, Glasses_Right, Wrinkles;
	public SVGRenderer Skin,Shadow;
	public BoxCollider Collider;
	public void Create(GrandInfo Info)
	{
		SetSkinColor(Info.C_Skin);
		SetHairColor(Info.C_Hair);
		SetOffsetColor(Info.C_Offset);
		SetNoseColor (Info.C_Nose);

		Skin.color = SkinCol;
		Shadow.color = Color.black;

		for(int i = 0; i < FaceChildren.Objects.Length; i++)
		{
			FaceChildren.Objects[i].Setup(this,FaceParents.Objects[i]);
		}

		FaceChildren.Left_Eye.Create(Info.Eye);
		FaceChildren.Right_Eye.Create(Info.Eye);
		FaceChildren.Left_Ear.Create(Info.Ear);
		FaceChildren.Right_Ear.Create(Info.Ear);
		FaceChildren.Left_Brow.Create(Info.Brow);
		FaceChildren.Right_Brow.Create(Info.Brow);
		FaceChildren.Hair.Create(Info.Hair);
		FaceChildren.Jaw.Create(Info.Jaw);
		FaceChildren.Nose.Create(Info.Nose);

		FaceChildren.Left_Eye.Images[1].transform.localScale = Info.PupilScale;
		FaceChildren.Right_Eye.Images[1].transform.localScale = Info.PupilScale;
		FaceChildren.Left_Eye.Images[1].color = Info.C_Eye;
		FaceChildren.Right_Eye.Images[1].color = Info.C_Eye;

		if(Info.Glasses != null) 
		{
			Glasses_Left.Setup(this, FaceParents.Left_Eye);
			Glasses_Right.Setup(this, FaceParents.Right_Eye);
			Glasses_Left.Create(Info.Glasses);
			Glasses_Right.Create(Info.Glasses);
		}
		if(Info.Wrinkles != null) Wrinkles.Create(Info.Wrinkles);
	}

	
	public Color	_skincol;
	public Color SkinCol
	{get{return _skincol;}}
	public void SetSkinColor(Color c)
	{
		_skincol = c;
	}

	public Color	_haircol;
	public Color HairCol
	{get{return _haircol;}}
	public void SetHairColor(Color c)
	{
		_haircol = c;
	}

	public Color _offsetcol;
	public Color OffsetCol
	{
		get{return _offsetcol;}
	}
	public void SetOffsetColor(Color c)
	{
		_offsetcol = c;
	}

	public Color _nosecol;
	public Color NoseCol
	{
		get{return _nosecol;}
	}
	public void SetNoseColor(Color c)
	{
		_nosecol = c;
	}

}

[System.Serializable]
public class FaceGroup
{
	public Face_Obj Right_Eye, Left_Eye,
					Right_Brow, Left_Brow,
					Right_Ear, Left_Ear,
					Nose, Jaw, Hair;

	public Face_Obj [] Objects
	{
		get{return new Face_Obj []{Right_Eye, Left_Eye,
					Right_Brow, Left_Brow,
					Right_Ear, Left_Ear,
					Nose, Jaw, Hair};}
	}
}
