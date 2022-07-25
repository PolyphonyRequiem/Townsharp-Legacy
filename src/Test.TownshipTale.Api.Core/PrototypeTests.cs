using TownshipTale.Api.Core;

namespace Test.TownshipTale.Api.Core
{
    public class PrototypeTests
    {
        [Fact]
        public void ScopesWorkAsExpected()
        {
            //Assert Implicit String Equal
            Assert.True(Scope.GroupInfo == "group.info");
            Assert.True("group.info" == Scope.GroupInfo);
        }
    }
}
