namespace Lexer

open System.Collections.Generic

//可以进行运算的集合
type Opset<'t when 't:comparison>(set:'t Set) =
    let rawset = set

    static let rec remove l (set:Set<_>) =
        match l with
        | [] -> set
        | h::t -> set.Remove h |> remove t

    new (l:'t list) = Opset(Set l)
        
    override me.Equals (o:obj) =
        match o with
        | :? Opset<'t> as opset -> opset.Set = me.Set
        |_ -> false

    override me.GetHashCode () =
        rawset.GetHashCode()

    override me.ToString () =
        rawset.ToString()

    member me.Set with get() = rawset    
    member me.IsEmpty with get() = rawset.IsEmpty
    member me.Contains (a) = rawset.Contains(a)
    member me.Add(a) = Opset(rawset.Add(a))
    static member toList(set:Opset<'t>) = Set.toList set.Set
    
    //求两个集合的交集，设运算的集合为A和B，则返回元组 A-(AB), AB, B-(AB)
    static member Intersection (s1:Opset<'t>, s2:Opset<'t>) =
        let inter = [for it1 in s1.Set do
                        if s2.Set.Contains it1 then
                            yield it1]
        let s1_ = remove inter s1.Set   
        let s2_ = remove inter s2.Set      
        Opset(s1_), Opset(Set inter), Opset(s2_)
                
    //求两个集合的并集
    static member Join (s1:Opset<'t>, s2:Opset<'t>) =
        let join = [for it in s1.Set do
                        yield it
                    for it in s2.Set do
                        yield it]    
        Opset join

    member me.Join (s:Opset<'t>) =
        Opset.Join(me, s)

    member me.Intersection (s:Opset<'t>) =
        Opset.Intersection(me, s)
        
    member me.ToList () : List<'t> =
        let l = new List<'t>()
        for v in rawset do
            l.Add(v)
        l
        

type Transitor<'input,'dest when 'input:comparison> (input:Opset<'input>, dest:'dest) = 
    let input = input
    let dest = dest

    static member cotr (input:'input List, dest:'dest) =
        let mutable set = Opset<'input>[]
        for v in input do
            set <- set.Add(v)
        Transitor(set, dest)
    
    member me.Input with get() = input
    member me.Dest with get() = dest


module Common =
    let cdr = List.tail
    let car = List.head

    let rec cdrn n l = 
        if n = 0 then 
            l
        else 
            match l with
            | [] -> []
            | _::tail -> cdrn (n-1) tail
        

    let s2l (str:string) = 
        str.ToCharArray() |> List.ofArray 

    let l2s (cl:char list) = 
        new string(cl |> List.toArray)


    let (|Letter|_|) = function
        | x when x <= 'z' && x >= 'a' -> Some()
        | x when x <= 'Z' && x >= 'A' -> Some()
        | _ -> None

    let (|Digit|_|) = function
        | x when x <= '9' && x >= '0' -> Some()
        | _ -> None
        
    let FList2CList l = 
        let ll = new System.Collections.Generic.List<_>()
        for v in l do
            ll.Add(v)
        ll
        
    let CList2FList (l:System.Collections.Generic.List<'a>) : 'a list = 
        [for v in l do yield v]
            

