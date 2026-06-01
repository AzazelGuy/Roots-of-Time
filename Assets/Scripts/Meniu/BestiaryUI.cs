// BestiaryUI.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUI : MonoBehaviour
{
    [Header("Criaturas")]
    public List<CreatureDataBestiary> allCreatures;

    [Header("Grid")]
    public Transform gridParent;
    public GameObject cardPrefab;

    [Header("Painel de Detalhe")]
    public GameObject detailPanel;
    public Image detailPortrait;
    public TMP_Text detailName, detailDescription, detailStats;

    void OnEnable()
    {
        PopulateGrid();
        detailPanel.SetActive(false);
    }

    void PopulateGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var creature in allCreatures)
        {
            var card = Instantiate(cardPrefab, gridParent);
            card.GetComponent<CreatureCardUI>().Setup(creature, ShowDetail);
        }
    }

    void ShowDetail(CreatureDataBestiary c)
    {
        detailPanel.SetActive(true);
        detailPortrait.sprite  = c.portrait;
        detailName.text        = c.creatureName;
        detailDescription.text = c.description;
        detailStats.text       = $"Vida {c.health}    Dano {c.damage}    Local {c.habitat}";
    }

    public void CloseDetail() => detailPanel.SetActive(false);
}