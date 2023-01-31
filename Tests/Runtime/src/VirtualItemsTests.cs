using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RGN.Extensions;
using RGN.Modules.VirtualItems;
using RGN.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.Networking.UnityWebRequest;

namespace RGN.VirtualItems.Tests.Runtime
{
    public class VirtualItemsTests : BaseTests
    {
        private static readonly List<bool> _addVirtualItemStackableOptions = new List<bool> { true, false };
        private static readonly List<bool> _updateVirtualItemStackableOptions = new List<bool> { true, false };

        [UnityTest]
        public IEnumerator AddVirtualItem_WorksOnlyForAdminUsers([ValueSource("_addVirtualItemStackableOptions")] bool isStackable)
        {
            yield return LoginAsAdminTester();

            var virtualItem = new VirtualItem() {
                name = "Play Test Item",
                description = "Created in Unity play tests",
                appIds = new List<string>() { RGNCoreBuilder.I.AppIDForRequests },
                isStackable = isStackable,
            };

            var task = VirtualItemsModule.I.AddVirtualItemAsync(virtualItem);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            UnityEngine.Debug.Log("Added new virtual item: " + result.id);
            CheckVirtualItemFields_CreatedUpdated(result);
        }

        [UnityTest]
        public IEnumerator AddVirtualItem_FailsForNormalUsers([ValueSource("_addVirtualItemStackableOptions")] bool isStackable)
        {
            yield return LoginAsNormalTester();

            var virtualItem = new VirtualItem() {
                name = "Play Test Item",
                description = "Created in Unity play tests",
                appIds = new List<string>() { RGNCoreBuilder.I.AppIDForRequests },
                isStackable = isStackable,
            };

            var task = VirtualItemsModule.I.AddVirtualItemAsync(virtualItem);
            yield return task.AsIEnumeratorReturnNullDontThrow();

            // TODO: uncomment when the user roles are implemented
            //Assert.True(task.IsFaulted, "Virtual item was added to db even with normal user account. Only admins or creators can add new items");
        }

        [UnityTest]
        public IEnumerator UpdateVirtualItem_DocExist_AdminUsers([ValueSource("_updateVirtualItemStackableOptions")] bool isStackable)
        {
            yield return LoginAsAdminTester();

            string itemId = "0e31cf69-d1c1-4b07-a756-63af82e124e6";

            var virtualItem = new VirtualItem() {
                id = itemId,
                name = "Play Test Item",
                description = "Created in Unity play tests",
                appIds = new List<string>() { RGNCoreBuilder.I.AppIDForRequests },
                isStackable = isStackable,
            };

            var task = VirtualItemsModule.I.UpdateVirtualItemAsync(itemId, virtualItem);
            yield return task.AsIEnumeratorReturnNull();

            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            UnityEngine.Debug.Log("Updated virtual item: " + result.id);
            CheckVirtualItemFields_CreatedUpdated(result);
        }

        [UnityTest]
        public IEnumerator UpdateVirtualItem_DocIsNotExist_AdminUsers([ValueSource("_updateVirtualItemStackableOptions")] bool isStackable)
        {
            yield return LoginAsAdminTester();

            string itemId = "999999-9999-99999-9999-999999999";

            var virtualItem = new VirtualItem() {
                id = itemId,
                name = "Play Test Item",
                description = "Created in Unity play tests",
                appIds = new List<string>() { RGNCoreBuilder.I.AppIDForRequests },
                isStackable = isStackable,
            };

            var task = VirtualItemsModule.I.UpdateVirtualItemAsync(itemId, virtualItem);
            yield return task.AsIEnumeratorReturnNullDontThrow();

            Assert.IsTrue(task.IsFaulted, "The item is updated, item id: " + virtualItem.id);
            UnityEngine.Debug.Log("Virtual item is not exist: " + virtualItem.id);
        }

        [UnityTest]
        public IEnumerator GetVirtualItemsByIds_ReturnsItemsForExistingItems()
        {
            yield return LoginAsNormalTester();

            var ids = new List<string>()
            {
                "0e31cf69-d1c1-4b07-a756-63af82e124e6",
                "3ddc10ae-8a04-4f9f-9cf5-fc5b0e6b4cb8"
            };

            var task = VirtualItemsModule.I.GetVirtualItemsByIdsAsync(ids);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for existing items");
            Assert.AreEqual(ids.Count, result.Count, "The count of result items does not equal the count of requested items");

            if (result != null)
            {
                foreach(VirtualItem item in result)
                {
                    CheckVirtualItemFields_CreatedUpdated(item);
                }
            }
        }
        [UnityTest]
        public IEnumerator GetVirtualItemsByIds_NoItemsForNonExistingItems()
        {
            yield return LoginAsNormalTester();

            var ids = new List<string>()
            {
                "non_existing_item_one",
                "non_existing_item_two"
            };

            var task = VirtualItemsModule.I.GetVirtualItemsByIdsAsync(ids);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsEmpty(result, "Got non empty list for non existing items");
            Assert.AreEqual(0, result.Count);

            if (result != null)
            {
                foreach (VirtualItem item in result)
                {
                    CheckVirtualItemFields_CreatedUpdated(item);
                }
            }
        }
        [UnityTest]
        public IEnumerator GetVirtualItemsByIds_ReturnsOnlyExistingItems()
        {
            yield return LoginAsNormalTester();

            var ids = new List<string>()
            {
                "non_existing_item_one",
                "0e31cf69-d1c1-4b07-a756-63af82e124e6",
                "non_existing_item_two",
                "3ddc10ae-8a04-4f9f-9cf5-fc5b0e6b4cb8"
            };

            var task = VirtualItemsModule.I.GetVirtualItemsByIdsAsync(ids);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for existing items");
            Assert.AreEqual(2, result.Count);

            if (result != null)
            {
                foreach (VirtualItem item in result)
                {
                    CheckVirtualItemFields_CreatedUpdated(item);
                }
            }
        }

