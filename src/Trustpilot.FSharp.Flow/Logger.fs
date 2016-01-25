namespace Trustpilot.FSharp

module Logger =

    module Types =
        type Property =
            { Name  : string
              Value : string }

        type PropertyLogger =
            { LogError : List<Property> -> Async<unit>
              LogInfo  : List<Property> -> Async<unit> }
    
        type IConvertToProperties = 
            abstract member Properties : List<Property>

    module Property =
        open Types
        open System
        open Common
        open Compatibility
        open System.Net

        let inline properties (t : 't when 't :> IConvertToProperties) = t.Properties

        let inline property name value = { Name = name; Value = string value}

        let name (p : Property) = p.Name

        let value (p : Property) = p.Value

        let fromTuple (n,v) = { Name = n; Value = v}
        
        let prefixName s (p : Property) = { p with Name = sprintf "%s%s" s p.Name}
        
        let fromException (exn : Exception) =
            let fromExcp (e : exn) = 
                [ property "Message" <| String.ofNull e.Message
                  property "StackTrace" <| String.ofNull e.StackTrace
                  property "Full" e ]
            
            let getInner (e : Exception) =
                Option.ofObj e.InnerException
                |> Option.toList
                |> List.map fromExcp
                |> List.concat
                |> List.map (prefixName "Inner.")

            (fromExcp exn @ getInner exn)
            |> List.map (prefixName "Exception.")    

        let unique (ps : List<Property>) =
            List.filter (not << String.IsNullOrWhiteSpace << value) ps
            |> List.map (fun p -> (p.Name, p.Value))
            |> Map.ofList
            |> Map.toList
            |> List.map fromTuple

    module Json =
        open Newtonsoft.Json.Linq
        
        let rec flattenToken (jt : JToken) = 
            if jt.HasValues 
                then Seq.collect flattenToken (jt)
                else seq { yield (jt.Path, jt.ToString()) }

        let deserialize o =
            let s = JObject.FromObject o
            flattenToken s
            |> List.ofSeq
        
        let toProperties o =
            deserialize o
            |> List.map (Property.fromTuple)

    module App =
        open Execution
        open AppFlow
        open Types

        let inline appErrorToProperties (error : AppError<#IConvertToProperties>) : List<Property> =
            match error with
            | StoreError (e,s) -> 
                Property.fromException e 
                @ [Property.property "Message" s]
                |> List.map (Property.prefixName "StoreError")
            | RequestError (e, s) -> 
                Property.fromException e 
                @ [Property.property "Message" s]
                |> List.map (Property.prefixName "RequestError")
            | UnhandledError e ->
                Property.fromException e 
                |> List.map (Property.prefixName "UnhandledError")
            | BusinessError e ->
                Property.properties e
                |> List.map (Property.prefixName "BusinessError")

        let inline executionAppToProperties (e : Execution<#IConvertToProperties,AppError<#IConvertToProperties>>) : List<Property> =
            let response = appErrorToProperties e.Output
                           |> List.map (Property.prefixName "Output")
            let request = Property.properties e.Input
                          |> List.map (Property.prefixName "Input")
            let stats = [ Property.property "RunTimeMS" e.RunningTime.TotalMilliseconds ]
                        |> List.map (Property.prefixName "Stats")
            request @ response @ stats