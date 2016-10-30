module Common

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