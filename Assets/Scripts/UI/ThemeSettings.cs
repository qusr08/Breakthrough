using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Settings", menuName = "Theme Settings")]
public class ThemeSettings : ScriptableObject {
	[Header("Properties")]
	[SerializeField] private Color _backgroundColor;
	[SerializeField] private Color _detailColor;
	[SerializeField] private List<Color> _backgroundDetailColors;
	[SerializeField] private BlockColorColorDictionary _blockColors;
	[SerializeField] private List<Color> _buttonColors;
	[SerializeField] private Color _textColor;
	[SerializeField] private Color _glowColor;
	[SerializeField] private Color _hazardColor;
	[SerializeField] private Color _breakthroughColor;

	#region Properties
	public Color BackgroundColor => _backgroundColor;
	public Color DetailColor => _detailColor;
	public List<Color> BackgroundDetailColors => _backgroundDetailColors;
	public BlockColorColorDictionary BlockColors => _blockColors;
	public List<Color> ButtonColors => _buttonColors;
	public Color TextColor => _textColor;
	public Color GlowColor => _glowColor;
	public Color HazardColor => _hazardColor;
	public Color BreakthroughColor => _breakthroughColor;
	#endregion
}
