using UnityEngine;

[CreateAssetMenu(fileName = "NewPlant", menuName = "Sparaxis/Plant")]
public class PlantData : ScriptableObject
{
    public Sprite seedlingSprite;
    public Sprite younglingSprite;
    public Sprite maturingSprite;
    public Sprite grownSprite;
    public float unlockAt;
    public string bonusStat;
    public float bonusAmount;
}
