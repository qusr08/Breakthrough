using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BoardText : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private GameManager gameManager;
	[SerializeField] private TextMeshPro labelText;
	[SerializeField] private TextMeshPro valueText;
	[SerializeField] private RectTransform labelRectTransform;
	[SerializeField] private RectTransform valueRectTransform;
	[Header("Properties")]
	[SerializeField] private bool isPercentange;
	[SerializeField] private bool disableValueText;
	[SerializeField] private float _labelHeight;
	[SerializeField] private float _valueHeight;
	[SerializeField] private float _width;

	#region Properties
	public string Label { get => labelText.text; set => labelText.text = value; }
	public float Value {
		get => float.Parse(valueText.text.Replace(",", "").Replace("%", "").Replace(".", ""));
		set => valueText.text = (isPercentange ? $"{value:0.##}%" : $"{value:n0}");
	}

	public float LabelHeight => _labelHeight;
	public float ValueHeight => disableValueText ? 0f : _valueHeight;
	public float Width => _width;
	public float Height => ValueHeight + LabelHeight;
	#endregion

#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		themeManager = FindObjectOfType<ThemeManager>( );
		gameManager = FindObjectOfType<GameManager>( );

		// Set the size and position of the text boxes
		labelRectTransform.localPosition = new Vector3(Width / 2f, -LabelHeight / 2f);
		labelRectTransform.sizeDelta = new Vector2(Width, LabelHeight);
		valueRectTransform.localPosition = new Vector3(Width / 2f, -LabelHeight - (ValueHeight / 2f));
		valueRectTransform.sizeDelta = new Vector2(Width, ValueHeight);

		// Set the color of the text
		labelText.color = themeManager.ActiveTheme.TextColor;
		valueText.color = themeManager.ActiveTheme.TextColor;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}
