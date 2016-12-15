# Football-game-with-Hololens-and-Kinect
A unity game where the person wearing the hololens plays as a goalkeeper and stops the ball which being kicked towards him/her. The movements of the person are tracked using Kinect.

# Sources
Hololens and Kinect scripts are derived from https://github.com/michell3/Hololens-Kinect

Football game assets and controller scripts are derived from existing football game from the lab

# Game Setup
Requires two Windows 10 machines with the following installed:
Kinect 2

Hololens Unity

Visual Studio 2015 Update 2

The Kinect SDK 2.0

HoloToolkit

Kinect Unity Pro Packages

One machine which is connected to Kinect, runs the Hololens-sender app on Unity where it sends Kinect skeletal data to the Hololens receiver connected through a sharing session manager. The other machine holds the Hololens receiver scripts and renders the data on the screen.

# Code description
## Hololens-sender:
The following scripts aid in sending skeletal data to the receiver through sharing session manager:

  SharingStage.cs - From HoloToolkit
  
  SharingSessionTracker.cs - From HoloToolkit
  
  AutoJoinSession.cs - From HoloToolkit
  
  CustomMessages2.cs - From https://github.com/michell3/Hololens-Kinect. Allows for customized messages to be broadcasted. In this case,     body tracking IDs and joint data.
  
  KinectBodyData.cs - From https://github.com/michell3/Hololens-Kinect. Reads in Kinect skeletal data.
  
  BodyDataConverter.cs - From https://github.com/michell3/Hololens-Kinect.  Parses the Kinect body data.
  
  BodyDataSender.cs - From https://github.com/michell3/Hololens-Kinect. Broadcasts the converted body data using custom messages.
  
  BodyView.cs - From https://github.com/michell3/Hololens-Kinect. Displays the converted body data.
  
## Hololens-receiver:
The following scripts aid in rendering received skeletal data to the screen through sharing session manager. These scripts are deployed to Hololens.

  SharingStage.cs - From HoloToolkit
  
  SharingSessionTracker.cs - From HoloToolkit
  
  AutoJoinSession.cs - From HoloToolkit
  
  CustomMessages2.cs - From https://github.com/michell3/Hololens-Kinect. Allows for customized messages to be broadcasted. In this case,     body tracking IDs and joint data.
  
  BodyDataReceiver.cs - From https://github.com/michell3/Hololens-Kinect.. Listens for custom messages containing skeletal data.
  
  BodyView.cs - From https://github.com/michell3/Hololens-Kinect. Displays the body data. Takes the scene's BodyReceiver as a parameter.
  
## Football collision and scoring scripts:

  FootballHoloGameController.cs - Modified from scripts in the existing football game from the lab. Adds score on collision and makes the   ball aim at objects with the tag "Targets" at random points.
  
  footballCollision.cs - Calls methods from the FootballHoloGameController on collision with "target" objects and with the player.           Modified from scripts in the existing football game from the lab. 
  
  BallReset.cs - Resets the ball to its starting position after every "kick".
