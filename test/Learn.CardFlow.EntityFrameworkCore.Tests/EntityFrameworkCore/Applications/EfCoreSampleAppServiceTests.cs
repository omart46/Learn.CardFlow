using Learn.CardFlow.Samples;
using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore.Applications;

[Collection(CardFlowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<CardFlowEntityFrameworkCoreTestModule>
{

}
