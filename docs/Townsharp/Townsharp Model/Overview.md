Trying to work through my thoughts on the relationship between groups, servers, session, and subscription so here goes.

[[Session]]
Session is the root access point for interacting with Townsharp.  A session represents a single access context, capturing credentials, managing access lifecycles, and ensuring isolation of logical management of a given "workspace" when it comes to dealing with Townsharp models.  Ideally multiple sessions can coexist with overlapping or distinct credentials and state without interference, giving a means for multi-tenancy in process.

[[Townsharp Model/Group]]
A group represents an administrative plane for user access to the group, and also is associated with a collection of servers. Groups are also the subject of most subscription events, at least in part (is this actually true?).  A group could be reasonably expected to have events raised on it around these changes.  What is the source of these change events?

[[Townsharp Model/Server]]
A server represents a single Township Tale server.  Events may occur at the server level (player joined, player leaves, server online, server offline, which we should observable).
What is the source of these change events?

[[Subscription]]
I'm not sure these make sense at the individual level to be honest.  I think this should be abstracted away by Townsharp, and if something is registered for it is subscribed to.  The manager however makes plenty of sense.