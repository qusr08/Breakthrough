using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Settings", menuName = "Theme Settings")]
public class ThemeSettings : ScriptableObject {
	[SerializeField] private Color _backgroundColor;
	[SerializeField] private Color _detailColor;
	[SerializeField] private Color _textColor;
	[SerializeField] private Color _glowColor;
	[SerializeField] private Color _hazardColor;
	[SerializeField] private Color _breakthroughColor;
	[SerializeField] private List<Color> _backgroundDetailColors;
	[SerializeField] private MinoBlockColorDictionary _minoBlockColors;
	[SerializeField] private WallBlockColorDictionary _wallBlockColors;

	#region Properties
	public Color BackgroundColor => _backgroundColor;
	public Color DetailColor => _detailColor;
	public Color TextColor => _textColor;
	public Color GlowColor => _glowColor;
	public Color HazardColor => _hazardColor;
	public Color BreakthroughColor => _breakthroughColor;
	public List<Color> BackgroundDetailColors => _backgroundDetailColors;
	public MinoBlockColorDictionary MinoBlockColors => _minoBlockColors;
	public WallBlockColorDictionary WallBlockColors => _wallBlockColors;
	#endregion

	
}
