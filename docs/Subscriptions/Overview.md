# Overview

# Connecting

The following WORKS:
wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev
[Authorization] = Bearer {token}
- NO CLIENT INFO NEEDED -

# State Flow

START     -> CONNECTED

CONNECTED -> MIGRATING
          -> DISCONNECTED

# Error Modes

Errors before websocket opens -> Fail fast?
Migrations close WebSocket with code 3000 or 3001.

Disconnected -> Connected (Re-subscribed)

Connected -> Migrating
          -> Disconnected (Stopped)

Migrating -> Connected
          -> Disconnected (Migration Failed)

# Migration

## Migration Process

During Migration, we need to handle incoming messages differently
MessageHalt outgoing messages during migration
New Websocket
Sent Migration Token over old websocket
If there's an issue with migration,
    Clear our ping interval
    Remove all listeners from the NEW client
    Close the NEW client as a 3001
    Dispose the NEW client.

Clear MessageHalt condition
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

A response is of the form:

```
{
    "id": 0,
    "event": "response",
    "key": " /ws/subscription/group-server-status/103278376",
    "content": "",
    "responseCode": 500
}
```

Errors look like this, note the embedded json:
```
{
    "id": 1,
    "event": "response",
    "key": "POST /ws/subscription/group-server-status/103278376",
    "content": "{\"message\":\"Could not find any Group with identifier 103278376\",\"error_code\":\"ObjectNotFound\"}",
    "responseCode": 404
}
```

QUESTIONS:
Do we need to send the authorization token on every websocket request?

OBSERVATIONS:
Messages may make sense as commands.