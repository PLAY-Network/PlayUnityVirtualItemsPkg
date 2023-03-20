using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Store;
using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    public sealed class VirtualItemScreen : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _createdAtText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _createdByText;
        [SerializeField] private TextMeshProUGUI _updatedByText;
        [SerializeField] private TextMeshProUGUI _tagsText;
        [SerializeField] private TextMeshProUGUI _appIdsText;
        [SerializeField] private TextMeshProUGUI _childIdsText;
        [SerializeField] private TextMeshProUGUI _propertiesText;
        [SerializeField] private TextMeshProUGUI _isStackableText;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private IconImage _virtualItemIconImage;
        [SerializeField] private RectTransform _scrollRectContent;
        [SerializeField] private RectTransform _buyButtonsAnchor;

        [SerializeField] private RGNButton _actionButtonForBuyPrefab;

        private VirtualItem _virtualItem;
        private List<RGNButton> _buyButtons;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _buyButtons = new List<RGNButton>();
            _virtualItemIconImage.OnClick.AddListener(OnUploadNewProfilePictureButtonClickAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _virtualItemIconImage.OnClick.RemoveListener(OnUploadNewProfilePictureButtonClickAsync);
        }
        public override async void OnWillAppearNow(object parameters)
        {
            VirtualItem virtualItem = parameters as VirtualItem;
            _virtualItem = virtualItem;
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
            _tagsText.text = BuildStringFromStringsList(virtualItem.tags, "tags");
            _appIdsText.text = BuildStringFromStringsList(virtualItem.appIds, "app ids");
            _childIdsText.text = BuildStringFromStringsList(virtualItem.childs, "virtual item childs");
            _propertiesText.text = BuildStringFromPropertiesList(virtualItem.properties);
            InstantiateBuyButtonsForEachPrice(virtualItem.id, virtualItem.prices);
            _fullScreenLoadingIndicator.SetEnabled(false);
            await LoadIconImageAsync(_virtualItem.id, false);
        }

        private async Task LoadIconImageAsync(string virtualItemId, bool tryToloadFromCache)
        {
            _canvasGroup.interactable = false;
            _virtualItemIconImage.SetLoading(true);
            string localPath = Path.Combine(Application.persistentDataPath, "virtual_items", virtualItemId + ".png");
            Texture2D image = null;
            if (tryToloadFromCache)
            {
                if (File.Exists(localPath))
                {
                    byte[] bytes = File.ReadAllBytes(localPath);
                    image = new Texture2D(1, 1);
                    image.LoadImage(bytes);
                    image.Apply();
                }
            }
            if (image == null)
            {
                byte[] bytes = await VirtualItemsModule.I.DownloadImageAsync(virtualItemId);

                if (bytes != null)
                {
                    image = new Texture2D(1, 1);
                    image.LoadImage(bytes);
                    image.Apply();
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                    File.WriteAllBytes(localPath, bytes);
                }
            }
            _virtualItemIconImage.SetProfileTexture(image);
            _canvasGroup.interactable = true;
            _virtualItemIconImage.SetLoading(false);
        }
        private async void OnUploadNewProfilePictureButtonClickAsync()
        {
            _canvasGroup.interactable = false;
            _fullScreenLoadingIndicator.SetEnabled(true);
            var tcs = new TaskCompletionSource<bool>();
            NativeGallery.GetImageFromGallery(async path => {
                try
                {
                    if (path == null)
                    {
                        Debug.Log("User cancelled the image upload, or no permission granted");
                        tcs.TrySetResult(false);
                        return;
                    }
                    if (!File.Exists(path))
                    {
                        Debug.LogError("File does not exist at path: " + path);
                        tcs.TrySetResult(false);
                        return;
                    }
                    byte[] textureBytes = File.ReadAllBytes(path);
                    await VirtualItemsModule.I.UploadImageAsync(_virtualItem.id, textureBytes);
                    await LoadIconImageAsync(_virtualItem.id, false);
                    tcs.TrySetResult(true);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    tcs.TrySetException(ex);
                }
            },
            "Select Virtual Item Image");
            await tcs.Task;
            _fullScreenLoadingIndicator.SetEnabled(false);
            _canvasGroup.interactable = true;
        }

        private string BuildStringFromStringsList(List<string> strings, string name)
        {
            if (strings == null || strings.Count == 0)
            {
                return $"No {name} set";
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < strings.Count; i++)
            {
                sb.Append(strings[i]);
                if (i < strings.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
        private string BuildStringFromPropertiesList(List<Properties> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return "No properties set";
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < properties.Count; ++i)
            {
                var property = properties[i];
                sb.Append("Properties for apps: ");
                sb.Append(BuildStringFromStringsList(property.appIds, "app ids"));
                sb.AppendLine(" are set to: ");
                sb.AppendLine(string.IsNullOrWhiteSpace(property.json) ? "empty json" : property.json);
            }
            return sb.ToString();
        }
        private void InstantiateBuyButtonsForEachPrice(string virtualItemId, List<PriceInfo> priceInfos)
        {
            if (priceInfos == null || priceInfos.Count == 0)
            {
                return;
            }
            DisposeAllPriceButtons();
            float pricesTitleTextPosition = _buyButtonsAnchor.localPosition.y;
            float buttonsCurrentYPos = pricesTitleTextPosition;
            Dictionary<string, List<PriceInfo>> pricesByGroup = new Dictionary<string, List<PriceInfo>>();
            const float GAB = 8f;
            for (int i = 0; i < priceInfos.Count; ++i)
            {
                var priceInfo = priceInfos[i];
                if (!string.IsNullOrEmpty(priceInfo.group))
                {
                    if (pricesByGroup.TryGetValue(priceInfo.group, out var group))
                    {
                        group.Add(priceInfo);
                    }
                    else
                    {
                        pricesByGroup.Add(priceInfo.group, new List<PriceInfo>() { priceInfo });
                    }
                    continue;
                }
                RGNButton button = Instantiate(_actionButtonForBuyPrefab, _scrollRectContent);
                button.RectTransform.localPosition = new Vector2(16, buttonsCurrentYPos);
                buttonsCurrentYPos -= button.GetHeight() + GAB;
                button.ButtonText.text = priceInfo.ToDiscountPriceCurrencyString();
                button.Button.onClick.AddListener(async () => {
                    bool failed = false;
                    _canvasGroup.interactable = false;
                    _fullScreenLoadingIndicator.SetEnabled(true);
                    try
                    {
                        await StoreModule.I.BuyVirtualItemsAsync(
                            new List<string>() { virtualItemId },
                            new List<string>() { priceInfo.name });
                    }
                    catch (System.Exception ex)
                    {
                        failed = true;
                        Debug.LogException(ex);
                        ToastMessage.I.ShowError(ex.Message);
                    }
                    _fullScreenLoadingIndicator.SetEnabled(false);
                    _canvasGroup.interactable = true;
                    if (!failed)
                    {
                        ToastMessage.I.ShowSuccess("Successfully purchased virtual item with id: " + virtualItemId);
                    }
                });
                _buyButtons.Add(button);
            }
            foreach (var priceGroup in pricesByGroup)
            {
                RGNButton button = Instantiate(_actionButtonForBuyPrefab, _scrollRectContent);
                float buttonInitialHeight = _actionButtonForBuyPrefab.GetHeight();
                button.SetHeight(buttonInitialHeight * priceGroup.Value.Count);
                button.RectTransform.localPosition = new Vector2(16, buttonsCurrentYPos);
                buttonsCurrentYPos -= button.GetHeight() + GAB;
                var sb = new System.Text.StringBuilder();
                sb.Append("PriceGroup ").Append(priceGroup.Key).AppendLine(": ");
                var currencies = new List<string>();
                for (int i = 0; i < priceGroup.Value.Count; i++)
                {
                    var priceInfo = priceGroup.Value[i];
                    sb.Append(priceInfo.ToDiscountPriceCurrencyString());
                    if (i < priceGroup.Value.Count - 1)
                    {
                        sb.AppendLine(", ");
                    }
                    currencies.Add(priceInfo.name);
                }
                button.ButtonText.text = sb.ToString();
                button.Button.onClick.AddListener(async () => {
                    bool failed = false;
                    _canvasGroup.interactable = false;
                    _fullScreenLoadingIndicator.SetEnabled(true);
                    try
                    {
                        await StoreModule.I.BuyVirtualItemsAsync(
                            new List<string>() { virtualItemId },
                            currencies);
                    }
                    catch (System.Exception ex)
                    {
                        failed = true;
                        Debug.LogException(ex);
                        ToastMessage.I.ShowError(ex.Message);
                    }
                    _fullScreenLoadingIndicator.SetEnabled(false);
                    _canvasGroup.interactable = true;
                    if (!failed)
                    {
                        ToastMessage.I.ShowSuccess("Successfully purchased virtual item with id: " + virtualItemId);
                    }
                });
                _buyButtons.Add(button);
            }
        }
        private void DisposeAllPriceButtons()
        {
            for (int i = 0; i < _buyButtons.Count; ++i)
            {
                _buyButtons[i].Dispose();
            }
            _buyButtons.Clear();
        }
    }
}
