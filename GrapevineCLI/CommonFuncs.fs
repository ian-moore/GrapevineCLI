namespace GrapevineCLI

module CommonFuncs =
    open System

    let datestring (d:DateTime) =
        if d <> DateTime.MinValue
        then d.ToShortDateString()
        else ""

    let substring (text:string) startPos length =
        text.Substring(startPos, length)
    
    let truncateWithOverflow (overflowText:string) (length:int) (text:string) =
        if text.Length > length
        then
            let subtext =  substring text 0 <| length - overflowText.Length
            sprintf "%s%s" subtext overflowText
        else text

    let truncateEllipsis (length:int) (text:string) =
        truncateWithOverflow "..." length text