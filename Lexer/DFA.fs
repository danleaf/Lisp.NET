module DfaModule

open NfaModule

let closemap = compute_e_close NfaStatic.NodeMap

type TfaNode(nfaIdSet:Set<int>) =
    let nfaIdSet = nfaIdSet
    let nextNodes:Map<char,TfaNode> = Map[]

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






