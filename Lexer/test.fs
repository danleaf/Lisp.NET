open Lexer
open Common

let r = new System.Text.RegularExpressions.Regex(@"""([^""\\]*|\\.)*""")
let m = r.Match(@"""sdfdsefdfdsfdsfsdf\""dsfds\fsdfsdf""")

//printfn "input: %s" m.Value

let rec printset' = function
    | [] -> ()
    | h::t -> 
        printf "%c " h
        printset' t

let printset (set:Set<_>) = 
    printf "set: "
    List.ofSeq set |> printset'
    printfn ""



let regexs = [  Regex(@"""([^""\r\n\\]*|\\.)*"""),"string";
                Regex(@";"),"sepor";
                Regex(@"[\r\n \t]+"),"blank";
                Regex(@"[a-zA-Z_][\w]*"),"identifier";
                Regex(@"\."),"point";
                Regex(@"[0-9]+(\.[0-9]+)?"),"number";
                Regex(@"."),"Error"]

let rec matchone (str:string) (regexs:(Regex*string) list) =
    match regexs with
    | [] -> "Unkown","",0
    | (reg, name)::tail -> 
        let r = reg.Match str
        if r.Length > 0 then
            name,r.Value,r.Length
        else
            matchone str tail

let rec matchall (str:string) (regexs:(Regex*string) list) =
    let name,r,len = matchone str regexs
    printfn "%s: %s" name (r.Replace("\r","\\r").Replace("\n","\\n").Replace("\t","\\t").Replace(" ","\\b"))
    if len = 0 then
        if str.Length > 1 then
            matchall (str.Substring(1)) regexs
    else
        if str.Length > len then
            matchall (str.Substring(len)) regexs

matchall @"""sdf""dsefd""fdsfdsfsdf\""dsfdsf\rsdfsdf""dfsdf,dsf,0sdf\ew32\f\dsf\\r4..99a999,99.9""" regexs




let trans1 = Transition(Opset['7';'8';'9'], Opset[1;2;3])
let trans2 = Transition(Opset['4';'5';'6'], Opset[1;2;3])
let trans3 = Transition(Opset['1';'2';'3'], Opset[1;2;3])
let trans4 = Transition(Opset['c';'v';'b'], Opset[0;2;3])

let trans = Transition.Join([trans3;trans2], trans1)

let tr = Transition.Single([trans1;trans2;trans3;trans4])

printfn "end"

