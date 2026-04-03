using System.Collections.Generic;
using UnityEngine;
using ProjectAvalanche.Data; 

[System.Serializable]
public class LinkItem
{
    public Sprite icon;          // YENÝ: Projenin veya Linkin Görseli/Ýkonu
    public string title;         
    [TextArea(2, 3)]
    public string description;   
    public string url;           
}

[CreateAssetMenu(fileName = "NewLinkCollection", menuName = "Project Avalanche/Link Collection Data")]
public class LinkCollectionData : ScriptableObject
{
    public CVSection sectionType;           
    public string headerTitle;              
    public List<LinkItem> items;            
}