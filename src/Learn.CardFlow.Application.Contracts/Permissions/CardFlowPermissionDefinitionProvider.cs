using Learn.CardFlow.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Learn.CardFlow.Permissions;

public class CardFlowPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            CardFlowPermissions.GroupName, L("Permission:CardFlow"));

        var batches = group.AddPermission(
            CardFlowPermissions.ProductionBatches.Default, L("Permission:ProductionBatches"));
        var deliveries = group.AddPermission(CardFlowPermissions.Deliveries.Default, L("Permission:Deliveries"));
        deliveries.AddChild(CardFlowPermissions.Deliveries.Record, L("Permission:Deliveries.Record"));
        var qualityChecks = group.AddPermission(
            CardFlowPermissions.QualityChecks.Default, L("Permission:QualityChecks"));
        qualityChecks.AddChild(CardFlowPermissions.QualityChecks.Create,
            L("Permission:QualityChecks.Create"));
        qualityChecks.AddChild(CardFlowPermissions.QualityChecks.Inspect,
            L("Permission:QualityChecks.Inspect"));
        batches.AddChild(CardFlowPermissions.ProductionBatches.Create,
            L("Permission:ProductionBatches.Create"));
        batches.AddChild(CardFlowPermissions.ProductionBatches.ManageRequests,
            L("Permission:ProductionBatches.ManageRequests"));
        batches.AddChild(CardFlowPermissions.ProductionBatches.Produce,
            L("Permission:ProductionBatches.Produce"));

        var cardRequests = group.AddPermission(
            CardFlowPermissions.CardRequests.Default, L("Permission:CardRequests"));

        cardRequests.AddChild(
            CardFlowPermissions.CardRequests.Create, L("Permission:CardRequests.Create"));

        cardRequests.AddChild(
            CardFlowPermissions.CardRequests.StartReview, L("Permission:CardRequests.StartReview"));

        cardRequests.AddChild(
            CardFlowPermissions.CardRequests.Decide, L("Permission:CardRequests.Decide"));

        cardRequests.AddChild(
            CardFlowPermissions.CardRequests.Edit, L("Permission:CardRequests.Edit"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CardFlowResource>(name);
    }
}
