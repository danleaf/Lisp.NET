namespace Lexer

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
        let dict = new System.Collections.Generic.Dictionary<Opset<int>,Opset<char>>()
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

    static member Join (rs:Transition list, r:Transition) =
        Transition.join(rs,r) |> Transition.Single
        
    member me.JoinTo (rs:Transition list)  =
        Transition.Join(rs, me)