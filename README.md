
# Frank-Read-Source-Via-Pinned-MCF

A .NET C# project to run a pinned mean curvature flow simulation with topological change as a model for the Frank-Read source.

Upon running one is asked to enter the forcing value, $f_s$.

## Files
### Program.cs
* Contains parametrisation methods $(x(\theta),y(\theta))$ for the initial dislocation line.
* Contains some pre chosen variables such as $\tau$. 
* Runs Main method to start program.

### MCF.cs
* Contains methods that calculate the motion of a dislocation line under forced and pinned MCF.

### Flow.cs
* The class that describes a dislocation lines.
* Contains methods for re-meshing the dislocation line.
* Contains methods for calculating when intersections have occured.
* Contains methods for splitting the dislocation line appropriately into two new lines.

### fPoint.cs
* Struct that describes floating point representation of cartesian coordinates in the plane.
* Contains methods for calculating properties of points in the plane as well as methods for manipulating them.

### FlowReadWrite.cs
* Contains methods for writing the simulation at each time step to frames.
* Contains methods for converting the frames to video.
* Stores forcing variable, $f_s$.

## Prerequisites 
### NuGet Packages Used:
* Accord
* Accord.Video
* Accord.Video.FFMPEG

### Standard Namespaces used:
* System
* System.Collections.Generic
* System.IO
* System.Linq
* System.Threading.Tasks
* System.Drawing
