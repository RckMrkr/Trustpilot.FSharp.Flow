namespace Trustpilot.FSharp

module AppFlow =
    open Flow
    open System
    open Common

    type ApiResponseError<'a> =
        | ApiException of Exception * string
        | ApiError of 'a

    type StorageError<'a> =
        | StorageException of Exception * string
        | StorageError of 'a

    type StoreFlow<'a, 'e> = Flow<'a, StorageError<'e>>
    type ApiFlow<'a, 'e> = Flow<'a, ApiResponseError<'e>>

    type AppError<'a> =
        | StoreError of Exception * string
        | RequestError of Exception * string
        | UnhandledError of Exception
        | BusinessError of 'a

    type AppFlow<'a, 'e> = Flow<'a, AppError<'e>>

        
    let fromStorageError f =
        let errorMapping = function
            | StorageException (ex, msg) -> StoreError (ex, msg)
            | StorageError error -> BusinessError <| f error
        Result.mapFailure errorMapping

    let fromApiError f  =
        let errorMapping = function
            | ApiException (ex, msg) -> RequestError (ex, msg)
            | ApiError error ->  BusinessError <| f error
        Result.mapFailure errorMapping

    let mapStorageError f = Async.map (fromStorageError f)

    let mapApiError f = Async.map (fromApiError f)

    let catchUnhandled<'r,'e> (fa : AppFlow<'r,'e>) : AppFlow<'r,'e> =
        let mapException e =
            match e with
            | Choice1Of2 err -> err
            | Choice2Of2  exn -> UnhandledError exn
        fa
        |> Flow.catch
        |> Flow.mapFailure mapException