using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageGridComponent : GridComponent {
	[Space]
	[SerializeField] protected RectTransform imageRectTransform;
	[SerializeField] protected Image image;

	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		image.color = Color.white;
	}
	#endregion
}
