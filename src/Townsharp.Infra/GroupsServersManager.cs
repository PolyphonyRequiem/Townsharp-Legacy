using Townsharp.Groups;
using Townsharp.Infra.Alta.Api;

namespace Townsharp.Infra
{
    public class AltaGroupsManager : GroupsManager
    {
        private readonly ApiClient apiClient;

        public AltaGroupsManager(ApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        public async override Task<GroupId[]> GetJoinedGroups()
        {
            var joinedGroups = await apiClient.GetJoinedGroups();

            var joinedGroupIds = joinedGroups
                .Select(joinedGroup => new GroupId(joinedGroup.Group.Id))
                .ToArray();

            return joinedGroupIds;
        }
    }
}
