using RGN.Impl.Firebase;
using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    public sealed class VirtualItemScreen : IUIScreen
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _createdAtText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _createdByText;
        [SerializeField] private TextMeshProUGUI _updatedByText;
        [SerializeField] private TextMeshProUGUI _isStackableText;
        [SerializeField] private RawImage _virtualItemIconRawImage;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;


        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        public override void OnWillAppearNow(object parameters)
        {
            VirtualItem virtualItem = parameters as VirtualItem;
            if (virtualItem == null)
            {
                Debug.LogError("The provided virtual item is null or invalid");
                return;
            }
            _titleText.text = virtualItem.name;
            _descriptionText.text = virtualItem.description;
            _idText.text = virtualItem.id;
            _createdAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(virtualItem.createdAt);
            _updatedAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(virtualItem.updatedAt);
            _createdByText.text = virtualItem.createdBy;
            _updatedByText.text = virtualItem.updatedBy;
            _isStackableText.text = virtualItem.isStackable ? "Item is stackable" : "Item is not stackable";
            _fullScreenLoadingIndicator.SetEnabled(false);
        }
    }
}
