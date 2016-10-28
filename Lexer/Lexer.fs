//module Lexer

open System

type Token = 
    | Empty
    | Identifier
    | Number
    | Error

type Status =
    | INIT
    | END
    | IDENTIFIER
    | NUMBER
    | ERROR

let cdr = List.tail
let car = List.head

let rec cdrn n l = 
    if n = 0 then 
        l
    else 
        match l with
        | [] -> []
        | _::tail -> cdrn (n-1) tail
        

let str2list (str:string) = 
    str.ToCharArray() |> List.ofArray 

let list2str (cl:char list) = 
    new string(cl |> List.toArray)

let (|Letter|_|) = function
    | x when x <= 'z' && x >= 'a' -> Some()
    | x when x <= 'Z' && x >= 'A' -> Some()
    | _ -> None

let (|Digit|_|) = function
    | x when x <= '9' && x >= '0' -> Some()
    | _ -> None

let (|Point|_|) = function
    | x when x = '.' -> Some()
    | _ -> None

let GetToken = function
    | INIT -> Empty
    | IDENTIFIER -> Identifier
    | NUMBER -> Number
    | ERROR -> Error
    | _ -> Error

let rec ReadToken' r len status = function
    | h::t -> 
        let next_status,is_valid_char = 
            match status with
            | INIT -> 
                match h with
                | Letter -> IDENTIFIER, true
                | Digit -> NUMBER, true
                | _ -> INIT, false

            | IDENTIFIER -> 
                match h with
                | Letter -> IDENTIFIER, true
                | Digit -> IDENTIFIER, true
                | _ -> END, false

            | NUMBER -> 
                match h with
                | Letter -> ERROR, true
                | Digit -> NUMBER, true
                | _ -> END, false

            | ERROR -> 
                match h with
                | Letter -> ERROR, true
                | Digit -> ERROR, true
                | _ -> END, false

            | _ -> END, false

        let r' = if is_valid_char then [h] else []
        if next_status = END then
            GetToken status, (r @ r'), len
        elif t = [] then
            GetToken next_status, (r @ r'), (len + 1)
        else
            ReadToken' (r @ r') (len + 1) next_status t

    | [] -> Empty, r, len        

let ReadToken input = 
    let t, r, len = ReadToken' [] 0 INIT input
    t, (list2str r), (cdrn len input), len

let rec Parse input =
    let token, word, last,len = ReadToken input
    printfn "%s\t%A\t%d" word token len
    if last = [] then
        ()
    else
        Parse last


let input = str2list ";;   ;"
Parse input
