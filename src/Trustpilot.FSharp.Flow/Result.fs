namespace Trustpilot.FSharp
module Result =
    
    type Result<'success, 'failure> = Success of 'success
                                    | Failure of 'failure

    let mapFailure f =
        function
        | Success x -> Success x
        | Failure x -> Failure <| f x

    let map f =
        function
        | Success x -> Success <| f x
        | Failure x -> Failure x

    let fromChoice =
        function
        | Choice1Of2 a -> Success a
        | Choice2Of2 e -> Failure e

