using System.Collections.ObjectModel;

namespace Townsharp.Servers
{
    public abstract class ServersManager
    {
        protected Dictionary<ServerId, Server> KnownServers = new Dictionary<ServerId, Server>();
        // Concept of "managed" is still fuzzy here, define in the ubiquitous language in obsidian at some point please.
        protected Dictionary<ServerId, Server> ManagedServers = new Dictionary<ServerId, Server>();

        public abstract Task<ServerId[]> GetJoinedServerIds();

        public abstract Task<Server> GetServer(ServerId id);
    }
}
