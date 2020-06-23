# Juicebox Audio Player
Multi-user music player made with ASP.NET Core and React. Also uses MySQL database. [Hosted on Heroku](http://jboxaudio.herokuapp.com) using Docker. Made as an example full stack project.
### :)

Join/Create a room and upload/listen to mp3s.
Click on users to see who is listening to what in real time.

![Dark mode only!](https://i.imgur.com/1Zo2pE3.png)

## TODO
- Sync to other user option
- Visualizer
- User Login
- Additional music sources
- Other stuff

### Current Issues/Limitations
 - When heroku dyno does its daily reset it doesn't gracefully close all signalR connections leaving users in database that aren't actually connected. This will be fixed.
 - For the purposes of the demo and to keep this project free of cost, uploaded files are not currently uploaded to a database but rather the server's local file path. They get wiped when the dyno resets. This also means there is quite a hard limit on how many songs can be uploaded.
 - At the time of writing this, file streaming/seeking is kind of hacky due to just having figured out how to serve a file with byte range requests in .NET Core. The change that made that possible was a very sparsely documented recent change to .NET Core.

