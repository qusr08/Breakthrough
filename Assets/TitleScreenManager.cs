using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private RectTransform canvasRectTransform;
	[Header("Properties")]
	[SerializeField] private int _gridWidth;
	[SerializeField] private int _gridHeight;
	[Header("Information")]
	[SerializeField] private int _gridSize;

	#region Properties
	public int GridWidth => _gridWidth;
	public int GridHeight => _gridHeight;

	public int GridSize { get => _gridSize; private set => _gridSize = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		canvasRectTransform = GetComponent<RectTransform>( );

		Debug.Log("OnValidate");
		Debug.Log((int) canvasRectTransform.rect.width);
		Debug.Log((int) canvasRectTransform.rect.height);

		Debug.Log(Screen.width / Screen.height);
		Debug.Log(16 / 9f);

		GridSize = Utils.GCD((int) canvasRectTransform.rect.width, (int) canvasRectTransform.rect.height);
	}

	private void Awake ( ) {
		Debug.Log("Awake");
		OnValidate( );
	}
	#endregion
}
