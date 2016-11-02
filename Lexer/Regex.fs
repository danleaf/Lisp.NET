module Regex

open Common
open FNA

let cset = Set<char>([])

let rec set_addl l (set:Set<'a>) = 
    match l with
    | [] -> set
    | h::t -> set.Add(h) |> set_addl t

let set_add a (set:Set<'a>) = 
    set.Add(a)

let spec c = char(int c + 128)
let despec c = char(int c - 128)
let (|Special|_|) c = if c > '\128' then Some(despec c) else None

let getspec = function
    | 'w' -> 
        set_addl ['a'..'z'] cset 
        |> set_addl ['A'..'Z'] 
        |> set_addl ['0'..'9'] 
        |> set_add '_'

    | 'd' -> 
        set_addl ['0'..'9'] cset

    | _ -> failwith "wrong special character"

let escape = function
    | 't' -> '\t'
    | 'r' -> '\r'
    | 'n' -> '\n'
    | '\\' -> '\\'
    | '(' -> '('
    | '[' -> '['
    | '{' -> '{'
    | '|' -> '|'
    | '?' -> '?'
    | '*' -> '*'
    | '+' -> '+'
    | '-' -> '-'
    | ')' -> ')'
    | ']' -> ']'
    | '}' -> '}'
    | '.' -> '.'
    | 'w' -> spec 'w'
    | 'd' -> spec 'd'
    | _ -> failwith "wrong escape character"

let readl = function
    | [] -> []
    | '\\'::[] -> failwith "unexpected escape terminate"
    | '\\'::c::tail -> escape c :: tail
    | l -> l


let rec bracket' l set = 
    match l with
    | '-'::_ -> failwith "wrong x-y expression"
    | ']'::tail -> set, tail
    | _ ->

    match readl l with
    | [] -> failwith "[] Set is not finish"
    | Special _::'-'::_ -> failwith "wrong x-y expression"
    | Special c::tail ->
        getspec c |> bracket' tail

    | beginc::'-'::tail ->
        match tail with
        | '-'::_ | ']'::_ -> failwith "wrong x-y expression"
        | _ ->

        match readl tail with
        | [] -> failwith "[] Set is not finish"
        | Special _::_ -> failwith "wrong x-y expression"
        | endc::tail -> 
            if endc < beginc then  failwith "wrong x-y expression, y must >= x"
            set |> set_addl [beginc..endc] |> bracket' tail

    | c::tail ->
        set |> set_add c |> bracket' tail


let bracket = function
    | '^'::tail -> 
        let set, rest = bracket' tail cset
        set, rest, true
    | l -> 
        let set, rest = bracket' l cset
        set, rest, false



let rec regex_ name l fnas = 
    match l with
    | [] -> 
        setend fnas name
        fnas, []
    | _ ->

    let rest,set,neg,fna_ = 
        match l with
        | [] -> failwith ""
        | '['::tail ->  
            let set, rest, neg = bracket tail
            rest, set, neg, addtrans fnas set (new FNA()) neg
                    
        | _ -> 
                
        match readl l with
        | [] -> failwith ""
        | Special c::tail -> tail, getspec c, false, addtrans fnas (getspec c) (new FNA()) false
        | c::tail -> tail, c2set c, false, addtrans fnas (c2set c) (new FNA()) false

    match rest with
    | '+'::tail -> [fna_.AddTransitor (set, fna_, neg)] |> regex_ name tail
    | '*'::tail -> fnas @ [fna_.AddTransitor (set, fna_, neg)] |> regex_ name tail
    | '?'::tail -> fnas @ [fna_] |> regex_ name tail
    | _ -> regex_ name rest [fna_]
    

let regex name l =
    let fna = new FNA()
    regex_ name l [fna] |> ignore
    fna

let rec trans_ c (fnas:FNA list) r =
    match fnas with
    | [] -> r
    | fna::tail -> (r @ fna.Transit c) |> trans_ c tail

let rec trans fnas c =
    trans_ c fnas []

let rec findend_ (fnas:FNA list) r =
    match fnas with
    | [] -> r
    | fna::tail -> (r @ [if fna.EndStatus then yield fna]) |> findend_ tail

let findend fnas = findend_ fnas []    

let rec matchone len l fnas long =
    match l with
    | head::tail  ->
        if long then
            let nextfnas = trans fnas head
            if nextfnas = [] then
                let endfnas = findend fnas
                if endfnas = [] then
                    "",0
                else
                    nameof(car endfnas),len
            else
                matchone (len+1) tail nextfnas long
        else
            let endfnas = findend fnas
            if not (endfnas = []) then
                nameof(car endfnas),len
            else
                let nextfnas = trans fnas head
                if nextfnas = [] then
                    let endfnas = findend fnas
                    if endfnas = [] then
                        "",0
                    else
                        nameof(car endfnas),len
                else
                    matchone (len+1) tail nextfnas long
    | [] -> 
        let endfnas = findend fnas
        if endfnas = [] then
            "",0
        else
            nameof(car endfnas),len


let mtchlong l (fna:FNA) = matchone 0 l [fna] true

let mtchshort l (fna:FNA) = matchone 0 l [fna] false


let r = new System.Text.RegularExpressions.Regex("[0-Z]+")
let m = r.Match("123Aa")

let rec printset' = function
    | [] -> ()
    | h::t -> 
        printf "%c " h
        printset' t

let printset (set:Set<_>) = 
    printf "set: "
    List.ofSeq set |> printset'
    printfn ""

//let fna = regex "int" (s2l @"""[^\r\n]*""")
let fna = regex "int" (s2l @"112*1")

let regstr = @"11221"

let name,len = mtchshort (s2l regstr) fna
printfn "%s: %s" name (regstr.Substring(0,len))
