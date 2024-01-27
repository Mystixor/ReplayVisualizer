# ReplayVisualizer
ReplayVisualizer is a tool that displays the path that was driven in a replay from TMUF, ManiaPlanet or Turbo.

Inspired by the private daily Turbodle event hosted by Rotaker.

# How to use
Drag a Replay or Ghost onto the supplied Batch file, deciding the view from which the path will be drawn. You may also choose between a `save Bitmap` variant which simply saves the drawing to disk, or the normal variant which opens a window instead.

# Settings
In the application directory should be a file called `settings.json`, which will give basic control over the final plot.

`scalingFactor`: Floating point number scaling the entire plot, to enhance/decrease quality.

`penWidth`: Floating point number setting the width of the drawn path in pixels, which is then multiplied by `scalingFactor`.

`penColor/backgroundColor`: Red, Green, Blue, Alpha (R/G/B/A) channel values defining the color of the drawn path and the background of the plot, respectively. The range is from 0 to 255.

# Command line
The Batch files use the command line interface of the application. You can use this yourself in the following format:

`ReplayVisualizer.exe "path\to\YourFile.Replay.Gbx" ZX/XY/ZY [--bitmap] [--nowindow]`

In Trackmania, the `Y` axis is the up-axis, pointing to the sky. Therefore:

`ZX`: top-down view

`XY`: south view

`ZY`: west view

Also:

`--bitmap`: The path in the Replay will be saved to disk as a Bitmap image.

`--nowindow`: The application will not open a window displaying the image. Combine this with `--bitmap` to only save to disk.
