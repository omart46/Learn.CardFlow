namespace Learn.CardFlow.Permissions;

public static class CardFlowPermissions
{
    public const string GroupName = "CardFlow";
    public static class Deliveries
    {
        public const string Default = GroupName + ".Deliveries";
        public const string Record = Default + ".Record";
    }
    public static class QualityChecks
    {
        public const string Default = GroupName + ".QualityChecks";
        public const string Create = Default + ".Create";
        public const string Inspect = Default + ".Inspect";
    }

    public static class ProductionBatches
    {
        public const string Default = GroupName + ".ProductionBatches";
        public const string Create = Default + ".Create";
        public const string ManageRequests = Default + ".ManageRequests";
        public const string Produce = Default + ".Produce";
    }


    
    public static class CardRequests
    {
        public const string Default = GroupName + ".CardRequests";
        public const string Create = Default + ".Create";
        public const string StartReview = Default + ".StartReview";
        public const string Decide = Default + ".Decide";
        public const string Edit = Default + ".Edit";
    }
}
