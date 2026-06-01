// CreatureCardUI.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureCardUI : MonoBehaviour
{
    public Image portrait;
    public TMP_Text nameText;

    private CreatureDataBestiary data;
    private Action<CreatureDataBestiary> onClick;

    public void Setup(CreatureDataBestiary creature, Action<CreatureDataBestiary> callback)
    {
        data      = creature;
        onClick   = callback;
        portrait.sprite = creature.portrait;
        nameText.text   = creature.creatureName;
    }

    public void OnClick() => onClick?.Invoke(data); // bota no Button component
}