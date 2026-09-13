// Product identity, shared by every assembly in the solution.
//
// Linked into all projects by Directory.Build.props. It lives in one file because every project
// sets GenerateAssemblyInfo=false, which means the <Company>, <Copyright> and <Product> properties
// in a .csproj are ignored entirely - the attributes have to be declared in source or the built
// DLLs carry a blank publisher, which is what they did before this file existed.
//
// The version is deliberately NOT here: each project still carries its own AssemblyInfo.cs with
// AssemblyVersionAttribute, and declaring it twice would not compile.

using System.Reflection;

[assembly: AssemblyCompany("DavidVeld")]
[assembly: AssemblyCopyright("Copyright © DavidVeld")]
[assembly: AssemblyProduct("Carbo Life Calculator")]
