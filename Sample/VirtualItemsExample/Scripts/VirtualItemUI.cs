using RGN.Modules.VirtualItems;
using RGN.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    internal sealed class VirtualItemUI : MonoBehaviour, System.IDisposable
    {
        public string Id { get => _virtualItem.id; }

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [SerializeField] private Button _openVirtualItemScreenButton;

        private VirtualItem _virtualItem;
        private bool _disposed = false;

        internal void Init(int index, VirtualItem virtualItem)
        {
            _virtualItem = virtualItem;
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
            _nameText.text = virtualItem.name;
            _updatedAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(virtualItem.updatedAt);
            _descriptionText.text = virtualItem.description;
            _openVirtualItemScreenButton.onClick.AddListener(OnOpenVirtualItemScreenButtonClick);
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            _openVirtualItemScreenButton.onClick.RemoveListener(OnOpenVirtualItemScreenButtonClick);
            _disposed = true;
        }

        internal float GetHeight()
        {
            return _rectTransform.sizeDelta.y;
        }

        private void OnOpenVirtualItemScreenButtonClick()
        {
            Debug.Log("OnOpenVirtualItemScreenButtonClick");
        }
    }
}
