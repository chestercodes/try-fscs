module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open System
open System.IO
open System.Text
open System.Runtime.InteropServices
open FSharp.Compiler.Interactive.Shell

type FsiExpressionEvaluator() =
    // Initialize output and input streams
    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    // Build command line arguments & start FSI session
    let isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    let dotnetLocation = if isWindows then "C:\\Program Files\\dotnet\\dotnet.exe" else "/usr/bin/dotnet"

    let allArgs = [| dotnetLocation; "fsi"; "--noninteractive" |]
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    /// Evaluate expression & return the result
    member this.Evaluate = fun (expression: string) -> 
        let result, warnings = fsiSession.EvalExpressionNonThrowing(expression)
        
        match result with
        | Choice1Of2 v ->
            match v with
            | Some va -> sprintf "%A" va.ReflectionValue |> Ok
            | None -> Ok ""
        | Choice2Of2 _ ->
            let messages =
                warnings
                |> Array.map (fun x -> x.Message)
                |> fun arr -> String.Join(", ", arr)
            sprintf "Failed: %s" messages |> Error

let evaluator = FsiExpressionEvaluator()

open Shared

let evalExpression (expression: Expression) =
    async {
        System.Console.WriteLine(expression)
        return evaluator.Evaluate expression
    }

let notebookApi = { eval = evalExpression }

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
