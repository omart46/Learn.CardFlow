using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore;

[CollectionDefinition(CardFlowTestConsts.CollectionDefinitionName)]
public class CardFlowEntityFrameworkCoreCollection : ICollectionFixture<CardFlowEntityFrameworkCoreFixture>
{

}
