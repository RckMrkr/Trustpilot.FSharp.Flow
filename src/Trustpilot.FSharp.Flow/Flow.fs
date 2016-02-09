namespace Trustpilot.FSharp

    type Result<'success, 'failure> = Success of 'success
                                    | Failure of 'failure

    type Flow<'a, 'e> = Async<Result<'a, 'e>>

    module Result =
        open Common

        let mapFailure f =
            function
            | Success x -> Success x
            | Failure x -> Failure <| f x

        let map f =
            function
            | Success x -> Success <| f x
            | Failure x -> Failure x

        let fromChoice<'a,'e> : Choice<'a,'e> -> Result<'a,'e> =
            Choice.choice Success Failure

    module Flow =
        open Common
    
        type FlowBuilder() =
            member this.Return(x : 'T) = async { return (Success x) }
            member this.ReturnFrom(x : Async<'T>) = x
            member this.Fail (x: 'T) = async { return Failure x }
            member this.Bind(x, f) = async {
                let! result = x
                match result with
                    | Success r -> return! (f r)
                    | Failure l -> return (Failure l) }
            member this.Zero () = async.Return()
            member this.Delay f =
                let delayed () = f ()
                delayed
            member this.Run f = f ()
            member this.Combine (m, f) = async {
               let! value = m
               match value with
               | Success _ -> return! f ()
               | Failure e -> return (Failure e)
            }
            member x.Using(resource : 'a, expr: 'a -> Flow<'b,_>) =
                async {
                    let! res = Async.Catch (expr resource)
                    (resource :> System.IDisposable).Dispose()
                    match res with
                    | Choice1Of2 e ->
                        return e
                    | Choice2Of2 exn ->
                        return raise exn
                }

        let flow = FlowBuilder()
                
        let map (f: 'a -> 'r) (x: Flow<'a, 'b>) : Flow<'r, 'b> =
            async {
                let! value = x
                return Result.map f value
            }

        let mapFailure (f: 'b -> 'r) (x: Flow<'a, 'b>) : Flow<'a, 'r> =
            async {
                let! value = x
                return Result.mapFailure f value
            }

        let catch (ma : Flow<'a,'e>) : Flow<'a,Choice<'e,exn>> =
            async {
                let! a = Async.Catch ma
                match a with
                | Choice1Of2 (Success s) -> return Success s
                | Choice1Of2 (Failure err) -> return Failure (Choice1Of2 err)
                | Choice2Of2 err -> return Failure (Choice2Of2 err)
            }

        let inline pure' a : Flow<_,'error> = async { return Success a }

        let inline bind f ma = flow.Bind(ma,f)

        let inline (>>=) ma f = bind f ma

        let catchMap (f : exn -> 'e) (ma : Flow<'a,'e>) : Flow<'a,'e> =
            ma
            |> catch
            |> mapFailure (Choice.mergeSnd f)

        let startChild (ma : Flow<'r,'e>) : Flow<Flow<'r,'e>,'e> =
            async {
                let! a = Async.StartChild ma
                return Success a
            }

        let fromAsyncChoice (ma : Async<Choice<'a,'e>>) : Flow<'a,'e> =
            Async.map Result.fromChoice ma

        let fromOption (err : 'e) (opt : Option<'r>) : Flow<'r, 'e> =
            match opt with
            | Some r -> flow.Return r
            | None -> flow.Fail <| err

        let fromBool (failWith : 'err) (failOn : bool) : Flow<unit, 'err> =
            if failOn
                then flow.Return(())
                else flow.Fail failWith

        module Applicative =

            let (<!>) (f: 'a -> 'r) (x: Flow<'a, 'b>) : Flow<'r, 'b> =
                async {
                    let! value = x

                    return Result.map f value
                }

            let (<*>) (mf : Flow<'a -> 'r, 'b>) (ma: Flow<'a, 'b>) :  Flow<'r, 'b> =
                flow {
                    let! f = mf
                    let! a = ma
                    return (f a)
                }

            let (<*) (fa : Flow<'a, 'e>) (fb : Flow<'b, 'e>) : Flow<'a, 'e> =
                flow {
                    let! a = fa
                    let! _ = fb

                    return a
                }