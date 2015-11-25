# NetworkTablesDotNet
[![Build status](https://ci.appveyor.com/api/projects/status/wti22t106a5yipby/branch/master?svg=true)](https://ci.appveyor.com/project/robotdotnet-admin/networktablesdotnet/branch/master)


Notice
=======

Please use [NetworkTablesCore](https://github.com/robotdotnet/NetworkTablesCore) instead. This will not be updated, unless somebody else would like to take that task on.





This is a native DotNet implementation of NetworkTables. Much of the externally facing code, and the method casing were derived from the C++ implementation, with other internal parts derived from the Java implementation. NetworkTables are used to pass non-Driver Station data to and from the robot across the network.

The implementation is currently compiled for .Net 4.5, but as long as you are using VS 2015, you should be able to compile it to .Net 3.5 without much modification. 


.. note:: NetworkTables is a protocol used for robot communication in the
          FIRST Robotics Competition, and can be used to talk to
          SmartDashboard/SFX. It does not have any security, and should never
          be used on untrusted networks.


