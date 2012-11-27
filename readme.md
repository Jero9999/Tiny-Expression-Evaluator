Tiny Expression Evaluator (or TinyEE) is a simple expression language running on .NET's Dynamic Language Runtime.

Its main strengths are:

1. Simple syntax: everything is an expression that returns a value. There is no looping, assignment or side-effect to worry about.
2. Easy to use and integrate with other system. The dll is tiny and there is no external dependency other than the core .NET framework

It's designed to be a power tool for domain experts to define rules and calculations that augment an existing system.

##Quick start
The easiest way to use TinyEE is to call its static method Tee.Evaluate(expression) :

```csharp
TEE.Evaluate("2^20") //returns 1048576
```

To use variables in your expression, pass in an object as the second parameter (dictionary and callback delegate are also supported).

```csharp
TEE.Evaluate("z = (x+y)^3 = x^3 + 3*x^2*y + 3*x*y^2 + y^3", new{x=3,y=2,z=125})
//returns True
```

For cases when an expression is evaluated multiple times (such as in a plotting app), you can improve performance using cached compilation. The struct returned by Compiled() holds a reference to a compiled delegate, which can be invoked repeatedly without incurring any parsing overhead:

```csharp
var expr = parsedExpr.Compile("x^3 + 3*x^2*y + 3*x*y^2 + y^3");//compiled
var result1 = expr.Evaluate(new{x=2,y=3});//result1 = 125
var result2 = expr.Evaluate(new{x=5,y=4});//result2 = 729
```

To get the list of variables in an expression, get a ParsedExpression:

```csharp
var parsedExpr = TEE.Parse("z = x^3 + 3*x^2*y + 3*x*y^2 + y^3");
var variables = parsedExpr.Variables;//variables = [z,x,y]
```

##Syntax Reference
The syntax borrows a lot from C#, JavaScript and Excel formula. When in doubt, there are 2 things to remember: 
* Everything is a value expression.
* Everything is dynamic.

**Literal data types**

<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
			<th>CLR type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Null</td>
			<td>null</td>
			<td>null</td>
		</tr>
		<tr>
			<td>String</td>
			<td>"A Review of \"A Tale of Two Cities\" and \"Moby Dick\""</td>
			<td>System.String</td>
		</tr>
		<tr>
			<td>Boolean</td>
			<td>True, False</td>
			<td>System.Boolean</td>
		</tr>
		<tr>
			<td>Integer</td>
			<td>-1234567890</td>
			<td>System.Int32</td>
		</tr>
		<tr>
			<td>Decimal</td>
			<td>+1234567890.555</td>
			<td>System.Double</td>
		</tr>
		<tr>
			<td>Integer range</td>
			<td>0..1048576</td>
			<td>IEnumerable&lt;int&gt; (lazily-evaluated)</td>
		</tr>
		<tr>
			<td>List</td>
			<td>["a string", 12, true, [0,1,2]]</td>
			<td>Object[]</td>
		</tr>
		<tr>
			<td>Hash</td>
			<td>{ "name":"Yoda", "age":900, "isMaster":true }</td>
			<td>Dictionary&lt;string,object&gt;</td>
		</tr>
	</tbody>
</table>


**Basic Arithmetics**

<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Addition</td>
			<td>x + y</td>
		</tr>
		<tr>
			<td>Subtraction</td>
			<td>x - y</td>
		</tr>
		<tr>
			<td>Multiplication</td>
			<td>x * y</td>
		</tr>
		<tr>
			<td>Modulo</td>
			<td>x % y</td>
		</tr>
		<tr>
			<td>Power</td>
			<td>x ^ y</td>
		</tr>
		<tr>
			<td>Negation</td>
			<td>-x</td>
		</tr>
	</tbody>
</table>

**Comparision**

<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Equal</td>
			<td>x = y</td>
		</tr>
		<tr>
			<td>Not Equal</td>
			<td>x <> y</td>
		</tr>
		<tr>
			<td>Greater than</td>
			<td>x > y</td>
		</tr>
		<tr>
			<td>Greater than or Equal</td>
			<td>x >= y</td>
		</tr>
		<tr>
			<td>Less than</td>
			<td>x &lt; y</td>
		</tr>
		<tr>
			<td>Less than or Equal</td>
			<td>x &lt;= y</td>
		</tr>
	</tbody>
</table>
Comparisions can also be chained, and the evaluation order is from left to right:
```csharp
(0 < x < 10) = (0 < x and x < 10)
```

**Logical**
<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>AND</td>
			<td>x and y</td>
		</tr>
		<tr>
			<td>OR</td>
			<td>x or y</td>
		</tr>
		<tr>
			<td>NOT</td>
			<td>not x</td>
		</tr>
	</tbody>
