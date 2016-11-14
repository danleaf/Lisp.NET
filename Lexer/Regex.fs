namespace Lexer

open Common
open NfaStatic
open System.Web.Script.Serialization

type Regex(name:string, dfa:DFA) =

    let dfa = dfa
    let name = name

    new (name:string, regexStr:string) =

        let cset = Opset<char>([])    
        let l2set (l:list<_>) = Opset(l)
        let c2set c = Opset<char>([c])

        let rec set_addl l (set:Opset<'a>) = 
            match l with
            | [] -> set
            | h::t -> set.Add(h) |> set_addl t

        let set_add a (set:Opset<'a>) = 
            set.Add(a)

        let spec c = char(int c + 128)
        let despec c = char(int c - 128)
        let (|Special|_|) c = if c > '\128' then Some(despec c) else None

        let titalset = ['\009';'\010';'\013'] @ ['\032'..'\126']
    
        let diffset (set:Opset<char>) = 
            Opset [ for c in titalset do
                        if not (set.Contains c) then
                            yield c]

        let getspec = function
            | 'w' -> 
                set_addl ['a'..'z'] cset 
                |> set_addl ['A'..'Z'] 
                |> set_addl ['0'..'9'] 
                |> set_add '_'

            | 'd' -> 
                set_addl ['0'..'9'] cset

            | '.' -> diffset (Opset['\n';'\r'])

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
                diffset set, rest
            | l -> 
                let set, rest = bracket' l cset
                set, rest

        let rec regex l nfa = 
            match l with
            | [] -> nfa,[]
            | ')'::tail -> nfa, tail 
            | '|'::tail ->
                let nextnfa,rest = regex tail EmptyNfa       
                parallel_nfa nfa nextnfa, rest
            | _ ->
            let curnfa, rest = 
                match l with
                | [] -> failwith ""
                | '('::tail ->
                    regex tail EmptyNfa
                | '['::tail ->  
                    let set, rest = bracket tail
                    cotr_nfa set, rest
                | '.'::tail -> cotr_nfa (getspec '.'), tail                
                | _ -> 
                match readl l with
                | [] -> failwith ""
                | Special c::tail -> cotr_nfa (getspec c), tail
                | c::tail -> cotr_nfa (c2set c), tail
            match rest with
            | '+'::tail -> cycle_nfa curnfa |> concat_nfa nfa |> regex tail
            | '?'::tail -> jump_nfa curnfa |> concat_nfa nfa |> regex tail
            | '*'::tail -> cycle_and_jump_nfa curnfa |> concat_nfa nfa |> regex tail
            | _ -> concat_nfa nfa curnfa |> regex rest

        let nfa,_ = regex (s2l regexStr) EmptyNfa
        Regex(name, DFA.FromNfa(nfa))        
        
    member me.DFA with get() = dfa
    member me.Name with get() = name

    member me.Match(input:string) = dfa.Match(input)

    member me.ToJson() =
        JavaScriptSerializer().Serialize(me.ToSerializerableStruct())

    static member FromJson(json:string) = 
        Regex.FromSerializerableStruct(JavaScriptSerializer().Deserialize<RegexRecord>(json))

    member me.ToSerializerableStruct() =
        { Name = name; Dfa = dfa.ToSerializerableStruct()}

    static member FromSerializerableStruct(data:RegexRecord) =
        Regex(data.Name, DFA.FromSerializerableStruct(data.Dfa))


and [<CLIMutable>] RegexRecord = { Name:string; Dfa:Dfa}
