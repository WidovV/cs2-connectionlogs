# cs2-connectionlogs Version 0.3
 Create a folder called ConnectionLogs inside your /plugins/ folder.  
 
 Use the compiled version: Drag the files from `\bin\Release\net7.0\publish` inside of the ConnectionLogs folder except from `CounterStrikeSharp.API.dll`  
 
 Compile yourself: Run _compile.bat to compile the plugin, it will be placed in `\bin\Release\net7.0\publish` folder.

# Dependencies
- [CounterStrikeSharp](https://docs.cssharp.dev/)
- [Metamod](https://www.sourcemm.net/downloads.php/?branch=master)

## Description
This plugin adds players to a database to track when they join with their ip.  
It prints to discord through a webhook when a player join (with their ip) and leaves.  
It has a command !connectedplayers that prints the 50 recent players that joined the server.

## Config
The config will automaticly be generated on first run and will be placed inside of the same directory as the plugin itself.  
Colors can be used in every key-value that is a string like so {White} or {Red} (this is case insensitive, thanks to [k4ryuu](https://github.com/K4ryuu) for the idea), every color in the ChatColors class can be used.
Example:
```json
{
  "ChatPrefix": "[CPH-{Darkred}GAMING{white}]",

  "SendMessageToDiscord": true,
  "DiscordWebhook": "https://discord.com/api/webhooks/",

  "StoreInDatabase": true,
  "DatabaseHost": "192.168.1.210",
  "DatabasePort": 3306,
  "DatabaseUser": "WidovV",
  "DatabasePassword": "MySuperSecretPassword",
  "DatabaseName": "test-connectionlogs"
}
```

![Alt text](image.png)

