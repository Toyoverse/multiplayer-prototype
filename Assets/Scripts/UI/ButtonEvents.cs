using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonEvents : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private CARD_TYPE cardType;
        private const string Beats = " beats ";
        [SerializeField] private Button button;
        private ScriptsReferences Refs => ScriptsReferences.Instance;

        private void Start()
        {
            button ??= this.gameObject.GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            var log = Refs.globalConfig.roundInitMessage;
            log += "\n" + cardType + Beats + PerkSystem.GetWeakestType(cardType);
            ShowSimpleLogs.Instance.Log(log);
        }
    }
}
