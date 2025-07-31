namespace ParseFranDictionary

open System
open Serilog
open Serilog.Configuration


[<RequireQualifiedAccess>]
module Log =

    let loggerConfig =
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(".\ParseFranDict.log")
            .CreateLogger()

    
    let Logger = loggerConfig
