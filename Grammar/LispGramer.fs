namespace Grammar

(* Lexer of Lisp.Net

SYMBLE  :   /[^\n\t \r\)\(\]\[\}\{.,;'""`]+/
NUMBER  :   /[0-9]+(\.[0-9]+)?/
STRING  :   /""([^""\\]|\\.)*""/
KEYWORD :   /:[\w\-]+/
OPEN    :   /[\(\[\{]/
CLOSE   :   /[\)\]\}]/
LET     :   /let/
DEF     :   /def/
FN      :   /fn/


Gramer of Lisp.Net

atom:              SYMBLE | NUMBER | STRING | KEYWORD
member:            atom | atom member
list:              OPEN member CLOSE | OPEN CLOSE
symble_bind_stmt:  OPEN LET name value CLOSE
var_def_stmt:      OPEN DEF name value CLOSE
function:          OPEN FN params body CLOSE
name:              list
value:             list
params:            list
body:              list

*)

type TokenType = 
    | SYMBLE        = 0
    | NUMBER        = 1
    | STRING        = 2  
    | KEYWORD       = 3 
    | OPEN          = 4  
    | CLOSE         = 5  
    | LET           = 6 
    | DEF           = 7
    | FN            = 8
    | BLANK         = 9
    | COMMENT       = 10
    | ERROR         = 11
    | Quot          = 12
    | NegQuot       = 13
    | Comma         = 14
    | Sharp         = 15
    | Point         = 16
    | At         = 17

type Token = { Type:TokenType; Value:string }

type LispGrammar(tokens:Lexer.Toekn list) =
    let tokenMap = Map[ "symble",TokenType.SYMBLE;
                        "blank",TokenType.BLANK;
                        "comment",TokenType.COMMENT;
                        "number",TokenType.NUMBER;
                        "string",TokenType.STRING;
                        "keyword",TokenType.KEYWORD;
                        "open",TokenType.OPEN;
                        "close",TokenType.CLOSE;
                        "error",TokenType.ERROR;
                        "`",TokenType.NegQuot;
                        "'",TokenType.Quot;
                        ",",TokenType.Comma;
                        ".",TokenType.Point;
                        "#",TokenType.Sharp;
                        "@",TokenType.At]
    let tokens = [for t in tokens -> { Type = tokenMap.[t.Name]; Value = t.Value}]

    member me.GetTokens() = tokens
    member me.Match() =
        match tokens with
        | [] -> ()
        | token -> ()
                    

