module Server

open System
open System.IO
open System.Text
open FSharp.Compiler.Interactive.Shell

type FsiExpressionEvaluator() =
    // Initialize output and input streams
    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    // differs across platforms
    let dotnetLocation = Environment.GetEnvironmentVariable("DOTNET_ROOT")

    // Build command line arguments & start FSI session
    let allArgs = [| dotnetLocation; "fsi"; "--noninteractive" |]
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

    let fsiSession =
        FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    // Evaluate expression & return the result
    member this.Evaluate(expression: string) =
        let result, warnings = fsiSession.EvalExpressionNonThrowing(expression)
        match result with
        // sucessful execution
        | Choice1Of2 valueOrUnit ->
            let expressionValue = 
                match valueOrUnit with
                // return value as string
                | Some fsharpValue -> sprintf "%A" fsharpValue.ReflectionValue
                // unit value, return blank string
                | None -> ""
            Ok expressionValue
        // execution failed
        | Choice2Of2 _ ->
            // join list of error messages into string
            let messages =
                warnings
                |> Array.map (fun x -> x.Message)
                |> fun arr -> String.Join(", ", arr)

            sprintf "Failed: %s" messages |> Error

let evaluator = FsiExpressionEvaluator()

open Shared

// implement shared api contract
let runnerApi: IRunnerApi =
    { 
        eval = fun expression ->
            async {
                return evaluator.Evaluate expression
            }
    }

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

// create Fable.Remoting api from IRunnerApi and shared route builder
let fableRemotingApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue runnerApi
    |> Remoting.buildHttpHandler

open Saturn

// create saturn app and run
let app =
    application {
        url "http://0.0.0.0:8085"
        use_router fableRemotingApi
        memory_cache
        use_static "public"
        use_gzip
    }

run app