        [UnityTest]
        public IEnumerator GetVirtualItems_ReturnsItemsForCurrentApp()
        {
            yield return LoginAsNormalTester();

            var task = VirtualItemsModule.I.GetVirtualItemsAsync();
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for current app: " + RGNCoreBuilder.I.AppIDForRequests);

            if (result != null)
            {
                foreach (VirtualItem item in result)
                {
                    CheckVirtualItemFields_CreatedUpdated(item);
                }
            }
        }

        [UnityTest]
        public IEnumerator GetAllVirtualItemsByAppIds_ReturnsItemsForRequestedApps()
        {
            yield return LoginAsNormalTester();

            List<string> appIds = new List<string>()
            {
                "io.getready.rgntest",
                "io.test.test"
            };

            var task = VirtualItemsModule.I.GetAllVirtualItemsByAppIdsAsync(appIds, 1000);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.IsNotEmpty(result, "Got empty list for requested apps");

            if (result != null)
            {
                foreach (VirtualItem item in result)
                {
                    CheckVirtualItemFields_CreatedUpdated(item);
                }
            }
        }
        
        [UnityTest]
        public IEnumerator GetByTags_ReturnsArrayOfOffers()
        {
            yield return LoginAsAdminTester();
            
            var tagsToFind = new[] { "tagToFind" };
            
            var getVirtualItemsByTagsTask = VirtualItemsModule.I.GetByTagsAsync(tagsToFind);
            yield return getVirtualItemsByTagsTask.AsIEnumeratorReturnNull();
            var getVirtualItemsByTagsResult = getVirtualItemsByTagsTask.Result;
            
            Assert.IsNotEmpty(getVirtualItemsByTagsResult);

            var areVirtualItemsTagsCorrect =
                tagsToFind.Any(x => getVirtualItemsByTagsResult[0].tags.Contains(x));

            Assert.True(areVirtualItemsTagsCorrect, "Retrieved virtual items doesn't contains any requested tag");

            var noDuplicates = getVirtualItemsByTagsResult
                .GroupBy(x => x.id)
                .Any(g => g.Count() <= 1);

            Assert.True(noDuplicates, "Request returns duplicated virtual items");
        }

        [UnityTest]
        public IEnumerator GetTags_ReturnsArrayOfOfferTags()
        {
            yield return LoginAsNormalTester();

            // specially created item for tests
            var virtualItemId = "ed589211-466b-4d87-9c94-e6ba03a10765";

            var getVirtualItemTagsTask = VirtualItemsModule.I.GetTagsAsync(virtualItemId);
            yield return getVirtualItemTagsTask.AsIEnumeratorReturnNull();
            var getVirtualItemTagsResult = getVirtualItemTagsTask.Result;

            Assert.NotNull(getVirtualItemTagsResult.tags, "Array of tags is null");
        }

        [UnityTest]
        public IEnumerator SetTags_ChecksSetTags()
        {
            yield return LoginAsAdminTester();

            // specially created item for tests
            var virtualItemId = "ed589211-466b-4d87-9c94-e6ba03a10765";
            var newTags = new[]
            {
                "tag1" + UnityEngine.Random.Range(0, 1000),
            };
            string appId = "io.getready.rgntest";

            var setTagsTask = VirtualItemsModule.I.SetTagsAsync(virtualItemId, newTags, appId);
            yield return setTagsTask.AsIEnumeratorReturnNull();

            var getVirtualItemTagsTask = VirtualItemsModule.I.GetTagsAsync(virtualItemId);
            yield return getVirtualItemTagsTask.AsIEnumeratorReturnNull();
            var getVirtualItemTagsResult = getVirtualItemTagsTask.Result;

            var tagsAreEqual = newTags.Length == getVirtualItemTagsResult.tags.Length;
            if (tagsAreEqual)
            {
                for (var i = 0; i < newTags.Length; i++)
                {
                    string expectedTag = $"{newTags[i]}_{appId}";
                    string actualTag = getVirtualItemTagsResult.tags[i];
                    
                    if (expectedTag.Equals(actualTag))
                    {
                        continue;
                    }
                    tagsAreEqual = false;
                    break;
                }
            }
            Assert.True(tagsAreEqual, "Tags field didn't set properly");
        }

