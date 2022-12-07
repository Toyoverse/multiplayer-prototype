using System;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonEvents : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private CARD_TYPE cardType;
        private const string beats = " beats ";
        [SerializeField] private Button button;

        private void Start()
        {
            button ??= this.gameObject.GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            var log = LocalGameManager.roundInitMessage;
            log += "\n" + cardType + beats + PerkSystem.GetWeakestType(cardType);
            ShowSimpleLogs.Instance.Log(log);
        }
    }
}
