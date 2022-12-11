using System.Collections.ObjectModel;
using Townsharp.Api;

namespace Townsharp.Groups
{
    public class GroupManager
    {
        private readonly IApiClient apiClient;

        public GroupManager(IApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        public async Task<GroupId[]> GetJoinedGroups()
        {
            var joinedGroups = await this.apiClient.GetJoinedGroups();

            var joinedGroupIds = joinedGroups
                .Select(joinedGroup => new GroupId(joinedGroup.Group.Id))
                .ToArray();

            return joinedGroupIds;
        }
    }
}
