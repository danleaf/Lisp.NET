namespace Lexer
open Common

type DFA (tfa:TFA) as this =
    static let mutable map = new System.Collections.Generic.Dictionary<int, DFA>()
    do map.Add(tfa.ID, this)

    let id = tfa.ID
    let isEnd = tfa.IsEnd
    let transitors = 
        [for trans in tfa.Transitors do
            yield 
                match trans with
                | Transitor(set, dest) -> 
                    Transitor(set, if map.ContainsKey(dest.ID) then map.[dest.ID] else DFA(dest))
                | _ -> NoTransitor ]


    new (nfa:NFA) = DFA(TFA(nfa))
    
    member me.Transitors with get() = transitors
    member me.ID with get() = id
    member me.IsEnd with get() = isEnd
    member me.Transit c = 
        let mutable hasnext = false
        let mutable dest = me
        for trans in transitors do
            match trans with
                | Transitor(set, dst) -> 
                    if set.Contains(c) then
                        hasnext <- true
                        dest <- dst
                | _ -> ()
        hasnext, dest

    static member FindEnds (dfas:DFA list) =
        [for dfa in dfas do
            if dfa.IsEnd then yield dfa]


module DfaModule =
    let rec matchone len inputlist (dfa:DFA) long =
        match inputlist with
        | input::tail  ->
            if long then
                let hasnext, nextdfa = dfa.Transit input
                if hasnext then
                    matchone (len+1) tail nextdfa long
                else
                    if dfa.IsEnd then
                        len
                    else
                        0
            else
                if dfa.IsEnd then
                    len
                else
                    let hasnext, nextdfa = dfa.Transit input
                    if hasnext then
                        matchone (len+1) tail nextdfa long
                    else
                        if dfa.IsEnd then
                            len
                        else
                            0
        | [] -> 
            if dfa.IsEnd then
                len
            else
                0


    let matchlong (input:string) (dfa:DFA) = 
        let len = matchone 0 (s2l input) dfa true
        input.Substring(0,len),len

    let matchshort (input:string) (dfa:DFA) = 
        let len = matchone 0 (s2l input) dfa false
        input.Substring(0,len),len