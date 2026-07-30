# Test Execution Evidence — EComLite

- **Date (UTC):** 2026-07-30 01:56:19
- **Command:** `dotnet test EComLite.Tests/EComLite.Tests.csproj`
- **.NET SDK:** 8.0.200
- **OS:** Windows 11 (local); GitHub Actions ubuntu-latest in CI
- **Working tree base commit:** b48220352d9f3bc514da3a85869a46786a649783 (plus the pending idempotency / persistent-cart / access-control changes)
- **Result:** 46 passed, 0 failed, 46 total

## AccessControlTests (5/5 passed)

- [PASS] EComLite.Tests.AccessControlTests.OrderDetails_OtherUsersOrder_IsNotAccessible
- [PASS] EComLite.Tests.AccessControlTests.OrderDetails_OwnOrder_IsAccessible
- [PASS] EComLite.Tests.AccessControlTests.ProductDetails_ArchivedProduct_ReturnsNotFound
- [PASS] EComLite.Tests.AccessControlTests.ProductDetails_LiveProduct_ReturnsPage
- [PASS] EComLite.Tests.AccessControlTests.ProductDetails_MissingProduct_ReturnsNotFound

## CartServiceTests (9/9 passed)

- [PASS] EComLite.Tests.CartServiceTests.AddItem_DifferentProducts_AddsBothToCart
- [PASS] EComLite.Tests.CartServiceTests.AddItem_NewProduct_AddsToCart
- [PASS] EComLite.Tests.CartServiceTests.AddItem_PriceAndQty_TotalCalculatedCorrectly
- [PASS] EComLite.Tests.CartServiceTests.AddItem_SameProductTwice_AccumulatesQty
- [PASS] EComLite.Tests.CartServiceTests.Clear_EmptyCart_RemainsEmpty
- [PASS] EComLite.Tests.CartServiceTests.Clear_WithItems_EmptiesCart
- [PASS] EComLite.Tests.CartServiceTests.GetCart_WhenSessionEmpty_ReturnsEmptyList
- [PASS] EComLite.Tests.CartServiceTests.Remove_ExistingProduct_RemovesFromCart
- [PASS] EComLite.Tests.CartServiceTests.Remove_NonExistentProduct_CartUnchanged

## CheckoutServiceTests (3/3 passed)

- [PASS] EComLite.Tests.CheckoutServiceTests.DifferentIdempotencyKeys_CreateTwoOrders
- [PASS] EComLite.Tests.CheckoutServiceTests.PlacedOrder_HasCorrectTotalStatusAndKey
- [PASS] EComLite.Tests.CheckoutServiceTests.SameIdempotencyKey_SubmittedTwice_CreatesOnlyOneOrder

## CheckoutTests (7/7 passed)

- [PASS] EComLite.Tests.CheckoutTests.ArchivedProduct_NotReturnedInCatalog
- [PASS] EComLite.Tests.CheckoutTests.OrderHistory_OnlyReturnsOrdersForCorrectUser
- [PASS] EComLite.Tests.CheckoutTests.PlaceOrder_EveryOrderHasAtLeastOneItem
- [PASS] EComLite.Tests.CheckoutTests.PlaceOrder_OrderItemCountMatchesCartItems
- [PASS] EComLite.Tests.CheckoutTests.PlaceOrder_PriceSnapshotPreserved_AfterProductPriceChange
- [PASS] EComLite.Tests.CheckoutTests.PlaceOrder_TotalAmountMatchesSumOfItems
- [PASS] EComLite.Tests.CheckoutTests.PlaceOrder_ValidItems_OrderSavedToDatabase

## OrderNumberTests (5/5 passed)

- [PASS] EComLite.Tests.OrderNumberTests.GenerateOrderNumber_ContainsFourCharSuffix
- [PASS] EComLite.Tests.OrderNumberTests.GenerateOrderNumber_DatePartMatchesPlacedAt
- [PASS] EComLite.Tests.OrderNumberTests.GenerateOrderNumber_DifferentOrderIds_ProduceDifferentNumbers
- [PASS] EComLite.Tests.OrderNumberTests.GenerateOrderNumber_ReturnsCorrectFormat
- [PASS] EComLite.Tests.OrderNumberTests.GenerateOrderNumber_SuffixIsUpperCase

## OrderStatusTransitionTests (13/13 passed)

- [PASS] EComLite.Tests.OrderStatusTransitionTests.DeliveredIsTerminal
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Delivered", to: "Shipped")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Pending", to: "Delivered")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Pending", to: "Shipped")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Processing", to: "Delivered")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Processing", to: "Pending")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.InvalidTransition_IsRejected(from: "Shipped", to: "Processing")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.NewOrdersStartPending
- [PASS] EComLite.Tests.OrderStatusTransitionTests.SameStatus_IsRejected
- [PASS] EComLite.Tests.OrderStatusTransitionTests.UnknownTargetStatus_IsRejected
- [PASS] EComLite.Tests.OrderStatusTransitionTests.ValidForwardTransition_IsAllowed(from: "Pending", to: "Processing")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.ValidForwardTransition_IsAllowed(from: "Processing", to: "Shipped")
- [PASS] EComLite.Tests.OrderStatusTransitionTests.ValidForwardTransition_IsAllowed(from: "Shipped", to: "Delivered")

## PersistentCartServiceTests (4/4 passed)

- [PASS] EComLite.Tests.PersistentCartServiceTests.Clear_RemovesPersistedCart
- [PASS] EComLite.Tests.PersistentCartServiceTests.Load_UnknownUser_ReturnsEmpty
- [PASS] EComLite.Tests.PersistentCartServiceTests.Save_ThenLoad_ReturnsSameItems
- [PASS] EComLite.Tests.PersistentCartServiceTests.Save_Twice_OverwritesAndKeepsOneRowPerUser
