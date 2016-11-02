module FNA
open System

let mutable index = 0

type FNA() = 
    let mutable statname = ""
    let mutable ends = false
    let id = index
    do index <- index + 1

    member me.StatusName with get() = statname
    member me.ID with get() = id

    member val Transitors:list<Set<char> * FNA * bool> = [] with get,set
    member val DirectTo:DirectNext = NoneNext with get,set

    member me.EndStatus with get() = ends

    member me.SetToEndStatus (name:string) = 
        ends <- true
        statname <- name

    member me.AddTransitor (trans:Set<char> * FNA * bool) =
        me.Transitors <- trans::me.Transitors
        let _,fna,_ = trans
        fna
    
    member me.Transit c =
        [for set,fna,neg in me.Transitors do
            match neg with
            | false -> 
                if set.Contains c then 
                    yield fna
            | _ -> 
                if not (set.Contains c) then 
                    yield fna]
and DirectNext =
    | Next of FNA
    | NoneNext

let nameof (fna:FNA) = fna.StatusName

let rec setend (fnas:FNA list) name =
    match fnas with 
    | head::tail -> 
        head.SetToEndStatus name
        setend tail name
    | [] -> ()

let rec addtrans (fnas:FNA list) set fna neg =
    match fnas with 
    | head::tail -> 
        head.AddTransitor (set, fna, neg) |> ignore
        addtrans tail set fna neg
    | [] -> fna

let cotr_fna c = []
    