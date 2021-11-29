namespace Shared

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

// type aliases for clarity/documentation
type Expression = string
type EvalValue = string
type EvalError = string
type EvalResult = Result<EvalValue, EvalError>

// used by Fable.Remoting to define communication types
type IRunnerApi =
    {
        eval: Expression -> Async<EvalResult>
    }
