module Index

open Elmish
open Shared

type Model =
    { Expression: string
      Response: EvalResult }

type Msg =
    // Run button is pressed
    | EvaluateExpression
    // Expression text is changed
    | UpdateExpression of Expression
    // Received a response from api
    | GotResponse of EvalResult

let init () : Model * Cmd<Msg> =
    let model =
        { Expression = "1 + 1"
          Response = Ok "Click above to run!" }

    model, Cmd.none

open Fable.Remoting.Client

// use Fable.Remoting and Shared.fs code to create object to call api
let runnerApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IRunnerApi>

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | EvaluateExpression ->
        // create command that sends expression to api and calls back to GotResponse
        let evaluateExpressionOnServer =
            Cmd.OfAsync.perform runnerApi.eval model.Expression GotResponse

        model, evaluateExpressionOnServer
    | GotResponse res ->
        // update the model with api response
        { model with Response = res }, Cmd.none
    | UpdateExpression expression ->
        // text area changed, update the model with new value
        { model with Expression = expression }, Cmd.none

open Feliz
open Feliz.Bulma

let appBody (model: Model) (dispatch: Msg -> unit) =
    let resultBox =
        // text and text colour from the EvalResult
        let (result, colour) =
            match model.Response with
            | Ok ok -> ok, "black"
            | Error err -> err, "red"

        // add a p element with border
        Html.div [
            prop.style [
                style.padding 10
                style.border (1, borderStyle.double, colour)
                style.borderRadius 5
            ]
            prop.children [
                Html.p [
                    prop.style [ style.color colour ]
                    prop.text result
                ]
            ]
        ]

    // editable textarea that dispatches UpdateExpression message when changed
    let expressionTextArea =
        Bulma.textarea [
            prop.value model.Expression
            prop.onChange (fun x -> dispatch (UpdateExpression x))
        ]

    // button that sends expression to api when clicked
    let runButton =
        Bulma.button.a [
            color.isPrimary
            prop.onClick (fun _ -> dispatch EvaluateExpression)
            prop.text "Run"
        ]

    Bulma.container [
        Bulma.column [
            column.is6
            column.isOffset3
            prop.children [
                Bulma.box [
                    Bulma.field.div [
                        field.isGrouped
                        prop.children [
                            Bulma.control.p [
                                control.isExpanded
                                prop.children [ expressionTextArea ]
                            ]
                            Bulma.control.p [ runButton ]
                        ]
                    ]
                    Bulma.content [ resultBox ]
                ]
            ]
        ]
    ]

let navbar =
    Bulma.navbar [
        Bulma.container [
            Bulma.navbarBrand.div [
                Bulma.navbarItem.a [
                    prop.href "https://fsharp.github.io/fsharp-compiler-docs/"
                    navbarItem.isActive
                    prop.children [
                        Html.img [
                            prop.src "/favicon.png"
                            prop.alt "Logo"
                        ]
                    ]
                ]
                Bulma.navbarItem.div [
                    prop.children [ Html.p "Try FSCS" ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [ navbar ]
            Bulma.heroBody [
                appBody model dispatch
            ]
        ]
    ]
