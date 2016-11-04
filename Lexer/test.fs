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

let lexer = new Lexer([("string", @"""([^""\r\n\\]*|\\.)*""");
                        ("sepor",@";");
                        ("identifier",@"[a-zA-Z_][\w]*");
                        ("number",@"[0-9]+(\.[0-9]+)?");
                        ("point",@"\.");
                        ("Error",@".")])

let result = lexer.GetTokenList(@"""sdf""dsefd""fdsfdsfsdf\""dsfdsf\rsdfsdf""dfsdf,dsf,0sdf\ew32\f\dsf\\r4..99a999,99.9""")

for name,str in result do
    printfn "%s: %s" name str


let trans1 = Transition(Opset['7';'8';'9'], Opset[1;2;3])
let trans2 = Transition(Opset['4';'5';'6'], Opset[1;2;3])
let trans3 = Transition(Opset['1';'2';'3'], Opset[1;2;3])
let trans4 = Transition(Opset['c';'v';'b'], Opset[0;2;3])

let trans = Transition.Join([trans3;trans2], trans1)

let tr = Transition.Single([trans1;trans2;trans3;trans4])

printfn "end"

