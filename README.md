# Core
---

This repository consists of libraries and packages, which are being used in other projects as a base layer. The main purpose is to provide a sleek and streamlined set of utilities for all kinds of projects as well as a common runtime for CLI- and GUI-applications.

## License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*

## Included packages

### Foundation
This package contains functionality which is required in all projects and is considered the minimum dependency for every library or application. It is targeting .NET Standard and .NET Framework for usage in cross-platform-scenarios.

Notable features are:

- Target-based logging
- Storage abstraction
- Directed graphs
- Task management / scheduling
- MVVM / MVC base classes
- Basic application runtime / console application runtime

The package should contain the least amount of dependencies as possible.

### Foundation.Data
This package contains a sleek API on top of ADO.NET, which provides an additional level of abstraction against various data sources. Also included is a `DynamicObject`-implementation of an `IDataReader` to allow the use of ADO.NET in scenarios with runtime binding.

The connection factory provides an utility for the abstraction of ADO.NET providers. Providers can be registered at runtime and requested by a key, which is chosen by the registrator. The connection factory creates connection objects, which are equivalent to `IDbConnection` but easier to use. Of course, this interface can also be given to the constructor of a connection object - the use of a connection factory is not required.

The package is targeting .NET Standard and .NET Framework for usage in cross-platform-scenarios.

### Foundation.Extensibility
The extensibility package contains to easily create plugin- and extension systems for applications and libraries. It provides base classes and interfaces for both, the plugin server and the plugin client. 

This even works in cross-platform-scenarios as the package is targeting both, .NET Standard and .NET Framework.

### Foundation.Communication
This package allows the consumer to easily host web services with a declarative approach. All which needs to be done is the implementation of a class containing methods, which are exported via attributes. The class is then given to a web host implementation alongside an endpoint configuration which takes the rest.

The package is targeting .NET Standard and .NET Framework for usage in cross-platform-scenarios.

### Foundation.Communication.Client
In addition to the communication package, a fluent web service client API is provided with the communication client package. With this API, creating web service requests and handling the in- and output data fluently is very easy and transparent to the consumer. Also async programming is supported by making use of TPL.

Like the communication package, this package is targeting .NET Standard and .NET Framework for usage in cross-platform-scenarios.

### Foundation.Windows
This package is a specialization of the foundation package for Microsoft Windows GUI applications using WPF. It contains XAML extensions and a runtime implementation for WPF applications. Since it is a specialization, this package is not targeting .NET Standard, just .NET Framework.

## Repository structure

The structure of this repository is standardized. Items shouldn't be placed on the root foder. Instead, they should be contained in one of the sub-structures below.

**src**:
The actual source code of the repository as a Microsoft Visual Studio solution

**tools**:
Script includes for PACMAN and other management shells as well as binaries for contained build tools.

## How to...

### Build
To build the solution after checkout, the following commands need to be executed in the Visual Studio Developer Shell:

	msbuild /t:restore
	msbuild /t:build

The first command restores the NuGet-packages included in the solution, the second command builds the solution.

### Create NuGet-packages
To create the NuGet-packages provided by this project, the following command needs to be executed in the PACMAN shell accessible with `shell.cmd`:

	 Publish-Packages -BuildOnly

### Create and publish NuGet-packages
To also publish the packages, omit the `-BuildOnly` switch:

	 Publish-Packages

### Change package version
The version manager is also accessible using the PACMAN shell and is used like below:

	# Raises the major version
		> Update-Version -Release
		  1.0.0 -> 2.0.0

	# Raises the minor version
		> Update-Version -Update
		  1.0.0 -> 1.1.0

	# Raises the patch version
		> Update-Version -Patch
		  1.0.0 -> 1.0.1

	# Sets a specific version
		> Update-Version -Version 2.1.0
		  1.0.0 -> 2.1.0

	# Raises the major version and adds a pre-release label 
		> Update-Version -Release -PreRelease alpha2
		  1.0.0 -> 2.0.0-alpha2

	# Raises the patch version by two steps
		> Update-Version -Patch -Increment 2
		  1.0.0 -> 1.0.2
