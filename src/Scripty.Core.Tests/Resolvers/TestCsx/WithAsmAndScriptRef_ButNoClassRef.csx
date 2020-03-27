﻿#r ".\..\..\nunit.framework.dll"
#load "ReferencedScript.csx"

//Write using supplied ScriptContext
Output.WriteLine("namespace TestNamespace{class TestClass{public void TestMethod(){}}}");

//with the referenced assembly
var bag = new NUnit.Framework.Internal.PropertyBag();
Output.WriteLine($"// {bag}");

//With referenced script
Output.WriteLine($"// we have multiplied {Go4th(2, 3)}");
