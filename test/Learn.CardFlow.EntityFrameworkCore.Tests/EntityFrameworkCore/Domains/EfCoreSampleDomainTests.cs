using Learn.CardFlow.Samples;
using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore.Domains;

[Collection(CardFlowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<CardFlowEntityFrameworkCoreTestModule>
{

}
