using TMPro;
using UnityEngine;

namespace Tools
{
    public class ShowSimpleLogs : Singleton<ShowSimpleLogs>
    {
        [Header("REFERENCES")] [SerializeField]
        private TextMeshProUGUI label;

        private void Start()
        {
            if (label == null)
                this.gameObject.GetComponent<TextMeshProUGUI>();
            label.text = "";
        }

        public void Log(string message)
        {
            Instance.label.text = message;
        }
    }
}