using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Settings", menuName = "Theme Settings")]
public class ThemeSettings : ScriptableObject {
	[Header("Properties")]
	[SerializeField] private Color _backgroundColor;
	[SerializeField] private Color _detailColor;
	[SerializeField] private Color _backgroundDetailColor;
	[SerializeField] private BlockColorColorDictionary _blockColors;
	[SerializeField] private Color _glowColor;
	[SerializeField] private Color _hazardColor;
	[SerializeField] private Color _breakthroughColor;

	#region Properties
	public Color BackgroundColor => _backgroundColor;
	public Color DetailColor => _detailColor;
	public Color BackgroundDetailColor => _backgroundDetailColor;
	public BlockColorColorDictionary BlockColors => _blockColors;
	public Color GlowColor => _glowColor;
	public Color HazardColor => _hazardColor;
	public Color BreakthroughColor => _breakthroughColor;
	#endregion
}
