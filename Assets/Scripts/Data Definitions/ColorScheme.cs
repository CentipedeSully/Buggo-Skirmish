using UnityEngine;



[CreateAssetMenu(fileName = "NewColorScheme", menuName = "Data/ColorScheme")]
public class ColorScheme : ScriptableObject
{
    [SerializeField] private Color _primaryColor = Color.white;
    [SerializeField] private Color _secondaryColor = Color.white;
    [SerializeField] private Color _tertiaryColor = Color.white;
    [SerializeField] private Color _blinkColor = Color.white;



    public Color Primary()
    {
        return _primaryColor;
    }

    public Color Secondary()
    {
        return _secondaryColor;
    }

    public Color Tertiary()
    {
        return _tertiaryColor;
    }

    public Color BlinkColor()
    {
        return _blinkColor;
    }
}
