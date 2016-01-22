module Trustpilot.FSharp.Flow.Tests

open Trustpilot.FSharp.Flow
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = 42
  printfn "%i" result
  Assert.AreEqual(42,result)
