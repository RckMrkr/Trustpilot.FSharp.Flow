﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyProductAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyDescriptionAttribute("A library for building contract based execution flows")>]
[<assembly: AssemblyVersionAttribute("0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.3"
