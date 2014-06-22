# FluxJpeg.Core

Copyright (c) 2008-2009 Occipital Open Source, (c) 2010-2013 Brian Donahue, (c) 2012-2013 Anders Gustafsson, Cureos AB   

Licensed and distributable under the terms of the [MIT license](http://www.opensource.org/licenses/mit-license.php).

*FJCore* is an image library including a pure C# implementation of the JPEG baseline and progressive codecs. Originally the library targeted Silverlight and Windows Forms applications. This is a clone of the original [Google Code library](https://code.google.com/p/fjcore) that currently seems to be stagant.

## Design goals

* No external dependencies (besides a C# compiler and ECMA-standard CIL runtime)
* High performance
* High image quality
* Simple, intuitive usage pattern

## Portable Class Library

*FJCore* is now also available as a *Portable Class Library (PCL)* that targets:

* Windows Store/8 apps
* Windows Phone version 7 and higher
* Silverlight version 4 and higher
* .NET Framework version 4 and higher
* XBox 360

## Downloads

To include *FJCore* in your application, we recommend [NuGet](https://nuget.org/packages/Flux.Jpeg.Core/).

##Changes

###0.7.2
* Signed assemblies with snk-key to make assemblies names strong.

###0.7.1
* Added portable class library to solution

###0.7.0
* **Breaking Change** Changed Resize to use specific x,y dimensions and moved old Resize to ResizeToScale
* Also wired up the ProgressChanged event to actually fire
