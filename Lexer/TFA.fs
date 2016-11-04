﻿namespace Lexer

open System.Collections.Generic

type Transition(input:Opset<char>,dst:Opset<int>) =
    let input = input
    let dst = dst

    //从指定NFA节点读取原生状态转移关系，生成Transition类的实例
    //生成的状态转移关系的目标集合为原始目标的ε闭包
    new (nfaNodeId:int) = 
        let nfaNode = NfaNode.GetNode(nfaNodeId)
        match nfaNode.Transitor with
        | Some(trans) -> Transition(trans.Input, trans.Dest.GetEClose())
        | _ -> Transition(Opset[], Opset[])
        

    new () = Transition(Opset[], Opset[])

    member me.IsEmpty with get() = input.IsEmpty
    member me.Input with get() = input
    member me.Dest with get() = dst

    //将状态转移列表中相同目的的合并到一起
    static member Single (rs:Transition list) =
        let dict = new Dictionary<Opset<int>,Opset<char>>()
        for r in rs do
            if dict.ContainsKey r.Dest then
                let input = dict.[r.Dest]
                dict.[r.Dest] <- input.Join r.Input
            else
                dict.Add(r.Dest, r.Input)
        [for v in dict do
            if not (v.Value.IsEmpty || v.Key.IsEmpty) then
                yield Transition(v.Value, v.Key) ]
    
    //将两个转移关系合并为一个转移关系列表
    //合并后的各转移关系的接收字符集互斥
    static member Join (r1:Transition, r2:Transition) =
        let input1s,input,input2s = Opset.Intersection(r1.Input, r2.Input)
        Transition(input1s, r1.Dest), 
        Transition(input, Opset.Join(r1.Dest, r2.Dest)), 
        Transition(input2s, r2.Dest)
    
    //将一个新的转移关系合并到已有转移关系列表里面
    //需保证已有转移关系列表里的各转移关系的接收字符集互斥
    //合并后的各转移关系的接收字符集互斥
    static member private join (rs:Transition list, trans:Transition)  =
        match rs with
        | head::rest ->
            let trans1,trans2,trans3 = Transition.Join(head, trans)
            [   if not trans1.IsEmpty then yield trans1
                if not trans2.IsEmpty then yield trans2 ] @ Transition.join(rest, trans3)
        | [] -> [if not trans.IsEmpty then yield trans]

    static member Join (rs:Transition list, r:Transition)  =
        Transition.join(rs,r) |> Transition.Single
        
    member me.JoinTo (rs:Transition list)  =
        Transition.Join(rs, me)

//临时状态机，用于将NFA转化为DFA
//每个节点包含若干NFA节点ID的集合，以及各NFA节点的状态转移关系的合集
type TFA(nfaNodeIdSet:Opset<int>, nfaEndNodeId:int) as this =   

    //该临时节点包含的NFA节点ID集合
    let nfaNodeIdSet = 
        if TfaStatic.IsTfaNodeExist(nfaNodeIdSet) then failwith "there has been same TFA node"
        nfaNodeIdSet

    let id = TfaStatic.AddTfaNode(nfaNodeIdSet, this) 
    let isEnd = nfaNodeIdSet.Contains(nfaEndNodeId)
    
    //将NFA节点列表中所有节点的转移关系合并
    let rec comput_transitions (nodeIds:int list) (rs:Transition list) = 
        match nodeIds with
        | [] -> rs
        | idx::tail -> Transition(idx).JoinTo(rs) |> comput_transitions tail

    //创建状态转移节点
    let rec create_transitors (tarnslist:Transition list) result =
        match tarnslist with
        |[] -> result
        | trans::rest -> 
            result @ 
            [Transitor(trans.Input, 
                if TfaStatic.IsTfaNodeExist(trans.Dest) then 
                    TfaStatic.GetTfaNode(trans.Dest)
                else
                    TFA(trans.Dest, nfaEndNodeId))
            ] |> create_transitors rest      
        

    //该临时节点的状态转移列表
    let transitors = create_transitors (comput_transitions (Opset.toList nfaNodeIdSet) []) []      

    new (nfa:NFA) = 
        match nfa with
        | Nfa(st,ed) -> 
            TFA(st.GetEClose(), ed.ID)
        | _ -> TFA(Opset<int>([]), -1)
        
    member me.Transitors with get() = transitors
    member me.NfaNodeSet with get() = nfaNodeIdSet
    member me.IsEnd with get() = isEnd
    member me.ID with get() = id


and TfaStatic() =
    static let mutable tfaNodeIdx = 0
    static let mutable nodeMap = new Dictionary<Opset<int>, TFA>()
    
    static member AddTfaNode (nfaNodeSet:Opset<int>, node:TFA) = 
        let idx = tfaNodeIdx
        tfaNodeIdx <- tfaNodeIdx + 1
        nodeMap.Add(nfaNodeSet, node)
        idx

    static member IsTfaNodeExist (nfaNodeSet:Opset<int>) = 
        nodeMap.ContainsKey(nfaNodeSet)

    static member GetTfaNode (nfaNodeSet:Opset<int>) = 
        nodeMap.[nfaNodeSet]
    


