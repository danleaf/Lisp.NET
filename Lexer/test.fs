open Lexer
open DfaModule
open Common

let r = new System.Text.RegularExpressions.Regex(@"[a-zA-Z_][\w]*")
let m = r.Match(@"djfhjf \n jfjdf \n 232.0ff \t kdjfe 0.12.1")

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

//let nfa = regex "int" (s2l @"""[^\r\n]*""")
//let nfa = regex "int" (s2l @"112*1")

//let regstr = @"11221"
//
//let name,len = mtchshort (s2l regstr) nfa
//printfn "%s: %s" name (regstr.Substring(0,len))


let regexs = [Regex(@";"),"sepor";
                Regex(@"[\r\n \t]+"),"blank";
                Regex(@"[a-zA-Z_][\w]*"),"identifier";
                Regex(@"[0-9]+(.[0-9]+)?"),"number";
                Regex("[\000-\127]"),"Error"]

let rec matchone (str:string) (regexs:(Regex*string) list) =
    match regexs with
    | [] -> "Unkown","",0
    | (reg, name)::tail -> 
        let (r,len) = matchlong str reg.DFA
        if len > 0 then
            name,r,len
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

matchall @"
        public static void Main()
        {

            object[] ctorParams = new object[2];

            string myX = ""1"";
            string myY = ""2"";

            Console.WriteLine(""---"");

            ctorParams[0] = Convert.ToInt32(myX);
            ctorParams[1] = Convert.ToInt32(myY);

            Type ptType = CreateDynamicType();

            object ptInstance = Activator.CreateInstance(ptType, ctorParams);
            ptType.InvokeMember(""WritePoint"",
                    BindingFlags.InvokeMethod,
                    null,
                    ptInstance,
                    new object[0]);
        }" regexs




let trans1 = Transition(OpSet['7';'8';'9'], OpSet[1;2;3])
let trans2 = Transition(OpSet['4';'5';'6'], OpSet[1;2;3])
let trans3 = Transition(OpSet['1';'2';'3'], OpSet[1;2;3])
let trans4 = Transition(OpSet['c';'v';'b'], OpSet[0;2;3])

let trans = Transition.Join([trans3;trans2], trans1)

let tr = Transition.Single([trans1;trans2;trans3;trans4])

printfn "end"

