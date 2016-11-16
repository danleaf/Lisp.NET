namespace Grammar

type Open = class end

and Close = class end

and List = 
    | A of Open * Atom * Close

and Atom = class end

and Members =
    | B of Atom*Members
    | Empty

