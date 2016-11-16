namespace Lexer

open System.Web.Script.Serialization
open Common

type DfaTmpNode(id:int, isEnd:bool, transitors:Transitor<char, int> list) = 
    let id = id
    let isEnd = isEnd
    let transitors = transitors  

    new (tfa:TFA) =
        DfaTmpNode(tfa.ID, tfa.IsEnd, 
            [for trans in tfa.Transitors do
                yield Transitor(trans.Input, trans.Dest.ID)])

    new (record:DfaRecordNode) =
        DfaTmpNode(record.ID, record.IsEnd, 
            [for trans in record.Transitors do
                yield Transitor(Opset (CList2FList trans.Input), trans.Dest)])
        

    member me.ID with get() = id
    member me.IsEnd with get() = isEnd
    member me.Transitors with get() = transitors

and DfaTmp(tfa:TFA) =
    let rec cotr_map (tfa:TFA) (map:Map<int,DfaTmpNode>) =
        if map.ContainsKey(tfa.ID) then
            map
        else
            let rec cotr_set2 (tfas:Transitor<char,TFA> list) (map:Map<int,DfaTmpNode>) =
                match tfas with
                | [] -> map
                | tfa::rest -> cotr_map tfa.Dest map |> cotr_set2 rest
            let dfaNode = DfaTmpNode(tfa)
            map.Add(dfaNode.ID, dfaNode) |> cotr_set2 tfa.Transitors

    let map = Map[] |> cotr_map tfa 
    let startId = tfa.ID
    let start = map.[tfa.ID]

    member me.Map = map
    member me.Start = start
    member me.StartId = startId

and DFA(id:int, tmpNodeMap:Map<int,DfaTmpNode>, map:System.Collections.Generic.Dictionary<int, DFA>) as this =
    let id = id
    do map.Add(id, this)

    let tmpNode = tmpNodeMap.[id]
    let isEnd = tmpNode.IsEnd

    let transitors = 
        [for trans in tmpNode.Transitors do
            yield Transitor(trans.Input, 
                if map.ContainsKey(trans.Dest) then 
                    map.[trans.Dest]
                else
                    DFA(trans.Dest, tmpNodeMap, map))]

    new (id:int, tmpNodeMap:Map<int,DfaTmpNode>) =
        DFA(id, tmpNodeMap, new System.Collections.Generic.Dictionary<int, DFA>())

    new (tfa:TFA) = 
        let tmpDfa = DfaTmp(tfa)
        DFA(tmpDfa.StartId, tmpDfa.Map)

    new (nfa:NFA) = DFA(TFA(nfa))
        
    member me.Transitors with get() = transitors
    member me.ID with get() = id
    member me.IsEnd with get() = isEnd
    member me.Transit(c) = 
        let mutable hasnext = false
        let mutable dest = me
        for trans in transitors do
            if trans.Input.Contains(c) then
                hasnext <- true
                dest <- trans.Dest
        hasnext, dest

    member me.Match(input:string) = 
        let len, _ = DFA.Match(0, s2l input, me, true)
        { Value = input.Substring(0,len); Length = len }

    member me.Match(input:char list) = 
        DFA.Match(0, input, me, true)            
    
    static member private Match(len, inputlist, dfa:DFA, long): int * char list =
        match inputlist with
        | input::tail  ->
            if long then
                let hasnext, nextdfa = dfa.Transit input
                if hasnext then
                    DFA.Match(len+1, tail, nextdfa, long)
                else
                    if dfa.IsEnd then
                        len, inputlist
                    else
                        0, []
            else
                if dfa.IsEnd then
                    len, inputlist
                else
                    let hasnext, nextdfa = dfa.Transit input
                    if hasnext then
                        DFA.Match(len+1, tail, nextdfa, long)
                    else
                        if dfa.IsEnd then
                            len, inputlist
                        else
                            0, []
        | [] -> 
            if dfa.IsEnd then
                len, []
            else
                0, []

    static member private ToDfaRecordList(set,(transes:Transitor<_,DFA> list), result : System.Collections.Generic.List<DfaRecordNode>) =
        match transes with
        | [] -> ()
        | trans::rest ->  
            trans.Dest.ToDfaRecordList(set, result)
            DFA.ToDfaRecordList(set, rest, result)

    member private me.ToDfaRecordList(set:System.Collections.Generic.HashSet<int>, result : System.Collections.Generic.List<DfaRecordNode>) = 
        if set.Contains(id) then
            ()
        else
            let transList = new System.Collections.Generic.List<TransitorRecord<char,int>>()
            for trans in transitors do
                transList.Add(TransitorRecord.cotr(trans.Input, trans.Dest.ID))
            let node = { ID = me.ID; IsEnd =me.IsEnd; Transitors = transList }
            set.Add(node.ID) |> ignore
            result.Add(node)
            DFA.ToDfaRecordList(set, me.Transitors, result)        
            
    member me.ToJson() =
        let data = me.ToSerializerableStruct()
        JavaScriptSerializer().Serialize(data)

    static member FromJson(json:string) =
        let data = JavaScriptSerializer().Deserialize<DfaRecord>(json)
        DFA.FromSerializerableStruct(data)

    member me.ToSerializerableStruct() =
        let set = new System.Collections.Generic.HashSet<int>()    
        let nodes = new System.Collections.Generic.List<DfaRecordNode>()
        me.ToDfaRecordList(set, nodes)
        { NodeList = nodes; StartID = me.ID }

    static member FromSerializerableStruct(dfa:DfaRecord) =
        let dict = new System.Collections.Generic.Dictionary<int, DfaTmpNode>();
        for node in dfa.NodeList do
            dict.Add(node.ID, DfaTmpNode(node))
        DFA(dfa.StartID, dict2map dict, new System.Collections.Generic.Dictionary<int, DFA>())  

and MatchResult = { Value:string; Length:int }

and [<CLIMutable>] DfaRecordNode = { ID:int; IsEnd:bool; Transitors:TransitorRecord<char,int> System.Collections.Generic.List }

and [<CLIMutable>] DfaRecord = { StartID:int; NodeList:DfaRecordNode System.Collections.Generic.List }

and [<CLIMutable>] TransitorRecord<'input,'dest when 'input:comparison> = { Input:System.Collections.Generic.List<'input>; Dest:'dest }
with 
    static member cotr(input:Opset<'input>, dest:'dest) : TransitorRecord<'input,'dest> =
        let trans = {Input = new System.Collections.Generic.List<'input>(); Dest = dest}
        for v in input.Set do
            trans.Input.Add(v)
        trans