using UnityEngine;
using UnityEngine.UI;

public class ToggleColorChanger : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Color onColor = new Color(0.2f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color offColor = Color.white;

    private void Awake()
    {
        if (toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }

        if (targetGraphic == null && toggle != null)
        {
            targetGraphic = toggle.targetGraphic;
        }
    }

    private void OnEnable()
    {
        if (toggle == null)
        {
            return;
        }

        toggle.onValueChanged.AddListener(UpdateColor);
        UpdateColor(toggle.isOn);
    }

    private void OnDisable()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(UpdateColor);
        }
    }

    private void UpdateColor(bool isOn)
    {
        if (targetGraphic != null)
        {
            targetGraphic.color = isOn ? onColor : offColor;
        }
    }
}
