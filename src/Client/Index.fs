module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Expression: string; Response: Result<string, string> }

type Msg =
    | RunExpression
    | UpdateExpression of string
    | GotResponse of Result<string, string>

let notebookApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<INotebookApi>

let init () : Model * Cmd<Msg> =
    let model = { Expression = "1 + 1"; Response = Ok "Click above to run!" }
    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | RunExpression -> model, Cmd.OfAsync.perform notebookApi.run model.Expression GotResponse
    | GotResponse res -> { model with Response = res }, Cmd.none
    | UpdateExpression expression -> { model with Expression = expression }, Cmd.none

open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    let resultBox =
        match model.Response with
        | Ok ok -> Html.p ok
        | Error err -> Html.p err
    
    Bulma.box [
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Expression
                            prop.placeholder "What do you want to run?"
                            prop.onChange (fun x -> UpdateExpression x |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        //prop.disabled (Todo.isValid model.Input |> not)
                        prop.onClick (fun _ -> dispatch RunExpression)
                        prop.text "Run"
                    ]
                ]
            ]
        ]
        Bulma.content [
            resultBox
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "try_fscs"
                            ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]
