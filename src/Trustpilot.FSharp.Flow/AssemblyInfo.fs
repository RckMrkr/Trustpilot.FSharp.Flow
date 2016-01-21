namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyProductAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyDescriptionAttribute("A library for building contract based execution flows")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
