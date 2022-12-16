using NUnit.Framework;
using RGN.Extensions;
using RGN.Impl.Firebase.Core;
using RGN.Modules.VirtualItems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace RGN.VirtualItems.Tests.Runtime
{
    [TestFixture]
    public class VirtualItemsTests
    {
        [OneTimeSetUp]
        public async void OneTimeSetup()
        {
            var applicationStore = ApplicationStore.I; //TODO: this will work only in editor.
            RGNCoreBuilder.AddModule(new VirtualItemModule(applicationStore.RGNStorageURL));
            var appOptions = new AppOptions()
            {
                ApiKey = applicationStore.RGNMasterApiKey,
                AppId = applicationStore.RGNMasterAppID,
                ProjectId = applicationStore.RGNMasterProjectId
            };

            await RGNCoreBuilder.Build(
                new RGN.Impl.Firebase.Dependencies(
                    appOptions,
                    applicationStore.RGNStorageURL),
                appOptions,
               applicationStore.RGNStorageURL,
               applicationStore.RGNAppId);

            if (applicationStore.usingEmulator)
            {
                RGNCore rgnCore = (RGNCore)RGNCoreBuilder.I;
                var firestore = rgnCore.readyMasterFirestore;
                string firestoreHost = applicationStore.emulatorServerIp + applicationStore.firestorePort;
                bool firestoreSslEnabled = false;
                firestore.UserEmulator(firestoreHost, firestoreSslEnabled);
                rgnCore.readyMasterFunction.UseFunctionsEmulator(applicationStore.emulatorServerIp + applicationStore.functionsPort);
                //TODO: storage, auth, realtime db
            }
        }

        [UnityTest]
        public IEnumerator GetVirtualItemsByIds_ReturnsItemsForExistingItems()
        {
            var ids = new List<string>()
            {
                "0e31cf69-d1c1-4b07-a756-63af82e124e6",
                "3ddc10ae-8a04-4f9f-9cf5-fc5b0e6b4cb8"
            };

            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().GetVirtualItemsByIds(ids);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for existing items");
            Assert.AreEqual(ids.Count, result.Count, "The count of result items does not equal the count of requested items");
        }
        [UnityTest]
        public IEnumerator GetVirtualItemsByIds_NoItemsForNonExistingItems()
        {
            var ids = new List<string>()
            {
                "non_existing_item_one",
                "non_existing_item_two"
            };

            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().GetVirtualItemsByIds(ids);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsEmpty(result, "Got non empty list for non existing items");
            Assert.AreEqual(0, result.Count);
        }
        [UnityTest]
        public IEnumerator GetVirtualItems_ReturnsItemsForCurrentApp()
        {
            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().GetVirtualItems();
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for current app: " + RGNCoreBuilder.I.AppIDForRequests);
        }
        [UnityTest]
        public IEnumerator GetAllVirtualItemsByAppIds_ReturnsItemsForRequestedApps()
        {
            List<string> appIds = new List<string>()
            {
                "io.getready.rgntest",
                "io.test.test"
            };

            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().GetAllVirtualItemsByAppIds(appIds);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for requested apps");
        }

        [UnityTest]
        public IEnumerator SetProperties_ReturnsPropertiesThatWasSet()
        {
            var virtualItemId = "92c7067d-cb58-4f3d-a545-36faf409d64c";
            var propertiesToSet = "{}";

            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().SetProperties(virtualItemId, propertiesToSet);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.AreEqual(propertiesToSet, result);
            UnityEngine.Debug.Log(result);
        }
        [UnityTest]
        public IEnumerator GetProperties_ReturnsPropertiesThatWasSetBeforeInDB()
        {
            var virtualItemId = "92c7067d-cb58-4f3d-a545-36faf409d64c";
            var expectedProperties = "{}";

            var task = RGNCoreBuilder.I.GetModule<VirtualItemModule>().GetProperties(virtualItemId);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.AreEqual(expectedProperties, result);
            UnityEngine.Debug.Log(result);
        }
    }
}
