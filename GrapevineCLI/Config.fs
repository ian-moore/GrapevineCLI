namespace GrapevineCLI

module Config =
    open Chessie.ErrorHandling
    open FSharp.Configuration
    open GrapevineCLI.CommandOptions

    type private GrapevineCliConfig = YamlConfig<"config.yaml">

    let getConnectionString () =
        let config = GrapevineCliConfig()
        try
            config.Load("config.yaml")
            ok config.ConnectionString
        with
        | ex -> fail ex

    let setConnectionString value =
        let config = GrapevineCliConfig()
        try
            config.Load("config.yaml")
            config.ConnectionString <- value
            config.Save()
            ok "Connection string updated."
        with
        | ex -> fail ex

    let getConfigList () =
        let printConfigKey k v =
            sprintf "%s: %s" k v |> ok

        getConnectionString () >>= printConfigKey "ConnectionString"

    let appProcess options =
        match options with
        | o when o.showConfigList -> getConfigList ()
        | o when o.connectionString <> "" -> setConnectionString o.connectionString
        | _ -> fail (exn "Unknown options for config command.")