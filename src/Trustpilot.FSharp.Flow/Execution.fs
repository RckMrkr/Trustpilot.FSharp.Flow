namespace Trustpilot.FSharp

module Execution =   
    open Common
    open AppFlow

    type Execution<'input, 'output> =
        { Input : 'input
          Output : 'output
          RunningTime : System.TimeSpan }    
    
    type ExecutionFlow<'input, 'output, 'error> = Async<Execution<'input, Result<'output, AppError<'error>>>>

    let asExecutionFlow<'input,'output,'error>  (input : 'input) (flowFunc : 'input -> AppFlow<'output,'error>) : ExecutionFlow<'input, 'output, 'error> =
        flowFunc input
        |> AppFlow.catchUnhandled
        |> Async.time
        |> Async.map (fun (t,o) -> { Input = input; Output = o; RunningTime = t})
    
    let asAppFlow (m : ExecutionFlow<'input, 'output, 'error>) :  AppFlow<'output,'error>=
        Async.map (fun e -> e.Output) m


    let onFailure (action : Execution<'input, AppError<'error>> -> Async<unit>)  (exe : ExecutionFlow<_,_,_>) =   
        async {
            let! e = exe
            match e.Output with
            | Success _ -> ()
            | Failure f ->
                do! action { Input = e.Input; RunningTime = e.RunningTime; Output = f } 
            return e
        }

    let onSuccess (action : Execution<'input, 'output> -> Async<unit>)  (exe : ExecutionFlow<'input, 'output,_>) =   
        async {
            let! e = exe
            match e.Output with
            | Success s -> do! action { Input = e.Input; RunningTime = e.RunningTime; Output = s } 
            | Failure _ ->()                
            return e
        }