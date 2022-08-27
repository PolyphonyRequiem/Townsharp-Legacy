﻿namespace Townsharp.Groups
{
    public record GroupId
    {
        private readonly long id;

        public GroupId(long id)
        {
            this.id = id;
        }

        public static implicit operator GroupId(long id)
            => new GroupId(id);

        public static implicit operator long(GroupId groupId)
            => groupId.id;
    }
}
