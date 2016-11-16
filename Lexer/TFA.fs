namespace Lexer

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
    static let mutable nodeMap = new System.Collections.Generic.Dictionary<Opset<int>, TFA>()
    
    static member AddTfaNode (nfaNodeSet:Opset<int>, node:TFA) = 
        let idx = tfaNodeIdx
        tfaNodeIdx <- tfaNodeIdx + 1
        nodeMap.Add(nfaNodeSet, node)
        idx

    static member IsTfaNodeExist (nfaNodeSet:Opset<int>) = 
        nodeMap.ContainsKey(nfaNodeSet)

    static member GetTfaNode (nfaNodeSet:Opset<int>) = 
        nodeMap.[nfaNodeSet]
    


