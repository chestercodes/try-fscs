module Index

open Elmish
open Fable.Remoting.Client
open Shared

let notebookApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<INotebookApi>

type Model =
    { Expression: string
      Response: EvalResult }

type Msg =
    | EvaluateExpression
    | UpdateExpression of Expression
    | GotResponse of EvalResult

let init () : Model * Cmd<Msg> =
    let model =
        { Expression = "1 + 1"
          Response = Ok "Click above to run!" }

    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | EvaluateExpression ->
        let evaluateExpressionOnServer =
            Cmd.OfAsync.perform notebookApi.eval model.Expression GotResponse

        model, evaluateExpressionOnServer
    | GotResponse res -> { model with Response = res }, Cmd.none
    | UpdateExpression expression -> { model with Expression = expression }, Cmd.none

open Feliz
open Feliz.Bulma

let appBody (model: Model) (dispatch: Msg -> unit) =
    let (result, colour) =
        match model.Response with
        | Ok ok -> ok, "black"
        | Error err -> err, "red"

    let resultBox =
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

    let expressionTextArea =
        Bulma.textarea [
            prop.value model.Expression
            prop.onChange (fun x -> dispatch (UpdateExpression x))
        ]

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
