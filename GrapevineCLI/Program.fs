open CommandLine
open Chessie.ErrorHandling
open GrapevineCLI
open GrapevineCLI.CommandOptions

let parseArgs (args:string[]) =
    let parseResult = Parser.Default.ParseArguments<ConfigOptions, ItemOptions>(args)
    match parseResult with
    | :? Parsed<obj> as command -> ok command.Value
    | :? NotParsed<obj> -> fail <| exn "Command line arguments not valid."

let runCommand (command:obj) =
    match command with
    | :? ConfigOptions as co -> Config.appProcess co
    | :? ItemOptions as io -> Items.appProcess io

let displayOutput (data, messages) =
    printf "%s" data

let printErrors (errors:exn list) =
    List.iter (fun e -> printf "%A" e) errors

let returnExitCode result =
    match result with
    | Pass _ -> 0
    | _ -> 1

[<EntryPoint>]
let main argv = 
    parseArgs argv
    >>= (runCommand >> eitherTee displayOutput printErrors)
    |> returnExitCode