open Callstack
open Diagram
open Render
open Sort
open Svg

let public convert_lines input =
    input
    |> parseCallstack
    |> drawDiagram
    |> sortDiagram
    |> renderSvg
    |> svgToText

let public convert_text input =
    input
    |> splitStr "\n"
    |> convert_lines

#if !FABLE

[<EntryPoint>]
let main _ =
    let input = System.IO.File.ReadAllLines("..\docs\callstack.txt")
    let output = convert_lines input

    System.IO.File.WriteAllText("..\docs\callstack.svg", output)
    0

#endif
