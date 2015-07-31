# NetworkTablesDotNet
<a href="https://www.myget.org/"><img src="https://www.myget.org/BuildSource/Badge/robotdotnet-build?identifier=5d4868a2-d79f-453e-9640-cc5ee0759d20" alt="robotdotnet-build MyGet Build Status" /></a>

This is a native DotNet implementation of NetworkTables. Much of the externally facing code, and the method casing were derived from the C++ implementation, with other internal parts derived from the Java implementation. NetworkTables are used to pass non-Driver Station data to and from the robot across the network.

The implementation is currently compiled for .Net 4.5, but as long as you are using VS 2015, you should be able to compile it to .Net 3.5 without much modification. 

.. note:: NetworkTables is a protocol used for robot communication in the
          FIRST Robotics Competition, and can be used to talk to
          SmartDashboard/SFX. It does not have any security, and should never
          be used on untrusted networks.
          
Documentation
-------------
TODO

Installation
------------
The lastest version can be found on NuGet, for use with desktop applications. When you create a WPILib project for the RoboRIO, it automatically gets downloaded.
