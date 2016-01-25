namespace Trustpilot.FSharp
module Common =
    module Compatibility =
        module List =
            let singleton (x: 'a) : List<'a> = [x]

         module Option =
            open System

            let ofObj : 'a -> Option<'a> = function
            | null -> None
            | o    -> Some o

            let ofNullable (nullable: Nullable<'a>) : Option<'a> =
                if nullable.HasValue
                    then Some (nullable.Value)
                    else None

        let isNull (x : 'a) : bool =
            obj.ReferenceEquals(x, null)

    module Prelude =
        open System

        let const' k _ = k

        let undefined<'T> : 'T = failwith "not implemented"

        let flip f a b = f b a

        let guard (expr: unit -> 'a) (err: Exception -> 'a): 'a =
            try
                expr()
            with
            | ex -> err ex

        let option f def =
                function
                | Some v -> f v
                | None -> def

        let inline unpack< ^input, 'output when ^input : (static member Unpack : ^input -> 'output)> (inp : ^input) : 'output =
            (^input  : (static member Unpack : ^input -> 'output) inp)

        let inline unpackStr< ^input, 'output when ^input : (static member Unpack : ^input -> 'output)> (inp : ^input) : String =
            (^input  : (static member Unpack : ^input -> 'output) inp).ToString()

        let swap (a, b) = (b, a)

    module Option =
        open Prelude
        open System

        /// Prefer ofObj, this is only to be used in situations where null is not in the type.
        let ofUnsafeNull o = if Compatibility.isNull o then None else Some o

        let valueOrDefault<'a> : 'a -> Option<'a> -> 'a = option id

        let join = function 
            | Some (Some s) -> Some s
            | _ -> None

        let lift (f: 'a -> 'b) : Option<'a> -> Option<'b> = Option.bind(Some << f)

        let fromCondition (conditional: 'a -> bool) (input : 'a) : Option<'a> =
            if conditional input then Some input else None

        let passCondition (conditional: 'a -> bool) : Option<'a> -> Option<'a> =
            Option.bind <| fromCondition conditional

        let ofDefault (a : 'a) : Option<'a> =
            if a = Unchecked.defaultof<'a>
                then None
                else Some a

        let toNullable<'a when 'a : (new : unit -> 'a)
                           and 'a : struct
                           and 'a :> ValueType
                      > : Option<'a> -> Nullable<'a> =
            Option.map (fun a -> new Nullable<'a>(a) )
            >> valueOrDefault (Nullable())

    module String =
        open Compatibility

        let trim s = if System.String.IsNullOrEmpty s then s else s.Trim()
        let toLower s = if System.String.IsNullOrEmpty s then s else s.ToLowerInvariant()
        let toUpper s = if System.String.IsNullOrEmpty s then s else s.ToUpperInvariant()
        let isEmpty = (=) System.String.Empty
        let ofNull s = if isNull s then "" else s

    module Async =
        open System.Diagnostics
        open System

        let map f ma = 
            async { 
                let! a = ma
                return f a
            }

        let time<'r> (ma : Async<'r>) : Async<TimeSpan * 'r> = 
            async {
                let timer = Stopwatch()
                timer.Start()
                let! a = ma
                return (timer.Elapsed, a)                
            }
    
    module Choice =
        let merge1Of2 (f : 'a -> 'b) (c : Choice<'a,'b>) : 'b =
            match c with
            | Choice1Of2 a -> f a
            | Choice2Of2 b -> b

        let merge2Of2 (f : 'b -> 'a) (c : Choice<'a,'b>) : 'a =
            match c with
            | Choice1Of2 a -> a
            | Choice2Of2 b -> f b
                
        let choice (f1 : 'a -> 'c) (f2 : 'b -> 'c) (c : Choice<'a,'b>) : 'c =
            match c with
            | Choice1Of2 a -> f1 a
            | Choice2Of2 b -> f2 b