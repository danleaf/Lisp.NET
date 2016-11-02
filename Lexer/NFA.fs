module NfaModule
open System
open Common

type NfaStatic() =
    static let mutable nfaNodeIdx = 0
    static let mutable nodeMap:Map<int, NfaNode> = Map[]

    static member AddNfaNode (node:NfaNode) = 
        let idx = nfaNodeIdx
        nfaNodeIdx <- nfaNodeIdx + 1
        nodeMap <- nodeMap.Add(idx, node)
        idx

    static member NodeMap with get() = nodeMap

and NfaNode() as this = 
    let mutable statname = ""
    let mutable ends = false
    let mutable directTo:NfaNode Set = Set[]
    let id = NfaStatic.AddNfaNode this
    
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

    member val Transitor:Transitor' = NoTransitor with get,set
    member me.DirectTo with get() = directTo

    member me.EndStatus with get() = ends

    member me.SetToEndStatus (name:string) = 
        ends <- true
        statname <- name

    member me.SetTransitor (trans:Set<char> * NfaNode) =
        me.Transitor <- Transitor(trans)

    member me.AddDirectTo (n:NfaNode) =
        directTo <- directTo.Add n
    
and Transitor' =
    | Transitor of Set<char> * NfaNode
    | NoTransitor

and Nfa' = 
    | Nfa of NfaNode * NfaNode
    | EmptyNfa

let nameof (nfa:NfaNode) = nfa.StatusName

let cotr_nfa set = 
    let start = new NfaNode()
    let end_ = new NfaNode()
    start.SetTransitor(set,end_) |> ignore
    Nfa(start, end_)

let concat_nfa (a:Nfa') (b:Nfa') =
    match a, b with
    | EmptyNfa, EmptyNfa -> EmptyNfa
    | EmptyNfa, (Nfa(_,_) as x) -> x
    | (Nfa(_,_) as x), EmptyNfa -> x
    | Nfa(astart,aend), Nfa(bstart,bend) -> 
        aend.AddDirectTo bstart
        Nfa(astart,bend)

let parallel_nfa (a:Nfa') (b:Nfa') =
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
    
let concat_nfa_node (a:NfaNode) (b:NfaNode) =
    a.AddDirectTo b

let cycle_nfa = function
    | EmptyNfa -> EmptyNfa
    | Nfa(s,e) as nfa -> 
        e.AddDirectTo s
        nfa

let jump_nfa = function
    | EmptyNfa -> EmptyNfa
    | Nfa(s,e) as nfa -> 
        s.AddDirectTo e
        nfa

let cycle_and_jump_nfa = function
    | EmptyNfa -> EmptyNfa
    | Nfa(s,e) as nfa -> 
        s.AddDirectTo e
        e.AddDirectTo s
        nfa    

let rec _search_e_close (node:NfaNode) (set:int Set) =
    if set.Contains node.ID then
        set
    else
        let mutable newset = set.Add node.ID
        for n in node.DirectTo do
            newset <- _search_e_close n newset
        newset

let search_e_close node = _search_e_close node (Set[])

let compute_e_close (map:Map<int, NfaNode>) =
    let mutable closemap = Map<int, Set<int>>[]
    for node in map do
        closemap <- closemap.Add(node.Key, search_e_close node.Value)
    closemap
