namespace Lexer

open System.IO
open System.Web.Script.Serialization
open Common

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
            
    new (regexs:Regex System.Collections.Generic.List) = 
        Lexer [for regex in regexs do yield regex]

    new (regexstrs:(string*string) list) = 
        Lexer [for name,str in regexstrs do yield Regex(name, str)]

    member me.GetTokenList(input:string) = 
        let rs = matchall input []
        let l = new System.Collections.Generic.List<string*string>()
        for r in rs do l.Add(r)
        l
        
    member me.ToJson() =
        JavaScriptSerializer().Serialize(me.ToSerializerableStruct())

    static member FromJson(json:string) = 
        Lexer.FromSerializerableStruct(JavaScriptSerializer().Deserialize<LexerRecord>(json))

    member me.ToSerializerableStruct() =
        let data = { Regexes = new System.Collections.Generic.List<RegexRecord>() }
        for regex in regexs do
            data.Regexes.Add(regex.ToSerializerableStruct())
        data

    static member FromSerializerableStruct(data:LexerRecord) =
        Lexer [for regex in data.Regexes do yield Regex.FromSerializerableStruct(regex)]

    member me.SaveToFile(path:string) = 
        use fs = new FileStream(path, FileMode.Create)
        use writer = new StreamWriter(fs)
        writer.Write(me.ToJson())

    static member LoadFromFile(path:string) = 
        use fs = new FileStream(path, FileMode.Open)
        use reader = new StreamReader(fs)
        Lexer.FromJson(reader.ReadToEnd())


and [<CLIMutable>] LexerRecord = { Regexes:RegexRecord System.Collections.Generic.List }