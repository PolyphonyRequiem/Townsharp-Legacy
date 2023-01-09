using Townsharp.Api;
using Townsharp.Infra.Alta.Api;

namespace Test.Townsharp.Infra
{
    public class ApiClientTests
    {
        ApiClient apiClient;

        public ApiClientTests(ApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        //[Fact]
        //public async Task GetServerInfoTest()
        //{
        //    var serverInfo = await this.apiClient.GetServerInfo(1174503463);
        //    Assert.True(serverInfo.Name=="Cairnbrook");
        //}

        //[Fact]
        //public async Task GetServerConnectionInfoTest()
        //{
        //    var serverConnectionInfo = await this.apiClient.GetConsoleInfo(1174503463);
        //    Assert.True(serverConnectionInfo.ServerId == 1174503463);
        //}

        //[Fact]
        //public async Task GetPendingGroupInvitesTest()
        //{
        //    var groupInvites = await this.apiClient.GetPendingGroupInvites();
        //}

        [Fact]
        public async Task GetJoinedGroupsTest()
        {
            await foreach (var joinedGroup in this.apiClient.GetJoinedGroupDescriptions())
            {
                Assert.True(joinedGroup.Id != 0);
                break;
            }
        }

        //[Fact]
        //public async Task GetGroupMemberTest()
        //{
        //    var member = await this.apiClient.GetGroupMember(1156211297, 861317881);
        //    Assert.True(member.Username == "Polyphony");
        //}

        //[Fact]
        //public async Task GetGroupTest()
        //{
        //    var group = await this.apiClient.GetGroupInfo(1156211297);
        //    Assert.True(group.Name == "Cairnbrook");
        //}

        [Fact]
        public async Task AcceptGroupInviteTest()
        {
            var result = await this.apiClient.AcceptGroupInvite(1156211297);
            Assert.False(result);
        }
    }
}