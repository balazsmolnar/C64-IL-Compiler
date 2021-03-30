## Running C# code in C64

This is an experimental project to run C# code in C64 environment.

### A simple example

```cs
static void Main()
{
    for (int i = 0; i < 10; i++)
        Console.WriteLine("hello world!");
}
```

Output:
![Hello world](doc\HelloWorld.png?raw=true "Hello World")

How does it work?

1. .NET compiler generates IL code

```IL
.method private hidebysig static
	void Main () cil managed
{
	// Method begins at RVA 0x2054
	// Code size 30 (0x1e)
	.maxstack 2
	.locals init (
		[0] int32,
		[1] bool
	)

	IL_0000: nop
	// for (int i = 0; i < 10; i++)
	IL_0001: ldc.i4.0
	IL_0002: stloc.0
	// (no C# code)
	IL_0003: br.s IL_0014
	// loop start (head: IL_0014)
		// Console.WriteLine("hello world!");
		IL_0005: ldstr "hello world!"
		IL_000a: call void [netstandard]System.Console::WriteLine(string)
		// (no C# code)
		IL_000f: nop
		// for (int i = 0; i < 10; i++)
		IL_0010: ldloc.0
		IL_0011: ldc.i4.1
		IL_0012: add
		IL_0013: stloc.0

		// for (int i = 0; i < 10; i++)
		IL_0014: ldloc.0
		IL_0015: ldc.i4.s 10
		IL_0017: clt
		IL_0019: stloc.1
		// (no C# code)
		IL_001a: ldloc.1
		IL_001b: brtrue.s IL_0005
	// end loop

	IL_001d: ret
} // end of method Program::Main

```

The tool generates assembly code in 64tass format, heavily leveraging on macros. This code is generated without optimization, it resembles the original IL code.

```assembly
;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main
    #init_locals_pull_parameters 2, []
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #locals_pull_value_8 1, 0 ; Stloc_0
    jmp Program_Main_20 ; Br_s
Program_Main_5:    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 1, 0 ; Stloc_0
Program_Main_20:    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 2, 0 ; Stloc_1
    #locals_push_value_8 2 ; Ldloc_1
    #branch_true Program_Main_5 ; Brtrue_s
    #method_exit 4, [] ; Ret
```

After this phase 64tass generated the final instructions.

A few simple optimization techniques are implemented to improve performance and reduce the size of the generated code.

### Memory layout

TODO

### Garbage collection

TODO

### Integration testing

TODO

### Demo game port

I started to port Hunchback to c#.
The assets are taken from this great repository: https://github.com/C64-Mark/Hunchback

![Hunchback](doc\HunchbackTitle.png?raw=true "Hunchback")
![Hunchback](doc\HunchbackRope.png?raw=true "Hunchback")
![Hunchback](doc\HunchbackKnights.png?raw=true "Hunchback")
