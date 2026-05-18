// CreatureCardUI.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureCardUI : MonoBehaviour
{
    public Image portrait;
    public TMP_Text nameText;

    private CreatureData data;
    private Action<CreatureData> onClick;

    public void Setup(CreatureData creature, Action<CreatureData> callback)
    {
        data      = creature;
        onClick   = callback;
        portrait.sprite = creature.portrait;
        nameText.text   = creature.creatureName;
    }

    public void OnClick() => onClick?.Invoke(data); // bota no Button component
}