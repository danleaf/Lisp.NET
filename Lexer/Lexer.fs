module Lexer

open System.Text
open System.Linq

type Status =
    | INIT
    | END
    | IDENTIFIER
    | NUMBER

let (|Letter|_|) = function
    | x when x <= 'z' && x >= 'a' -> Some()
    | x when x <= 'Z' && x >= 'A' -> Some()
    | _ -> None

let (|Digit|_|) = function
    | x when x <= '9' && x >= '0' -> Some()
    | _ -> None

let inittrans = function
    | Letter -> IDENTIFIER
    | Digit -> NUMBER
    | _ -> INIT
    
let idtrans = function
    | Letter -> IDENTIFIER
    | Digit -> IDENTIFIER
    | _ -> END

let rec StatusTransition r len input currStatus = 
    if currStatus = END then
        (r, len)
    else  
        match input with 
        | [] -> (r, len)
        | h::tail -> 
            let s =
                match currStatus with 
                | INIT -> inittrans h
                | IDENTIFIER -> idtrans h
                | _ -> END
            let r' = if s = IDENTIFIER then [h] else []
            StatusTransition (r @ r') (len+1) tail s

let r = StatusTransition [] 0 ("  a0/".ToCharArray().ToList()) INIT