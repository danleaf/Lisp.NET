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
    

let map = new Map<Set<char>, int>([cset, 0])
    

let rec regex' name l (fna:FNA) = 
    match l with
    | [] -> ()
    | _ ->

    let rest,set,neg,fna' = 
        match l with
        | [] -> failwith ""
        | '['::tail ->  
            let set, rest, neg = bracket tail
            rest, set, neg, 
            match rest with
            | [] -> fna.AddTransitor (set, new FNA(name), neg)
            | _ -> fna.AddTransitor (set, new FNA(), neg)
                    
        | _ -> 
                
        match readl l with
        | [] -> failwith ""
        | Special c::[] -> [], getspec c, false, fna.AddTransitor (getspec c, new FNA(name), false)
        | Special c::tail -> tail, getspec c, false, fna.AddTransitor (getspec c, new FNA(), false)
        | c::[] -> [], c2set c, false, fna.AddTransitor (c2set c, new FNA(name), false)
        | c::tail -> tail, c2set c, false, fna.AddTransitor (c2set c, new FNA(), false)

    match rest with
    | [] -> ()
    | '+'::[] -> 
        fna'.EndStatus<- true 
        fna'.AddTransitor (set, fna', neg) |> ignore
    | '+'::tail -> fna'.AddTransitor (set, fna', neg) |> regex' name tail
    | '?'::tail -> ()
    | '*'::tail -> ()
    | _ -> regex' name rest fna'
    

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


let rec mtch l (fna:FNA) = 
    matchone 0 l fna
