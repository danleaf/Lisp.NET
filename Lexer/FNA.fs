module FNA

type FNA(statname:string, ends:bool) = 
    let mutable statname = statname
    let mutable ends = ends

    new () = FNA("", false)
    new (statname:string) = FNA(statname, true)

    member me.StatusName with get() = statname

    member val Transitors:list<Set<char> * FNA * bool> = [] with get,set

    member me.EndStatus with get() = ends and set(v) = ends <- v

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