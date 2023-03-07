using System;
using System.Globalization;
using RGN.Modules.VirtualItems;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class VirtualItemUI : MonoBehaviour, System.IDisposable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        private VirtualItem _virtualItem;
        private bool _disposed = false;

        internal void Init(int index, VirtualItem virtualItem)
        {
            _virtualItem = virtualItem;
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
            _nameText.text = virtualItem.name;
            _updatedAtText.text = DateTimeToISOLikeStringNoMilliseconds(UnixTimeStampToDateTime(virtualItem.updatedAt));
            _descriptionText.text = virtualItem.description;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static string DateTimeToISOString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }
        public static string DateTimeToISOLikeStringNoMilliseconds(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss", CultureInfo.InvariantCulture);
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
            _disposed = true;
        }

        internal float GetHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}
