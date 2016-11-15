namespace Lexer

open System.Web.Script.Serialization
open Common

type DFA(id:int, isEnd:bool, transitors:Transitor<char, DFA> list) =

    let id = id
    let isEnd = isEnd
    let mutable transitors = transitors   

    static member FromNfa(nfa:NFA) = DFA.FromTfa(TFA(nfa))
    static member FromTfa(tfa:TFA) = DFA.FromTfa(tfa, new System.Collections.Generic.Dictionary<int, DFA>())
        
    member me.Transitors with get() = transitors
    member me.ID with get() = id
    member me.IsEnd with get() = isEnd
    member me.Transit c = 
        let mutable hasnext = false
        let mutable dest = me
        for trans in transitors do
            if trans.Input.Contains(c) then
                hasnext <- true
                dest <- trans.Dest
        hasnext, dest

    member me.Match(input:string) = 
        let len, _ = DFA.match' 0 (s2l input) me true
        { Value = input.Substring(0,len); Length = len }

    member me.Match(input:char list) = 
        DFA.match' 0 input me true

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
        let map = new System.Collections.Generic.Dictionary<int, DfaRecordNode>();
        for node in dfa.NodeList do
            map.Add(node.ID, node)
        DFA.FromDfaRecordNode(dfa.StartID, map, new System.Collections.Generic.Dictionary<int, DFA>())
    
    static member private FromDfaRecordNode(nodeID,nodeMap:System.Collections.Generic.Dictionary<int, DfaRecordNode>,dfaMap:System.Collections.Generic.Dictionary<int, DFA>) = 
            
        let node = nodeMap.[nodeID]
        if dfaMap.ContainsKey(node.ID) then
            dfaMap.[node.ID] 
        else    
            let dfa = DFA(node.ID, node.IsEnd, [])
            dfaMap.Add(dfa.ID, dfa)
            dfa.SetTransitors(
                [for v in node.Transitors do
                    yield Transitor.cotr(v.Input, DFA.FromDfaRecordNode(v.Dest, nodeMap, dfaMap))])
            dfa
        
    static member private FromTfa(tfa:TFA, map:System.Collections.Generic.Dictionary<int, DFA>) = 
        if map.ContainsKey(tfa.ID) then
            map.[tfa.ID] 
        else    
            let dfa = DFA(tfa.ID, tfa.IsEnd, [])
            map.Add(dfa.ID, dfa)
            dfa.SetTransitors(
                [for trans in tfa.Transitors do
                    let set,dest = trans.Input,trans.Dest
                    yield Transitor(set, DFA.FromTfa(dest, map))])
            dfa
    
    static member private match' len inputlist (dfa:DFA) long : int * char list =
        match inputlist with
        | input::tail  ->
            if long then
                let hasnext, nextdfa = dfa.Transit input
                if hasnext then
                    DFA.match' (len+1) tail nextdfa long
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
                        DFA.match' (len+1) tail nextdfa long
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
            
    member private me.SetTransitors(ts:Transitor<char, DFA> list) = transitors <- ts

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