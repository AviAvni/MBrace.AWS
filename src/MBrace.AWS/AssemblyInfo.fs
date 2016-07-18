﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("MBrace.AWS")>]
[<assembly: AssemblyProductAttribute("MBrace.AWS")>]
[<assembly: AssemblyDescriptionAttribute("AWS PaaS bindings for MBrace")>]
[<assembly: AssemblyVersionAttribute("0.1.7")>]
[<assembly: AssemblyFileVersionAttribute("0.1.7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.7"
    let [<Literal>] InformationalVersion = "0.1.7"
