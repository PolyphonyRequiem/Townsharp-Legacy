# Overview

# Example Session
## `Subscribe`

`SEND`
```json
{
    "id": 1,
    "method": "POST",
    "path": "subscription/group-server-status/1156211297",
    "authorization": "Bearer eyJh..."
}
```

`RECV`
```json
{
    "id": 1,
    "event": "response",
    "key": "POST /ws/subscription/group-server-status/1156211297",
    "content": "",
    "responseCode": 200
}
```

## `Server Start`
`RECV`
```json
{
    "id": 0,
    "event": "group-server-status",
    "key": "1156211297",
    "content": "{\"id\":1174503463,\"name\":\"Cairnbrook\",\"online_players\":[],\"server_status\":\"Online\",\"final_status\":\"Online\",\"scene_index\":4,\"target\":2,\"region\":\"north-america-east\",\"last_online\":\"2022-08-26T02:02:16.4072146Z\",\"description\":\"A casual server for friendly players to explore, discover, and create.\",\"playability\":0.0,\"version\":\"main-0.1.3.33060\",\"group_id\":1156211297,\"owner_type\":\"Group\",\"owner_id\":2026253269,\"type\":\"Normal\",\"fleet\":\"att-quest\",\"up_time\":\"3.01:17:05.6823802\",\"join_type\":\"PrivateGroup\",\"player_count\":0,\"created_at\":\"2021-09-10T20:34:29.2385673Z\"}",
    "responseCode": 200
}
```
`Normalized Content`
```json
"content": {
    "id": 1174503463,
    "name": "Cairnbrook",
    "online_players": [],
    "server_status": "Online",
    "final_status": "Online",
    "scene_index": 4,
    "target": 2,
    "region": "north-america-east",
    "last_online": "2022-08-26T02:02:16.4072146Z",
    "description": "A casual server for friendly players to explore, discover, and create.",
    "playability": 0.0,
    "version": "main-0.1.3.33060",
    "group_id": 1156211297,
    "owner_type": "Group",
    "owner_id": 2026253269,
    "type": "Normal",
    "fleet": "att-quest",
    "up_time": "3.01:17:05.6823802",
    "join_type": "PrivateGroup",
    "player_count": 0,
    "created_at": "2021-09-10T20:34:29.2385673Z"
}
```

## `Player Joined`
`RECV`
```json
{
    "id": 0,
    "event": "group-server-status",
    "key": "1156211297",
    "content": "{\"id\":1174503463,\"name\":\"Cairnbrook\",\"online_players\":[{\"id\":2026253269,\"username\":\"PolyphonyRequiem\"}],\"server_status\":\"Online\",\"final_status\":\"Online\",\"scene_index\":4,\"target\":2,\"region\":\"north-america-east\",\"online_ping\":\"2022-09-03T23:29:33.3370237Z\",\"last_online\":\"2022-09-03T23:29:33.3370237Z\",\"description\":\"A casual server for friendly players to explore, discover, and create.\",\"playability\":0.0,\"version\":\"main-0.1.3.33060\",\"group_id\":1156211297,\"owner_type\":\"Group\",\"owner_id\":2026253269,\"type\":\"Normal\",\"fleet\":\"att-quest\",\"up_time\":\"3.01:17:05.6823802\",\"join_type\":\"PrivateGroup\",\"player_count\":1,\"created_at\":\"2021-09-10T20:34:29.2385673Z\"}",
    "responseCode": 200
}
```
`Normalized Content`
```json
"content": {
    "id": 1174503463,
    "name": "Cairnbrook",
    "online_players": [
        {
            "id": 2026253269,
            "username": "PolyphonyRequiem"
        }
    ],
    "server_status": "Online",
    "final_status": "Online",
    "scene_index": 4,
    "target": 2,
    "region": "north-america-east",
    "online_ping": "2022-09-03T23:29:33.3370237Z",
    "last_online": "2022-09-03T23:29:33.3370237Z",
    "description": "A casual server for friendly players to explore, discover, and create.",
    "playability": 0.0,
    "version": "main-0.1.3.33060",
    "group_id": 1156211297,
    "owner_type": "Group",
    "owner_id": 2026253269,
    "type": "Normal",
    "fleet": "att-quest",
    "up_time": "3.01:17:05.6823802",
    "join_type": "PrivateGroup",
    "player_count": 1,
    "created_at": "2021-09-10T20:34:29.2385673Z"
}
```

