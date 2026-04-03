using UnityEngine;
using UnityEngine.UI;
using ProjectAvalanche.Data;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Normal CV Paneli (Eðitim, Yetenek, Deneyim)")]
    [SerializeField] private GameObject cvPanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text contentText;
    [SerializeField] private CVDataSO[] cvDataCollection;

    [Header("Ýnteraktif Panel (Projeler, Linkler)")]
    [SerializeField] private GameObject interactivePanel;
    [SerializeField] private Text interactiveHeaderTitle; // (Eðer TextMeshPro kullandýysan: TMPro.TextMeshProUGUI yap)
    [SerializeField] private Transform cardContainer;
    [SerializeField] private ProjectCardUI projectCardPrefab;
    [SerializeField] private LinkCollectionData[] linkDataCollection;

    [Header("Animasyon Ayarlarý")]
    [SerializeField] private float animOpenDuration = 0.4f;
    [SerializeField] private float animCloseDuration = 0.4f;

    private GameObject _activePanel; // Ekranda o an hangi panel açýksa onu aklýnda tutar

    private void OnEnable()
    {
        StationTrigger.OnStationUnlocked += HandleStationUnlocked;
    }

    private void OnDisable()
    {
        StationTrigger.OnStationUnlocked -= HandleStationUnlocked;
    }

    private void Start()
    {
        // Ýki paneli de oyun baþýnda görünmez yap
        cvPanel.transform.localScale = Vector3.zero;
        cvPanel.SetActive(false);

        if (interactivePanel != null)
        {
            interactivePanel.transform.localScale = Vector3.zero;
            interactivePanel.SetActive(false);
        }
    }

    private void HandleStationUnlocked(CVSection section)
    {
        // 1. Önce gelen veri "Normal CV Verisi" mi diye bak
        foreach (var data in cvDataCollection)
        {
            if (data.sectionType == section)
            {
                titleText.text = data.title;
                contentText.text = data.content;
                OpenPanel(cvPanel);
                return; // Bulduysa iþlemi burada bitir
            }
        }

        // 2. Normal veri deðilse, demek ki "Link/Proje" verisidir, onlara bak
        foreach (var linkData in linkDataCollection)
        {
            if (linkData.sectionType == section)
            {
                // Ana baþlýðý yaz (Örn: YAYINLANAN OYUNLARIM)
                interactiveHeaderTitle.text = linkData.headerTitle;

                // Kutunun içindeki eski kartlarý temizle (Eðer önceden baþka soru iþaretine girdiyse)
                foreach (Transform child in cardContainer)
                {
                    Destroy(child.gameObject);
                }

                // Data dosyasýndaki her bir item (oyun veya link) için 1 tane Kart kopyala!
                foreach (var item in linkData.items)
                {
                    ProjectCardUI newCard = Instantiate(projectCardPrefab, cardContainer);
                    newCard.SetupCard(item); // Kartýn içine veriyi yolla
                }

                OpenPanel(interactivePanel);
                return; // Bulduysa iþlemi bitir
            }
        }
    }

    private void OpenPanel(GameObject panelToOpen)
    {
        _activePanel = panelToOpen;
        _activePanel.SetActive(true);
        _activePanel.transform.DOKill();
        _activePanel.transform.localScale = Vector3.zero;
        _activePanel.transform.DOScale(Vector3.one, animOpenDuration).SetEase(Ease.OutBack);
    }

    public void ClosePanel()
    {
        if (_activePanel == null) return;

        _activePanel.transform.DOKill();
        _activePanel.transform.DOScale(Vector3.zero, animCloseDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _activePanel.SetActive(false);
            _activePanel = null;
        });
    }
}