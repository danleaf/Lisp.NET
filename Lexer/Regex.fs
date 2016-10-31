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

    let l' = readl l
    match l' with
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
    

let map = new Map<Set<char>, int>([cset, 0])
    

let rec regex' name l (fna:FNA) = 
    match l with
    | '['::tail ->  
        let set, rest, neg = bracket tail
        if tail = [] then
            fna.AddTransitor (set, new FNA(name), neg) |> ignore
        else 
            fna.AddTransitor (set, new FNA(), neg) |> regex' name rest 

    | c::[] ->
        fna.AddTransitor (c2set c, new FNA(name), false) |> ignore

    | c::tail ->
        fna.AddTransitor (c2set c, new FNA(), false) |> regex' name tail

    | [] -> ()

let regex name l =
    let fna = FNA()
    regex' name l fna
    fna

let rec matchone len l (fna:FNA) =
    match l with
    | head::tail  ->
        let fnas = fna.Transit head
        if fnas = [] then
            if fna.EndStatus then
                fna.StatusName,len
            else
                "",0
        else
            let fna = car fnas
            matchone (len+1) tail fna
    | [] -> 
        if fna.EndStatus then
            fna.StatusName,len
        else
            "", 0   


let rec mtch idx l (fna:FNA) = 
    let name, len = matchone 0 l fna
    match l with
    | [] | _::[] -> name, idx, len
    | _ ->

    if len = 0 then
        mtch (idx+1) (cdr l) fna
    else
        name, idx, len


let rec matchall l (fna:FNA) = 
    let name,idx,len = mtch 0 l fna
    if len = 0 then
        printfn "can't parse"
    else
        printfn "%s: %s" name ((l2s l).Substring(idx,len))
        let rest = cdrn (idx + len) l
        if not (rest = []) then
            matchall rest fna