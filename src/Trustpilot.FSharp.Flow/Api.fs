namespace Trustpilot.FSharp

module Api =
    open Newtonsoft.Json
    open Common
    open Execution
    open Logger
    open Logger.Types
    open Logger.Property
    open System.Net
    open AppFlow
    open System.Net.Http
    open System
    open System.Web.Http

    type GenericApiResponse =
        { [<JsonProperty("message")>]
          Message       : string
          [<JsonProperty("errorCode")>]
          ErrorCode     : int
          [<JsonProperty("correlationId")>]
          CorrelationId : string }

    type ApiErrorResponse = 
        { Response : GenericApiResponse 
          HttpCode : HttpStatusCode }
            interface IConvertToProperties with
                member this.Properties = 
                    [ property "HttpCode"       this.HttpCode
                      property "Message"        this.Response.Message
                      property "ErrorCode"      this.Response.ErrorCode
                      property "CorrelationId"  this.Response.CorrelationId ] 
    
    let asHttpError httpCode apiError =
        let response = new HttpResponseMessage(httpCode)
        let data = JsonConvert.SerializeObject(apiError)
        response.Content <- new StringContent(data, Text.Encoding.UTF8, "application/json")
        response
    
    let toApiError errorCode msg  : GenericApiResponse =
        { ErrorCode = errorCode; Message = msg; CorrelationId = System.Guid.NewGuid().ToString() }

    let toError errorCode msg httpCode =
        { Response = toApiError errorCode msg
          HttpCode = httpCode }

    let combineAppErrors (ex: AppError<ApiErrorResponse>) : ApiErrorResponse  =
        match ex with
        | AppError.StoreError     _ -> toError 1001 "Storage error"    HttpStatusCode.InternalServerError
        | AppError.RequestError   _ -> toError 1002 "Api error"        HttpStatusCode.InternalServerError
        | AppError.UnhandledError _ -> toError 1003 "Unexpected error" HttpStatusCode.InternalServerError
        | AppError.BusinessError  e -> e
        
    let toResponse (res : Result<'r, ApiErrorResponse>) : 'r =
        let raiseResponse (response : HttpResponseMessage) : 'r = 
            HttpResponseException response
            |> raise
        match res with
        | Success s -> s
        | Failure f -> raiseResponse <| asHttpError f.HttpCode f.Response

    let inline run logger (exe : ExecutionFlow<_,_,_>)  =
        exe
        |> Execution.onFailure (logger << App.executionAppToProperties)
        |> Execution.asAppFlow
        |> Flow.mapFailure combineAppErrors
        |> Async.map toResponse
        |> Async.StartAsTask