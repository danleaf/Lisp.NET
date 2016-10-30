//module Lexer

open Common
open System
open System.Text.RegularExpressions

type Token =
    | Error 
    | Empty
    | Identifier
    | Number
    | Operator
    | Seperator

type Status =
    | ERROR
    | INIT
    | END
    | IDENTIFIER
    | NUMBER
    | OPERATOR
    | SEPERATOR




let (|Blank|_|) = function
    | ' ' | '\t' | '\n' | '\r' -> Some()
    | _ -> None



let (|Point|_|) = function
    | '.' -> Some()
    | _ -> None

let (|Comma|_|) = function
    | ',' -> Some()
    | _ -> None

let (|OpenParen|_|) = function
    | '(' -> Some()
    | _ -> None

let (|CloseParen|_|) = function
    | ')' -> Some()
    | _ -> None

let (|OpenBracket|_|) = function
    | '[' -> Some()
    | _ -> None

let (|CloseBracket|_|) = function
    | ']' -> Some()
    | _ -> None

let (|Semic|_|) = function
    | ';' -> Some()
    | _ -> None

let (|OpenBrace|_|) = function
    | '{' -> Some()
    | _ -> None

let (|CloseBrace|_|) = function
    | '}' -> Some()
    | _ -> None

let (|Quotes|_|) = function
    | ''' -> Some()
    | _ -> None

let (|DualQuotes|_|) = function
    | '"' -> Some()
    | _ -> None

let (|ExprSign|_|) = function
    | '+' | '-' | '*' | '/' | '&' | '|' | '^' | '%' | '!' | '>' | '<'  -> Some()
    | _ -> None

let (|Sider|_|) = function
    | '(' | ')' | '[' | ']' | '{' | '}' -> Some()
    | _ -> None

let (|Seper|_|) = function
    | '(' | ')' | '[' | ']' | '{' | '}' | ';' -> Some()
    | _ -> None

let GetToken = function
    | INIT -> Empty
    | IDENTIFIER -> Identifier
    | NUMBER -> Number
    | SEPERATOR -> Seperator
    | OPERATOR -> Operator
    | _ -> Error

let rec ReadToken' r len status = function
    | [] -> Empty, r, len        
    | h::t -> 
        let next_status,is_valid_char = 
            match status with
            | INIT -> 
                match h with
                | Letter -> IDENTIFIER, true
                | Digit -> NUMBER, true
                | ExprSign -> OPERATOR, true
                | Blank -> INIT, false
                | Seper -> SEPERATOR,true
                | _ -> ERROR, true

            | IDENTIFIER -> 
                match h with
                | Letter | Digit | ExprSign -> IDENTIFIER, true
                | _ -> END, false

            | NUMBER -> 
                match h with
                | Letter -> ERROR, true
                | Digit -> NUMBER, true
                | _ -> END, false

            | OPERATOR -> 
                match h with
                | Letter | Digit -> IDENTIFIER, true
                | ExprSign -> OPERATOR, true
                | _ -> END, false

            | SEPERATOR -> END, false

            | ERROR -> 
                match h with
                | Blank -> END, false
                | _ -> ERROR, false

            | _ -> END, false

        let r' = if is_valid_char then [h] else []
        if next_status = END then
            GetToken status, (r @ r'), len
        elif t = [] then
            GetToken next_status, (r @ r'), (len + 1)
        else
            ReadToken' (r @ r') (len + 1) next_status t


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


let input = str2list "(def *v*)"
Parse input

let regex = new Regex("[0-Z]+")
let m = regex.Match("123Aa")