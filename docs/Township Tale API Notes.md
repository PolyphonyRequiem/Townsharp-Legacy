The following information was found by days of testing Alta's endpoints and debugging the code of: [Js-tale, alta-api, ATT-websockets etc...]. 
All sources of code are undocumented. All sources of code have function calls on function calls that span multiple files and sometimes even libraries
which make it hard to understand/read. Especially for someone like me, who has never used Javascript/Typescript and is unfamiliar with syntax,
not to mention it's library import's functions. There's probably more information in there, I'm just entirely unable to understand it. Please DM/@ me
at the ATT-Meta Discord Dly2424#2781 for any questions or help.

General flow:
Go to https://accounts.townshiptale.com/connect/token using your bot's credentials sent by Joel, get temporary access_token from there. Use this token to
	access the main websocket wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev
	as well as make REST queries to https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/...
	Make rest query to https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/servers/{group_id}/console
	to get token, address and port to connect to the console websocket for a server.

https://accounts.townshiptale.com/connect/token:
	-Purpose of URL: To obtain a temp Bearer token to use for our Websocket connection at wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev
		and POST/GET/DELETE(etc...) requests to https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/...
		
	-Headers required: User-Agent, Content-Type, Host, Content-Length
		example:
		{
		'Content-Type': 'application/x-www-form-urlencoded'
		'Host': 'accounts.townshiptale.com'
		'User-Agent': client_79s0nt5r-1wd8-4zz5c-95u9f-aazsdswtgb
		'Content-Length': '395'
		}
		
	-body required: grant_type, scope, client_id, client_secret
		example:
			grant_type=client_credentials&scope=ws.group%20ws.group_members%20ws.group_servers%20ws.group_bans%20ws.group_invites%20group.info%20group.join%20group.leave%20group.view%20group.members%20group.invite%20server.view%20server.console&client_id=client_89hs97t5r-1wd8-4zz5c-95u9f-aazsdswtgb&client_secret=v8rv98yne87y97w7y9c7ynrfccr98wy4Adi42mRflhuio&gGUYV9v/vaserv==

		
	-Notes: Uses POST request. The returned value 'access_token' is your Bearer token.
				This is the only endpoint that doesn't use json Content-Type from what i can tell.
	
	-Return example <Response 200>:
	{'access_token': 'Llvi7v7gUV_YuVUtCU_dCVUFYoKIU...(This is really long usually)', 'expires_in': 3600, 'token_type': 'Bearer', 
	'scope': 'group.info group.invite group.join group.leave group.members group.view server.console server.view ws.group ws.group_bans ws.group_invites ws.group_members ws.group_servers'}



wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev:
	-Purpose of URL: This URL is the websocket address you connect to in order to recieve some info such as invites to groups etc in real-time.

	-Headers required: Content-Type, x-api-key, User-Agent, Authorization
		example:
		{
		'Content-Type': 'application/json'
		'x-api-key': '2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf'
		'User-Agent': client_7g34t5r-1wd8-4zz5c-95u9f-aazsdswtgb
		'Authorization': 'Bearer kiabdilugalsiudlau8yh9dfo8yb98bq98v_puq89o_v8q9b8(this token is really long usually)'
		}
		
	-Notes: Will disconnect you aprox 10 minutes after not being sent data. Be sure to periodically send data to it. Invalid data works such as "Ping!".
		Also requires that you get a new token every hour (or prematurely) from the https://accounts.townshiptale.com/connect/token endpoint.
		Because after an hour, the token expires (see expires_in from https://accounts.townshiptale.com/connect/token return example)



https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/...:
	-Purpose of URL: We request from this URL to request information about things like: members of private groups, invites the account has recieved,
		available server consoles you can connect to etc...
		
	-Headers required: Content-Type, x-api-key, User-Agent, Authorization
		example:
		{
		'Content-Type': 'application/json'
		'x-api-key': '2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf'
		'User-Agent': client_7g34t5r-1wd8-4zz5c-95u9f-aazsdswtgb
		'Authorization': 'Bearer kiabdilugalsiudlau8yh9dfo8yb98bq98v_puq89o_v8q9b8(this token is really long usually)'
		}
		
	-Notes: Some requests require a body. For example: https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/users/search/username
		requires a body containing the name of who you are searching? (Needs confirmation by dev, no idea exactly how this works)
		The "prod" part of the URL is interchangeable with: [test, dev, latest, local]. I'm uncertain to what difference it makes.
		
		
		
### REST QUEREIES ### [https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/...] see required headers above. All responses return a json.


https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/servers/{group_id}/console
Gets info about a groups server console. This returns stuff like the IP address and Port of the websocket for the console. You connect to this website via:
	ws://{addr}:{port}
	Note this is not the WSS protocol, this is WS. All the other alta websockets use WSS.
	This URL requires a body of: {"should_launch":"false","ignore_offline":"false"} being sent (I believe these are configurable bools). Uses POST method.
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/{group_id}/requests/{player_id} (maybe works? needs confirmation)
Can be sent a DELETE request to reject a player's request to join
	or can be sent a POST request to accept a player's request to join
	Note: Bot will probably need administrative permissions in order to do this on the respective server.
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/{group_id}/invites/{player_id}
Can be sent a POST request to invite a player to a server, or DELETE to uninvite a player from a server.
	Note: Bot will probably need administrative permissions to do this.
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/invites/{group_id}
Can be send a POST to accept the invite to a server, or a DELETE to reject the invite to the server.
	Note: This is the way the bot accepts/rejects it's own invites to servers. 
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/joined
Gets a json of the groups you are currently a member of.
	Note: Uses GET method
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/{group_id}/members
Gets a json of the members in a group.
	Note: Uses GET method
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/groups/invites?limit=1000 (?limit=1000 is not necessary. Just limits the number or received invites :3)
Gets a json of all your invites to groups.
	Note: Uses GET method
	
https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/servers/console
Gets a json of all consoles you have permissions to use.
	Note: Uses GET method
	



### WEBSOCKETS ###


Main websocket:
wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev
This is the main websocket used for bots. This websocket allows you to do things like get real-time subscriptions to events to invites, or being kicked from a server etc.
See top of document for more info.
How to send content to this websocket:
Content must be json format. Example:
{"id": 1, "method": "POST", "path": "subscription/me-group-invite-create/{client-id}", "authorization": "Bearer 9873zv98s709f839dv086498y98uhfd987efhw7g937t98..."}
The above example subscribes to getting invites to servers aka groups. (there is also a me-group-invite-delete)
There are more subscriptions but I'm busy writing my own library atm :3 wait for full docs if you don't want to search Alta's libraries for more.


Console websockets:
ws://{addr}:{port} (This is WS protocol. Not WSS like the main websocket.)
This is the console websocket. Console websockets generally run concurrently with other websockets, such as other console websockets and the main websocket.
	You can get the address and port of your server's console by making a post request to:
	https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/servers/{group_id}/console
	and getting the "address" value for the address, and "console_port" within the "connection" section for the port.
	As well as your token from the "token" value. You'll need it to connect to the console.
	The absolute first this you must do after connecting to a console websocket, is send the token to it just as is. Not in json format.
	After doing so, you'll receive a response stating you've been verified to which you can then send commands just like you would in the dashboard, however it needs to be in this format:
	{"id":{i},"content":"{command}"} (json format)
	Where "i" is a unique number, and command is the command you would typically type in the dashboard. "i" is necessary because the responses you get back
	won't always be in order. So it's how you identify what response goes to what command. The ID doesn't have to be anything crazy. Personally I just
	start with a variable at 0 and add 1 to it each time a command is sent. (Alta's official libraries also does this)
	Note: If you don't send your token, or if messages after aren't in the specified format, you will get NO response whatsoever from the server.
	


### MORE INFO TO COME ###
These are not ALL the endpoints. Only the currently confirmed working ones. I do have more data available, however until i work out the details these will be undocumented.
This information took tons of time to figure out. If you find any more information not listed here, message me on discord: Dly2424#2781 Good luck on your development!