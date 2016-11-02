module DfaModule

open NfaModule

let eclosemap = compute_e_close NfaStatic.NodeMap
let get_nfa_node idx = NfaStatic.NodeMap.[idx]
let get_eclose idx = eclosemap.[idx]

type OpSet<'t when 't:comparison>(set:'t Set) =
    let rawset = set

    static let rec remove l (set:Set<_>) =
        match l with
        | [] -> set
        | h::t -> set.Remove h |> remove t

    new (l:'t list) = OpSet(Set l)
        
    override me.Equals (o:obj) =
        match o with
        | :? OpSet<'t> as opset -> opset.RawSet = me.RawSet
        |_ -> false

    override me.GetHashCode () =
        rawset.GetHashCode()

    member me.RawSet with get() = rawset    
    member me.IsEmpty with get() = rawset.IsEmpty

    static member Intersection (s1:OpSet<'t>, s2:OpSet<'t>) =
        let inter = [for it1 in s1.RawSet do
                        if s2.RawSet.Contains it1 then
                            yield it1]
        let s1_ = remove inter s1.RawSet   
        let s2_ = remove inter s2.RawSet      
        OpSet(s1_), OpSet(Set inter), OpSet(s2_)
                
    static member Join (s1:OpSet<'t>, s2:OpSet<'t>) =
        let join = [for it in s1.RawSet do
                        yield it
                    for it in s2.RawSet do
                        yield it]    
        OpSet(Set join)

and Transition(input:OpSet<char>,dst:OpSet<int>) =
    let input = input
    let dst = dst

    new (set:char Set, nfaNode:NfaNode) = 
        Transition(OpSet set, OpSet (get_eclose nfaNode.ID))

    new (trans:Transitor') =
        match trans with
        | Transitor(chset, nextNode) -> Transition(chset, nextNode)
        | _ -> Transition(OpSet[], OpSet[])

    new () = Transition(OpSet[], OpSet[])

    member me.IsEmpty with get() = input.IsEmpty
    member me.Input with get() = input
    member me.Dest with get() = dst
    
    static member Join (r1:Transition, r2:Transition) =
        let input1s,input,input2s = OpSet.Intersection(r1.Input, r2.Input)
        Transition(input1s, r1.Dest), Transition(input, OpSet.Join(r1.Dest, r2.Dest)), Transition(input2s, r2.Dest)
    
    static member Join (rs:Transition list, r:Transition)  =
        match rs with
        | h::t ->
            let trans1,trans2,trans3 = Transition.Join(h, r)
            [   if not trans1.IsEmpty then 
                    yield trans1
                if not trans2.IsEmpty then 
                    yield trans2
                for trans in Transition.Join(t, trans3) do
                    if not trans.IsEmpty then
                        yield trans ]
        | [] -> [r]


type TfaNode(nfaNodeIdSet:Set<int>) =
    let nfaNodeIdSet = nfaNodeIdSet
    let mutable transitions = []
    
    do
        for idx in nfaNodeIdSet do
            let node = get_nfa_node idx
            let trans = Transition(node.Transitor)
            transitions <- Transition.Join(transitions, trans)


let cons_transitor (a:Map<char,int Set>) (b:Set<char>*NfaNode) =
    let chset, node = b
    let mutable map = a
    for ch in chset do
        match a.TryFind(ch) with
        | None -> map <- map.Add(ch,Set[node.ID])
        | Some(set) -> map <- map.Add(ch, (set.Add node.ID))
    map     

//生成NfaNode集合的状态转换表
let _ctor_next_set (idxset:int Set) (map:Map<int, NfaNode>) =
    let mutable nextset = Map<char,int Set>[]
    for idx in idxset do
        let node = map.[idx]
        match node.Transitor with
        | NoTransitor -> ()
        | Transitor(chset, nfanode) -> nextset <- cons_transitor nextset (chset, nfanode)
    nextset

let ctor_next_set (set:int Set) = _ctor_next_set set NfaStatic.NodeMap