        [UnityTest]
        public IEnumerator SetName_ChecksSetName()
        {
            yield return LoginAsAdminTester();

            // specially created item for tests
            var virtualItemId = "ed589211-466b-4d87-9c94-e6ba03a10765";
            var newName = "Name" + UnityEngine.Random.Range(0, 1000);

            var setNameTask = VirtualItemsModule.I.SetNameAsync(virtualItemId, newName);
            yield return setNameTask.AsIEnumeratorReturnNull();

            var getVirtualItemsTask = VirtualItemsModule.I.GetVirtualItemsByIdsAsync(new List<string> { virtualItemId });
            yield return getVirtualItemsTask.AsIEnumeratorReturnNull();
            var getVirtualItemsResult = getVirtualItemsTask.Result;

            Assert.NotNull(getVirtualItemsResult, "Array of virtual items is null");
            Assert.IsNotEmpty(getVirtualItemsResult, "Array of virtual items is empty");
            Assert.AreEqual(newName, getVirtualItemsResult[0].name, "Name field didn't set properly");
        }

        [UnityTest]
        public IEnumerator SetDescription_ChecksSetName()
        {
            yield return LoginAsAdminTester();

            // specially created item for tests
            var virtualItemId = "ed589211-466b-4d87-9c94-e6ba03a10765";
            var newDescription = "Description" + UnityEngine.Random.Range(0, 1000);

            var setDescriptionTask = VirtualItemsModule.I.SetDescriptionAsync(virtualItemId, newDescription);
            yield return setDescriptionTask.AsIEnumeratorReturnNull();

            var getVirtualItemsTask = VirtualItemsModule.I.GetVirtualItemsByIdsAsync(new List<string> { virtualItemId });
            yield return getVirtualItemsTask.AsIEnumeratorReturnNull();
            var getVirtualItemsResult = getVirtualItemsTask.Result;

            Assert.NotNull(getVirtualItemsResult, "Array of virtual items is null");
            Assert.IsNotEmpty(getVirtualItemsResult, "Array of virtual items is empty");
            Assert.AreEqual(newDescription, getVirtualItemsResult[0].description, "Description field didn't set properly");
        }

        [UnityTest]
        public IEnumerator SetProperties_ReturnsPropertiesThatWasSet()
        {
            yield return LoginAsAdminTester();

            var virtualItemId = "92c7067d-cb58-4f3d-a545-36faf409d64c";
            var propertiesToSet = "{}";

            var task = VirtualItemsModule.I.SetPropertiesAsync(virtualItemId, propertiesToSet);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.AreEqual(propertiesToSet, result);
            UnityEngine.Debug.Log(result);
        }

        [UnityTest]
        public IEnumerator GetProperties_ReturnsPropertiesThatWasSetBeforeInDB()
        {
            yield return LoginAsNormalTester();

            var virtualItemId = "92c7067d-cb58-4f3d-a545-36faf409d64c";
            var expectedProperties = "{}";

            var task = VirtualItemsModule.I.GetPropertiesAsync(virtualItemId);
            yield return task.AsIEnumeratorReturnNull();
            var result = task.Result;

            Assert.NotNull(result, "The result is null");
            Assert.AreEqual(expectedProperties, result);
            UnityEngine.Debug.Log(result);
        }

        // TODO: [UnityTest]
        public IEnumerator DownloadVirtualItemThumbnail_CastToUnityTexture2DWorks()
        {
            yield return LoginAsNormalTester();

            var virtualItemId = "92c7067d-cb58-4f3d-a545-36faf409d64c";
            var task = VirtualItemsModule.I.DownloadThumbnailAsync<Texture2D>(virtualItemId);
            yield return task.AsIEnumeratorReturnNull();

            Texture2D result = task.Result;
            Debug.Log(result.width + ":" + result.height);
        }

        // helper function for checking of VirtualItems fields: createdAt, updatedAt, createdBy, updatedBy
        private void CheckVirtualItemFields_CreatedUpdated(VirtualItem item)
        {
            if (item == null)
                return;

            if (!string.IsNullOrEmpty(item.createdAt))
            {
                DateTime dt;
                bool result = DateTime.TryParse(item.createdAt, out dt);
                Assert.IsTrue(result, "createdAt has wrong format for Item " + item.id);
            }
            else
            {
                Assert.Fail("createdAt is Null for Item " + item.id);
            }

            if (!string.IsNullOrEmpty(item.updatedAt))
            {
                DateTime dt;
                bool result = DateTime.TryParse(item.updatedAt, out dt);
                Assert.IsTrue(result, "updatedAt has wrong format for Item " + item.id);
            }
            else
            {
                Assert.Fail("updatedAt is Null for Item " + item.id);
            }

            Assert.IsFalse(string.IsNullOrEmpty(item.createdBy), "createdBy is Null or Empty for Item " + item.id);
            Assert.IsFalse(string.IsNullOrEmpty(item.updatedBy), "updatedBy is Null or Empty for Item " + item.id);
        }

    }
}
