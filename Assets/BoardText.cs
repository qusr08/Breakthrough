using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BoardText : MonoBehaviour {
	[Header("Components - Board Text")]
	[SerializeField] private TextMeshPro labelText;
	[SerializeField] private TextMeshPro valueText;
	[SerializeField] private RectTransform labelRectTransform;
	[SerializeField] private RectTransform valueRectTransform;
	[Header("Properties - Board Text")]
	[SerializeField] private bool isPercentange;
	[SerializeField, Min(0f)] private float _width;
	[SerializeField, Min(0f)] private float labelHeight;
	[SerializeField, Min(0f)] private float valueHeight;

	public string Label => labelText.text;
	public float Value {
		get => float.Parse(valueText.text.Replace(",", "").Replace("%", "").Replace(".", ""));
		set => valueText.text = (isPercentange ? $"{value:0.##}%" : $"{value:n0}");
	}

	public float Width => _width;
	public float Height => valueHeight + labelHeight;

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

		// Set the size and position of the text boxes
		labelRectTransform.localPosition = new Vector3(Width / 2f, -labelHeight / 2f);
		labelRectTransform.sizeDelta = new Vector2(Width, labelHeight);
		valueRectTransform.localPosition = new Vector3(Width / 2f, -labelHeight - (valueHeight / 2f));
		valueRectTransform.sizeDelta = new Vector2(Width, valueHeight);
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}
