using UnityEngine;
using UnityEngine.UI;

public class SeedlingItem : MonoBehaviour
{
    [SerializeField] private Image seedlingImage;

    private static readonly Color EffectiveColor  = new Color(0.2f, 0.8f, 0.3f);
    private static readonly Color SpeedColor      = new Color(1f,   0.7f, 0.1f);
    private static readonly Color ResistanceColor = new Color(0.7f, 0.2f, 0.9f);

    public Seedling Data    { get; private set; }
    public bool    IsGrown { get; private set; }

    public void Initialize(Seedling data, bool isGrown = false)
    {
        IsGrown = isGrown;
        Data = data;

        if (isGrown && data.sourcePlant?.grownSprite != null)
            seedlingImage.sprite = data.sourcePlant.grownSprite;

        float total = data.effective + data.speed + data.resistance;
        if (total <= 0) return;

        float e = data.effective  / total;
        float s = data.speed      / total;
        float r = data.resistance / total;

        seedlingImage.color = EffectiveColor * e + SpeedColor * s + ResistanceColor * r;
    }
}
