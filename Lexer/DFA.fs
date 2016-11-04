namespace Lexer

open System.Collections.Generic
open System.Web.Script.Serialization
open Common

type DFA(id:int, isEnd:bool, transitors:Transitor<char, DFA> list) =

    let id = id
    let isEnd = isEnd
    let mutable transitors = transitors   

    static member FromNfa(nfa:NFA) = DFA.FromTfa(TFA(nfa))
    static member FromTfa(tfa:TFA) = DFA.FromTfa(tfa, new Dictionary<int, DFA>())
        
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
        let len = DFA.match' 0 (s2l input) me true
        { Value = input.Substring(0,len); Length = len }

    member me.ToJson() =
        let data = me.ToSerializerableStruct()
        JavaScriptSerializer().Serialize(data)

    static member FromJson(json:string) =
        let data = JavaScriptSerializer().Deserialize<Dfa>(json)
        DFA.FromSerializerableStruct(data)

    member me.ToSerializerableStruct() =
        let set = new HashSet<int>()    
        let nodes = new List<DfaNode>()
        me.ToDfaList(set, nodes)
        { NodeList = nodes; StartID = me.ID }

    static member FromSerializerableStruct(dfa:Dfa) =
        let map = new Dictionary<int, DfaNode>();
        for node in dfa.NodeList do
            map.Add(node.ID, node)
        DFA.FromDfaNode(dfa.StartID, map, new Dictionary<int, DFA>())
    
    static member private FromDfaNode(nodeID,nodeMap:Dictionary<int, DfaNode>,dfaMap:Dictionary<int, DFA>) = 
            
        let node = nodeMap.[nodeID]
        if dfaMap.ContainsKey(node.ID) then
            dfaMap.[node.ID] 
        else    
            let dfa = DFA(node.ID, node.IsEnd, [])
            dfaMap.Add(dfa.ID, dfa)
            dfa.SetTransitors(
                [for v in node.Transitors do
                    yield Transitor.cotr(v.Input, DFA.FromDfaNode(v.Dest, nodeMap, dfaMap))])
            dfa
        
    static member private FromTfa(tfa:TFA, map:Dictionary<int, DFA>) = 
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
    
    static member private match' len inputlist (dfa:DFA) long =
        match inputlist with
        | input::tail  ->
            if long then
                let hasnext, nextdfa = dfa.Transit input
                if hasnext then
                    DFA.match' (len+1) tail nextdfa long
                else
                    if dfa.IsEnd then
                        len
                    else
                        0
            else
                if dfa.IsEnd then
                    len
                else
                    let hasnext, nextdfa = dfa.Transit input
                    if hasnext then
                        DFA.match' (len+1) tail nextdfa long
                    else
                        if dfa.IsEnd then
                            len
                        else
                            0
        | [] -> 
            if dfa.IsEnd then
                len
            else
                0

    static member private ToDfaList(set,(transes:Transitor<_,DFA> list), result : List<DfaNode>) =
        match transes with
        | [] -> ()
        | trans::rest ->  
            trans.Dest.ToDfaList(set, result)
            DFA.ToDfaList(set, rest, result)

    member private me.ToDfaList(set:HashSet<int>, result : List<DfaNode>) = 
        if set.Contains(id) then
            ()
        else
            let transList = new List<NodeTransitor<char,int>>()
            for trans in transitors do
                transList.Add(NodeTransitor.cotr(trans.Input, trans.Dest.ID))
            let node = { ID = me.ID; IsEnd =me.IsEnd; Transitors = transList }
            set.Add(node.ID) |> ignore
            result.Add(node)
            DFA.ToDfaList(set, me.Transitors, result)            
            
    member private me.SetTransitors(ts:Transitor<char, DFA> list) = transitors <- ts

and MatchResult = { Value:string; Length:int }

and [<CLIMutable>] DfaNode = { ID:int; IsEnd:bool; Transitors:NodeTransitor<char,int> List }

and [<CLIMutable>] Dfa = { StartID:int; NodeList:DfaNode List }

and [<CLIMutable>] NodeTransitor<'input,'dest when 'input:comparison> = { Input:List<'input>; Dest:'dest }
with 
    static member cotr(input:Opset<'input>, dest:'dest) : NodeTransitor<'input,'dest> =
        let trans = {Input = new List<'input>(); Dest = dest}
        for v in input.Set do
            trans.Input.Add(v)
        trans