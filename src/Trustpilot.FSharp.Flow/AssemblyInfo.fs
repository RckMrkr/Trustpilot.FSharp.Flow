namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyProductAttribute("Trustpilot.FSharp.Flow")>]
[<assembly: AssemblyDescriptionAttribute("A library for building explicit execution flows")>]
[<assembly: AssemblyVersionAttribute("0.4.3")>]
[<assembly: AssemblyFileVersionAttribute("0.4.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.3"
