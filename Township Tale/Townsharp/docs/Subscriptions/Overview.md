# Overview

# Error Modes

Errors before websocket opens -> Fail fast?
Migrations close WebSocket with code 3000 or 3001.

Disconnected -> Connected (Re-subscribed)

Connected -> Migrating
          -> Disconnected (Deliberate)

Migrating -> Connected
          -> Disconnected (Migration Failed)



# Migration

## Migration Process

During Migration, we need to handle incoming messages differently
Halt outgoing messages during migration
New Websocket
Sent Migration Token over old websocket
If there's an issue with migration,
    Clear our ping interval
    Remove all listeners from the NEW client
    Close the NEW client as a 3001
    Dispose the NEW client.

Clear Halt condition
Set new Migration Due Date
Close OLD client as 3000
Remove all listeners from OLD client

## Sending the Migration Token

Listen for a response message with status 200 and key `POST /ws/migrate`

## Messages and Responses

An outbound message is of the form:

```
{
    "id": |messageId|,
    "method": "|method|",
    "path": "|path|",
    "authorization": "Bearer |token|"
}
```
