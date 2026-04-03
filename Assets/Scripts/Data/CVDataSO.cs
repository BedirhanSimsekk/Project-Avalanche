using UnityEngine;

namespace ProjectAvalanche.Data
{
    // Bu satýr, Unity'nin sað týk menüsüne yeni bir seçenek ekler
    [CreateAssetMenu(fileName = "NewCVData", menuName = "Project Avalanche/CV Data")]
    public class CVDataSO : ScriptableObject
    {
        [Header("Eþleþtirme Türü")]
        public CVSection sectionType;

        [Header("UI Ýçerikleri")]
        public string title;
        
        // TextArea, Inspector'da metin girmek için geniþ bir kutu saðlar
        [TextArea(5, 10)] 
        public string content;
    }
}