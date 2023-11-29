[<AutoOpen>]
module Utils

open System.Text.RegularExpressions

let (|Regex|_|) pattern input =
    let r = new Regex(pattern)
    let m = r.Match input
    if m.Success then Some m
    else None

let splitStr (sep: string) (str: string) =
    str.Split([|sep|], System.StringSplitOptions.None)

let splitBy v list =
  let yieldRevNonEmpty list = 
    if list = [] then []
    else [List.rev list]

  let rec loop groupSoFar list = seq { 
    match list with
    | [] -> yield! yieldRevNonEmpty groupSoFar
    | head::tail when v head ->
        yield! yieldRevNonEmpty groupSoFar
        yield! loop [] tail
    | head::tail ->
        yield! loop (head::groupSoFar) tail }
  loop [] list |> List.ofSeq
