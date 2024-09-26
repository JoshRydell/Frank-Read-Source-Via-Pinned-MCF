
# Frank-Read-Source-Via-Pinned-MCF

A .NET C# project to run a pinned mean curvature flow simulation with topological change as a model for the Frank-Read source.

Upon running one is asked to enter the forcing value, $f_s$.

### Program
* Contains parametrisation methods $(x(\theta),y(\theta))$ for the initial dislocation line.
* Paramaters tMax - the total number of frames.
* N - the number of points in the initial mesh.
* Tau - the timestep.
* Runs Main method to start program.

### MCF
* Contains methods that calculate the motion of a dislocation line under forced and pinned MCF.

### Flow
* The class that describes a dislocation lines.
* Contains methods for re-meshing the dislocation line.
* Contains methods for calculating when intersections have occured.
* Contains methods for splitting the dislocation line appropriately into two new lines.

### fPoint
* Struct that describes floating point representation of cartesian coordinates in the plane.
* Contains methods for calculating properties of points in the plane as well as methods for manipulating them.

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
