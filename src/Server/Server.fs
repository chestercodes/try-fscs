module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

// open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Interactive.Shell

open System
open System.IO
open System.Text

// Initialize output and input streams
let sbOut = new StringBuilder()
let sbErr = new StringBuilder()
let inStream = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

// Build command line arguments & start FSI session
// let argv = [| "C:\\fsi.exe" |]
let argv = [| "C:\\Program Files\\dotnet\\dotnet.exe" |]
let allArgs = Array.append argv [|"fsi"; "--noninteractive"|]

let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

/// Evaluate expression & return the result
let evalExpression text =
  let result, warnings = fsiSession.EvalExpressionNonThrowing(text)
  
  match result with
  | Choice1Of2 v ->
    match v with
    | Some va -> sprintf "%A" va.ReflectionValue |> Ok
    | None -> Ok ""
  | Choice2Of2 exn ->
    let warning = warnings.[0].Message
    sprintf "Failed: %s" warning |> Error

/// Evaluate expression & return the result, strongly typed
let evalExpressionTyped<'T> (text) =
    match fsiSession.EvalExpression(text) with
    | Some value -> value.ReflectionValue |> unbox<'T>
    | None -> failwith "Got no result!"


open Shared

let runExpression (expression: string) =
    async {
        System.Console.WriteLine(expression)
        return evalExpression expression
    }

let notebookApi = { run = runExpression }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue notebookApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