## `Player Left`
`RECV`
```json
{
    "id": 0,
    "event": "group-server-status",
    "key": "1156211297",
    "content": "{\"id\":1174503463,\"name\":\"Cairnbrook\",\"online_players\":[],\"server_status\":\"Online\",\"final_status\":\"Online\",\"scene_index\":4,\"target\":2,\"region\":\"north-america-east\",\"online_ping\":\"2022-09-03T23:30:21.2285207Z\",\"last_online\":\"2022-09-03T23:30:21.2285207Z\",\"description\":\"A casual server for friendly players to explore, discover, and create.\",\"playability\":0.0,\"version\":\"main-0.1.3.33060\",\"group_id\":1156211297,\"owner_type\":\"Group\",\"owner_id\":2026253269,\"type\":\"Normal\",\"fleet\":\"att-quest\",\"up_time\":\"3.01:17:05.6823802\",\"join_type\":\"PrivateGroup\",\"player_count\":0,\"created_at\":\"2021-09-10T20:34:29.2385673Z\"}",
    "responseCode": 200
}
```
`Normalized Content`
```json
"content": {
    "id": 1174503463,
    "name": "Cairnbrook",
    "online_players": [],
    "server_status": "Online",
    "final_status": "Online",
    "scene_index": 4,
    "target": 2,
    "region": "north-america-east",
    "online_ping": "2022-09-03T23:30:21.2285207Z",
    "last_online": "2022-09-03T23:30:21.2285207Z",
    "description": "A casual server for friendly players to explore, discover, and create.",
    "playability": 0.0,
    "version": "main-0.1.3.33060",
    "group_id": 1156211297,
    "owner_type": "Group",
    "owner_id": 2026253269,
    "type": "Normal",
    "fleet": "att-quest",
    "up_time": "3.01:17:05.6823802",
    "join_type": "PrivateGroup",
    "player_count": 0,
    "created_at": "2021-09-10T20:34:29.2385673Z"
}
```

## `Server Shutdown`
`RECV`
```json
{
    "id": 0,
    "event": "group-server-status",
    "key": "1156211297",
    "content": "{\"id\":1174503463,\"name\":\"Cairnbrook\",\"online_players\":[],\"server_status\":\"Online\",\"final_status\":\"Online\",\"scene_index\":4,\"target\":2,\"region\":\"north-america-east\",\"last_online\":\"2022-09-03T23:36:32.0940509Z\",\"description\":\"A casual server for friendly players to explore, discover, and create.\",\"playability\":0.0,\"version\":\"main-0.1.3.33060\",\"group_id\":1156211297,\"owner_type\":\"Group\",\"owner_id\":2026253269,\"type\":\"Normal\",\"fleet\":\"att-quest\",\"up_time\":\"3.01:24:15.4811069\",\"join_type\":\"PrivateGroup\",\"player_count\":0,\"created_at\":\"2021-09-10T20:34:29.2385673Z\"}",
    "responseCode": 200
}
```
`Normalized Content`
```json
"content": {
    "id": 1174503463,
    "name": "Cairnbrook",
    "online_players": [],
    "server_status": "Online",
    "final_status": "Online",
    "scene_index": 4,
    "target": 2,
    "region": "north-america-east",
    // "online_ping" is absent when a server is shut down.
    "last_online": "2022-09-03T23:36:32.0940509Z",
    "description": "A casual server for friendly players to explore, discover, and create.",
    "playability": 0.0,
    "version": "main-0.1.3.33060",
    "group_id": 1156211297,
    "owner_type": "Group",
    "owner_id": 2026253269,
    "type": "Normal",
    "fleet": "att-quest",
    "up_time": "3.01:24:15.4811069",
    "join_type": "PrivateGroup",
    "player_count": 0,
    "created_at": "2021-09-10T20:34:29.2385673Z"
}
```


## `Migration`
`Get Migration Token`
`SEND`
```json
{
    "Id":384,
    "Method":"GET",
    "Path":"migrate",
    "Authorization":"Bearer eyJhb..."
}

```

`RECV`
```json
{
    "id":384,
    "event":"response",
    "key":"GET /ws/migrate",
    "content":"{"token":"eyJhbGc..."}",
    "responseCode":200}
```


