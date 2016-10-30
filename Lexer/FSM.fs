module FSM

open Common

let ch' c = c + '\128'

let rec parsestr r = function
    | [] -> r
    | '\\'::[] -> failwith "wrong escape characters"
    | '\\'::c::tail -> 
        match c with
        | 't' -> parsestr (r @ ['\t']) tail
        | 'r' -> parsestr (r @ ['\r']) tail
        | 'n' -> parsestr (r @ ['\n']) tail
        | '\\' -> parsestr (r @ ['\\']) tail
        | '(' -> parsestr (r @ ['(']) tail
        | '[' -> parsestr (r @ ['[']) tail
        | '{' -> parsestr (r @ ['{']) tail
        | '|' -> parsestr (r @ ['|']) tail
        | '?' -> parsestr (r @ ['?']) tail
        | '*' -> parsestr (r @ ['*']) tail
        | '+' -> parsestr (r @ ['+']) tail
        | ')' -> parsestr (r @ [')']) tail
        | ']' -> parsestr (r @ [']']) tail
        | '}' -> parsestr (r @ ['}']) tail
        | '.' -> parsestr (r @ ['.']) tail
        | 'b' -> parsestr (r @ [ch' 'b']) tail
        | 'w' -> parsestr (r @ [ch' 'w']) tail
        | _ -> failwith "wrong escape characters"
    | c::tail ->
        match c with
        | '(' -> parsestr (r @ [ch' '(']) tail
        | '[' -> parsestr (r @ [ch' '[']) tail
        | '|' -> parsestr (r @ [ch' '|']) tail
        | '?' -> parsestr (r @ [ch' '?']) tail
        | '*' -> parsestr (r @ [ch' '*']) tail
        | '+' -> parsestr (r @ [ch' '+']) tail
        | ')' -> parsestr (r @ [ch' ')']) tail
        | ']' -> parsestr (r @ [ch' ']']) tail
        | '.' -> parsestr (r @ [ch' '.']) tail
        | _ -> parsestr (r @ [c]) tail
        

type FNA() = 
    member val StatusName:string = "" with get,set
    member val Transition:Transition = None with get,set

    member me.NotEndStatus with get() = match me.Transition with
                                        | None -> false
                                        | _ ->true
and
    Transition = 
        | Transitor of list<Set<char> * list<FNA>>
        | None


let bracket = function
    | '-'::_ -> failwith "wrong bracket string"
    | _::'-'::']'::_ -> failwith "wrong bracket string"
    | _::'-'::'-'::_ -> failwith "wrong bracket string"
    | c1::'-'::c2::_ -> 
        if c1 > c2 then
            failwith "wrong bracket string"
        else
            Set [c1..c2]

        

let rec ProcessRegex regex = 
    match regex with
    | ch::tail ->
        let input = 
            match ch with
            | '\\' -> car tail |> escape
            | '(' -> ()
            | '[' -> ()
            | '|' -> ()
            | '?' -> ()
            | '*' -> ()
            | '+' -> ()
            | ')' -> ()
            | ']' -> ()
            | '.'
            | _ -> ()
        
    


let rec CreateFNA name = function
    | h::[] -> { StatusName = name ; Transition = None }
    | _::tail -> { StatusName = null; Transition = CreateFNA name tail }
    | _ -> { StatusName = null;
            Transition = None }
