using UnityEngine;
using RGN.Impl.Firebase;
using RGN.UI;
using System.Collections.Generic;
using RGN.Modules.VirtualItems;
using System.Threading.Tasks;

namespace RGN.Samples
{
    public sealed class VirtualItemsExample : IUIScreen
    {
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _scrollContentRectTrasform;

        [SerializeField] private VirtualItemUI _virtualItemPrefab;

        private List<VirtualItemUI> _virtualItems;
        private bool _triedToLoad;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _virtualItems = new List<VirtualItemUI>();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async void OnShow()
        {
            if (_triedToLoad)
            {
                return;
            }
            await ReloadVirtualItemsAsync();
        }

        private void SetUIInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _fullScreenLoadingIndicator.SetEnabled(!interactable);
        }

        private async Task ReloadVirtualItemsAsync()
        {
            DisposeVirtualItems();
            SetUIInteractable(false);
            var virtualItems = await VirtualItemsModule.I.GetVirtualItemsAsync();
            for (int i = 0; i < virtualItems.Count; ++i)
            {
                VirtualItemUI ui = Instantiate(_virtualItemPrefab, _scrollContentRectTrasform);
                ui.Init(i, virtualItems[i]);
                _virtualItems.Add(ui);
            }
            Vector2 sizeDelta = _scrollContentRectTrasform.sizeDelta;
            _scrollContentRectTrasform.sizeDelta = new Vector2(sizeDelta.x, virtualItems.Count * _virtualItemPrefab.GetHeight());
            SetUIInteractable(true);
        }
        private void DisposeVirtualItems()
        {
            if (_virtualItems == null)
            {
                return;
            }
            for (int i = 0; i < _virtualItems.Count; ++i)
            {
                _virtualItems[i].Dispose();
            }
            _virtualItems.Clear();
        }
    }
}
