namespace Lexer

open System
open Common

type Transitor<'input,'dest> =
    | Transitor of 'input * 'dest
    | NoTransitor

//NFA节点，创建节点时会将节点信息加入NfaStatic
and NfaNode() as this = 
    static let mutable nfaNodeIdx = 0
    static let mutable nodeMap = new System.Collections.Generic.Dictionary<int, NfaNode>()

    let mutable statname = ""
    let mutable ends = false
    let mutable directTo:NfaNode Set = Set[]
    let id = nfaNodeIdx 
    
    do 
        nfaNodeIdx <- nfaNodeIdx + 1
        nodeMap.Add(id, this)        

    //获取某NFA节点的ε闭包
    static member private get_e_close (node:NfaNode) (set:int Set) =
        if set.Contains node.ID then
            set
        else
            let mutable newset = set.Add node.ID
            for n in node.DirectTo do
                newset <- NfaNode.get_e_close n newset
            newset

    static member GetNode(id) = nodeMap.[id]
    static member GetEClose(id) = NfaNode.get_e_close nodeMap.[id] (Set[])
    static member GetEClose(node:NfaNode) = NfaNode.get_e_close node (Set[])

    member me.GetEClose() = NfaNode.GetEClose(me)
    
    interface System.IComparable with
        member me.CompareTo (node:obj) = 
            match node with
            | :? NfaNode as nfanode -> id - nfanode.ID
            |_ -> 0

    override me.Equals (node:obj) =
        match node with
        | :? NfaNode as nfanode -> id = nfanode.ID
        |_ -> false

    override me.GetHashCode () =
        id.GetHashCode()

    member me.StatusName with get() = statname
    member me.ID with get() = id

    member val Transitor:Transitor<Set<char>, NfaNode> = NoTransitor with get,set
    member me.DirectTo with get() = directTo

    member me.EndStatus with get() = ends

    member me.SetToEndStatus (name:string) = 
        ends <- true
        statname <- name

    member me.SetTransitor (trans:Set<char> * NfaNode) =
        me.Transitor <- Transitor(trans)

    member me.AddDirectTo (n:NfaNode) =
        directTo <- directTo.Add n

//NFA为元组 Start, End
and NFA = 
    | Nfa of NfaNode * NfaNode
    | EmptyNfa

module NfaModule =

    //生成一个NFA，包含两个节点，一个起始节点，一个终止节点，set为接受的字符集
    let cotr_nfa set = 
        let start = new NfaNode()
        let end_ = new NfaNode()
        start.SetTransitor(set,end_) |> ignore
        Nfa(start, end_)

    //连接两个NFA
    let concat_nfa (a:NFA) (b:NFA) =
        match a, b with
        | EmptyNfa, EmptyNfa -> EmptyNfa
        | EmptyNfa, (Nfa(_,_) as x) -> x
        | (Nfa(_,_) as x), EmptyNfa -> x
        | Nfa(astart,aend), Nfa(bstart,bend) -> 
            aend.AddDirectTo bstart
            Nfa(astart,bend)

    //并接两个NFA
    let parallel_nfa (a:NFA) (b:NFA) =
        match a, b with
        | EmptyNfa, EmptyNfa -> EmptyNfa
        | EmptyNfa, (Nfa(_,_) as x) -> x
        | (Nfa(_,_) as x), EmptyNfa -> x
        | Nfa(astart,aend), Nfa(bstart,bend) -> 
            let s = new NfaNode()
            let e = new NfaNode()
            s.AddDirectTo astart
            s.AddDirectTo bstart
            aend.AddDirectTo e
            bend.AddDirectTo e
            Nfa(s, e)

    //连接两个NFA节点
    let concat_nfa_node (a:NfaNode) (b:NfaNode) =
        a.AddDirectTo b

    //往NFA的终止节点添加一条ε边到起始节点
    let cycle_nfa = function
        | EmptyNfa -> EmptyNfa
        | Nfa(s,e) as nfa -> 
            e.AddDirectTo s
            nfa
        
    //往NFA的起始节点添加一条ε边到终止节点
    let jump_nfa = function
        | EmptyNfa -> EmptyNfa
        | Nfa(s,e) as nfa -> 
            s.AddDirectTo e
            nfa
        
    //往NFA的起始节点添加一条ε边到终止节点，再往NFA的终止节点添加一条ε边到起始节点
    let cycle_and_jump_nfa = function
        | EmptyNfa -> EmptyNfa
        | Nfa(s,e) as nfa -> 
            s.AddDirectTo e
            e.AddDirectTo s
            nfa    

       