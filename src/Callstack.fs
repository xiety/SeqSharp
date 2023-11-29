module Callstack

open System
open System.Text.RegularExpressions

type CallstackItemFlat = { ObjectName: string; MethodName: string; Line: int }

type CallstackItem = { ObjectName: string; MethodName: string; Line: int; Children: CallstackItem list }

let private parseCall (line: string) =
        let mutable depth = 0
        let mutable from = 0
        let mutable i = 0
        seq {
            while i < line.Length do
                let c = line.[i]
                if c = '<' then
                    depth <- depth + 1
                elif c = '>' then
                    depth <- depth - 1
                elif c = '.' then
                    if depth = 0 then
                        yield line.[from..(i-1)]
                        from <- i + 1
                i <- i + 1
            if from <> i then
                yield line.[from..(i-1)]
        }

let private removeGenerics (text: string) : string =
    let n1 = text.IndexOf('<')

    if n1 >= 0 then
        text.[..(n1 - 1)]
    else
        text

let private parseItem (m: Match) =
    //let assembly = m.Groups.["assembly"].Value
    let call = m.Groups.["call"].Value
    //let parameters = m.Groups.["parameters"].Value
    let line = m.Groups.["line"].Value

    let calls = parseCall(call) |> Seq.toArray
    //let ns = String.Join(".", calls[..calls.Length - 3])
    let cls = removeGenerics calls.[calls.Length - 2]
    let method = removeGenerics calls.[calls.Length - 1]

    { CallstackItemFlat.ObjectName = cls; MethodName = method; Line = int(line) }

let private parse items =
    let regex s =
        match s with
        | Regex @"^[ >]\t(.*)$" a -> 
            match a.Groups.Item(1).Value with
                | Regex @"^(?<assembly>.+)!(?<call>.+)\((?<parameters>.+)?\) Line (?<line>\d+)\t(?<lang>.+)$" b
                    -> Some(parseItem b)
                | "[Native to Managed Transition]\t" -> None
                | "[Managed to Native Transition]\t" -> None
                | _ -> None
        | _ -> None

    items |> List.choose regex

let private reverse (root: CallstackItemFlat) items =
    let mapLine a =
        //first root will be eliminated by pairwise
        root::root::(List.rev a)
        |> List.pairwise
        |> List.map (fun a -> { (snd a) with Line = (fst a).Line } ) //move line number to child

    List.map mapLine items

let private nextLayer curstack (items: CallstackItemFlat list list) =
    let curlen = List.length curstack

    items
    |> List.filter (fun a -> List.length a > curlen && List.forall2 (=) (List.take curlen a) curstack)
    |> List.map (List.take (curlen + 1))
    |> List.distinct
    |> List.sortBy (fun a -> (List.last a).Line)

let rec private convert curstack curitem level items =
    let children = items
                   |> nextLayer curstack
                   |> List.map (fun a -> convert a (List.last a) (level + 1) items)

    {
        CallstackItem.ObjectName = curitem.ObjectName;
        MethodName = curitem.MethodName;
        Line = curitem.Line;
        Children = children
    }

let convertCallstack items =
    let root = { CallstackItemFlat.ObjectName = "User"; MethodName = "Do"; Line = 0 }
    items
    |> reverse root
    |> convert [root] root 0

let parseCallstack lines =
    let comments (a: string) = a.StartsWith("#")

    lines
    |> Array.filter (not << comments) |> List.ofArray
    |> splitBy String.IsNullOrEmpty   |> List.ofSeq
    |> List.map parse
    |> convertCallstack
