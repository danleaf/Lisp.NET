namespace Lexer

type Lexer(regexs:Regex list) =
    let regexs = regexs
    
    let rec matchone (str:string) (regexs:Regex list) =
        match regexs with
        | [] -> "Unkown","",0
        | reg::tail -> 
            let r = reg.Match str
            if r.Length > 0 then
                reg.Name,r.Value,r.Length
            else
                matchone str tail

    let rec matchall (str:string) result =
        let name,s,len = matchone str regexs
        let newresult = if len > 0 then result @ [name,s] else result
        if len > 0 && str.Length > len then
            newresult |> matchall (str.Substring(len)) 
        else
            newresult

    member me.GetTokenList(input:string) = 
        matchall input []

    new (regexstrs:(string*string) list) = 
        Lexer([for name,str in regexstrs do yield Regex(name, str)])

