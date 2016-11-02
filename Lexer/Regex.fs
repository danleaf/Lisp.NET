module Regex

open Common
open NfaModule
open DfaModule

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

let diffset (set:Set<char>) = 
    Set[for c in '\000'..'\127' do
            if not (set.Contains c) then
                yield c]


let bracket = function
    | '^'::tail -> 
        let set, rest = bracket' tail cset
        diffset set, rest
    | l -> 
        let set, rest = bracket' l cset
        set, rest

let rec _regex name l nfa = 
    match l with
    | [] -> nfa,[]
    | ')'::tail -> nfa, tail 
    | '|'::tail ->
        let nextnfa,rest = _regex name tail EmptyNfa       
        parallel_nfa nfa nextnfa, rest
    | _ ->
    let curnfa, rest = 
        match l with
        | [] -> failwith ""
        | '('::tail ->
            _regex name tail EmptyNfa
        | '['::tail ->  
            let set, rest = bracket tail
            cotr_nfa set, rest
        | _ -> 
        match readl l with
        | [] -> failwith ""
        | Special c::tail -> cotr_nfa (getspec c), tail
        | c::tail -> cotr_nfa (c2set c), tail
    match rest with
    | '+'::tail -> cycle_nfa curnfa |> concat_nfa nfa |> _regex name tail
    | '?'::tail -> jump_nfa curnfa |> concat_nfa nfa |> _regex name tail
    | '*'::tail -> cycle_and_jump_nfa curnfa |> concat_nfa nfa |> _regex name tail
    | _ -> concat_nfa nfa curnfa |> _regex name rest
        


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

//let nfa = regex "int" (s2l @"""[^\r\n]*""")
//let nfa = regex "int" (s2l @"112*1")

//let regstr = @"11221"
//
//let name,len = mtchshort (s2l regstr) nfa
//printfn "%s: %s" name (regstr.Substring(0,len))

let nfa,_ = _regex "int" (s2l @"(1|2)*") EmptyNfa

let map = compute_e_close NfaStatic.NodeMap

let mm = 
    match nfa with
    | Nfa(st,ed) -> 
        let stset = search_e_close st
        ctor_next_set stset
    | _ -> failwith ""

printfn "end"