using UnityEngine;
using UnityEngine.UI;

public class ProjectCardUI : MonoBehaviour
{
    [Header("UI Baðlantýlarý")]
    [SerializeField] private Image cardIcon;
    [SerializeField] private Text cardTitle;
    [SerializeField] private Text cardDescription;
    [SerializeField] private Button actionButton;

    public void SetupCard(LinkItem data)
    {
        cardTitle.text = data.title;
        cardDescription.text = data.description;

        if (data.icon != null)
        {
            cardIcon.sprite = data.icon;
            cardIcon.gameObject.SetActive(true);
        }
        else
        {
            cardIcon.gameObject.SetActive(false);
        }

        actionButton.onClick.RemoveAllListeners();

        if (!string.IsNullOrEmpty(data.url))
        {
            actionButton.onClick.AddListener(() =>
            {
                Debug.Log("Linke gidiliyor: " + data.url);
                Application.OpenURL(data.url);
            });
        }
    }
}