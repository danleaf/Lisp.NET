# Lisp.NET

[def [func x y][add x y]]

[defm |[def @left -> @right]|
	[def @left @right]
]


[def func x y -> x + y]

def func x -> x.b + x.a

[switch x [1 [x+2]] [2 [x*2]] [3 [x^2]] [_ x]]

[defm switch x args ->
	[if
		
	]
]

[if [x = 1] 
	[x+2] 
	[if [x = 2]
		[x*2]
		[if [x = 3] 
			[x^2]
			[]
		]
	]
]

[x + [y + e] + z]

[x + [add y e] + z]
[add [x [add y e]] + z]
[]

[defm + x y ->
	[add x y]
	
	



[x + 2] --> [+ x 2]

[x a;t c;e e;e e] == [[x a][t c][r e][e ]]

class
[	A
	[
		a
		[b [] []]
		c
		d
	]
]