using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _foregroundImage;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _reduceSpeed = 2f;

    private float _widthModifier = 0.25f;
    private float _target = 1;

    private void Awake()
    {
        _foregroundImage.fillAmount = 1f;

        Hide();
    }

    private void Update()
    {
        _foregroundImage.fillAmount = Mathf.MoveTowards(_foregroundImage.fillAmount, _target, _reduceSpeed * Time.deltaTime);

        // If full health hide gameobject
        if (_foregroundImage.fillAmount == 1)
        {
            Hide();
        }
    }

    // --------- Public Methods --------- //

    public void ChangeWidth(float maxHealth)
    {
        float Width = maxHealth * _widthModifier;
        RectTransform rectTransform = _canvas.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(Width, rectTransform.sizeDelta.y);
    }

    public void ChangeColor(Color color)
    {
        _foregroundImage.color = color;
    }

    public void UpdateHealthBar(float maxHeatlh, float currentHealth)
    {
        Show();

        _target = currentHealth / maxHeatlh;
    }

    // --------- Private Methods --------- //

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