</table>

**Object**
<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
			<th>Note</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Variable</td>
			<td>x + $math.Max(y,z)</td>
			<td>Variable names are case-sensitive, x and X are not the same. Their names must start with a character or the dollar sign ($).</td>
		</tr>
		<tr>
			<td>Method call</td>
			<td>"anton".ToUpper()</td>
			<td></td>
		</tr>
		<tr>
			<td>Member Access</td>
			<td>person.Name.Length</td>
			<td></td>
		</tr>
		<tr>
			<td>Indexer Access</td>
			<td>table.Rows[0]["column-0"]</td>
			<td></td>
		</tr>
		<tr>
			<td>Global function call</td>
			<td>SUM(x,y)</td>
			<td>Note that global function names are allow to be case-insensitive, SUM() and sum() are the same.</td>
		</tr>
		<tr>
			<td>Grouping</td>
			<td>(x + y) * z</td>
			<td></td>
		</tr>
	</tbody>
</table>

**Branching**
<table>
	<thead>
		<tr>
			<th>Type<th>
			<th>Example<th>
			<th>Note<th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Conditional</td>
			<td>booleanVar ? "value-if-true" : "value-if-false"</td>
			<td></td>
		</tr>
		<tr>
			<td>Coalescing</td>
			<td>nullableVar ?: "fallback-value"</td>
			<td>This is similar to c#'s ?? operator or Javascript's use of ||. Unlike Javasript, however, this operator does not perform type conversion.</td>
		</tr>
	</tbody>
</table>

**Chaining and nesting**
Almost all expression can be chained or nested.

* Property and indexer:
```
table01A.Rows[3]["col3"]
```
* Function call:
```
100>10 ? Sum(Max(1,2),Max(0,1),Max(-5,3)) : Sum(Min(-2,-3),Min(1,2))
```
* Comparision:
```
x > y > z
```
* Conditional:
```
condition1
    ? "value 1"
    : condition2
        ? "value 2"
        : "value 3"
```

**Operator precedence and associativity**

The following table shows all operators in order of precedence from highest to lowest:

<table>
	<thead>
		<tr>
			<th>Operator type</th>
			<th>Operator</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Primary</td>
			<td>() f(x) x[y] x.y </td>
		</tr>
		<tr>
			<td>Arithmetics</td>
			<td>- ^ * / % + -</td>
		</tr>
		<tr>
			<td>Comparison</td>
			<td>= &gt; &lt; &gt;= &lt;= &lt;&gt;</td>
		</tr>
		<tr>
			<td>Boolean</td>
			<td>not and or</td>
		</tr>
		<tr>
			<td>Branching</td>
			<td>x?:y  x?y:z</td>
		</tr>
	</tbody>
</table>

All expressions are evaluated from left to right. The expression 2^2^2^2, for example, is evaluated as ((2^2)^2)^2 == 256 (same as in C# and Excel (but differs from Mathematical convention))

##Main APIs
<table>
	<thead>
		<tr>
			<th>Class</th>
			<th>Function</th>
			<th>Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>TinyEE.TEE</td>
			<td>Evaluate(string):object</td>
			<td>Parse, compile and evaluate an expression with no variables</td>
		</tr>
		<tr>
			<td></td>
			<td>Evaluate(string,object):object</td>
			<td>Parse, compile and evaluate an expression with a context object. Variables within the expression are resolved using the context's properties and fields. Both anonymous and named objects are supported.</td>
		</tr>
		<tr>
			<td></td>
			<td>Evaluate(string,Func&lt;string,object&gt;):object</td>
			<td>Parse, compile and evaluate an expression with a context functor. The functor will be called when a variable needs to be resolved.</td>
		</tr>
		<tr>
			<td></td>
			<td>Parse(string):ParsedExpression</td>
			<td>Parse an expression (no compilation or evaluation yet). The metadata from the returned object can be useful in building a dependency graph or in transforming to another language.</td>
		</tr>
		<tr>
			<td></td>
			<td>Compile(string):CompiledExpression</td>
			<td>Parse and compile an expression (no evaluation yet). The returned struct simply wraps around a delegate to the compiled code and can be called over and over again.</td>
		</tr>
	</tbody>
</table>

##Benchmark
//TODO

##Credits
TinyEE's parser and scanner are generated by Herre Kuijpers's excellent [Tiny Parser Generator](http://www.codeproject.com/Articles/28294/a-Tiny-Parser-Generator-v1-2). All thanks go to him.

##License
MS-PL