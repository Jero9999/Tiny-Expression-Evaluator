Tiny Expression Evaluator (or TinyEE) strives to be the simplest expression language for .NET, ever.

Its main strengths are:

1. Easy to use and integrate
2. Supports a wide array of syntaxes: operator chaining, nested indexer & property access, method call...
3. Minimal code base, running on .NET 4's reliable Dynamic Language Runtime (DLR)
4. Supports compiled expression
5. Supports translation to JavaScript

It is ideal for cases when you want to give users a lot of power (for example: a rule engine, or a validation engine), but not so much they can compromise the security of your system. Its ability to translate expressions into JavaScript is especially useful when you have to unify server-side and client-side validations.

##Quick start
The easiest way to use TinyEE is to call its static method Tee.Evaluate(expression) :

```csharp
TEE.Evaluate("2^20") //returns 1048576
```

To use variables, you can put in an object as the second parameter (dictionary and callback delegate are also supported).

```csharp
TEE.Evaluate("z = (x+y)^3 = x^3 + 3*x^2*y + 3*x*y^2 + y^3", new{x=3,y=2,z=125})
//returns True
```

For cases when an expression is evaluated over and over again (such as in a plotting program), you can improve performance using cached compilation. The struct returned by Compiled() holds a reference to a compiled delegate, which can be invoked repeatedly without incurring any parsing:

```csharp
var expr = parsedExpr.Compile("x^3 + 3*x^2*y + 3*x*y^2 + y^3");//compiled
var result1 = expr.Evaluate(new{x=2,y=3});//result1 = 125
var result2 = expr.Evaluate(new{x=5,y=4});//result2 = 729
```

To get the list of variables from an expression, or to translate it to JavaScript, get a ParsedExpression:

```csharp
var parsedExpr = TEE.Parse("z = x^3 + 3*x^2*y + 3*x*y^2 + y^3");
var variables = parsedExpr.Variables;//variables = [z,x,y]
var jsExpr = parsedExpr.TranslateToJs();//extension method
//jsExpr = "z===Math.pow(x,3)+3*Math.pow(x,2)*y+3*x*Math.pow(y,2)+Math.pow(y,3)"
```

##Syntax Reference

Here's some general notes about the syntax:
* Most of it follows that of C#, JavasSript and Excel formula.
* Basically, everything is an expression that can be evaluated to a value.
* Everything is dynamic so there's little need for type casting

**Literal data types**

<table>
	<thead>
		<tr>
			<th>Type</th>
			<th>Example</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>String</td>
			<td>"A Review of \"A Tale of Two Cities\" and \"Moby Dick\""</td>
		</tr>
		<tr>
			<td>Boolean</td>
			<td>True, False</td>
		</tr>
		<tr>
			<td>Integer</td>
			<td>-1234567890</td>
		</tr>
		<tr>
			<td>Decimal</td>
			<td>+1234567890.555</td>
		</tr>
		<tr>
			<td>Null</td>
			<td>null</td>
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
			<td>x, y, z</td>
			<td>Variable names are case-sensitive, x and X are not the same.</td>
		</tr>
		<tr>
			<td>Variable with callable methods</td>
			<td>$sys, $usr</td>
			<td></td>
		</tr>
		<tr>
			<td>Method call</td>
			<td>$x:GetName(y)</td>
			<td></td>
		</tr>
		<tr>
			<td>Member Access</td>
			<td>person1.Name.Length</td>
			<td></td>
		</tr>
		<tr>
			<td>Indexer Access</td>
			<td>table01.Rows[0]["column-0"]</td>
			<td></td>
		</tr>
		<tr>
			<td>Global function call</td>
			<td>IF(x>0, SUM(x,y), 0)</td>
			<td>Note that global function names are case-insensitive, SUM() and sum() are the same.</td>
		</tr>
		<tr>
			<td>Grouping</td>
			<td>(x + y) * z</td>
                        <td></td>
		</tr>
	</tbody>
</table>
**List of global functions**
Most of the global functions are simple relays for static methods on System.Math, System.Linq.Enumerable and System.String

//TODO


**Chaining and nesting**
Almost all expression can be chained or nested.

* Property and indexer:
```
table01A.Rows[3]["col3"]
```
* Function call:
```
If(100>10, Sum(Max(1,2),Max(0,1),Max(-5,3)), Sum(Min(-2,-3),Min(1,2)))
```
* Comparision:
```
x > y > z
```

**Evaluation order**
Expressions are evaluated from left to right. The expression 2^2^2^2, for example, is evaluated as ((2^2)^2)^2 == 256 (same as in C# and Excel, but differs from Mathematical convention)

**Limitation**
The expression between the squared bracket of an indexer can only be a string or integer literal. So this is valid: ```dataTable[0]["xyz"]``` while this is not: ```dataTable[x+1][ToUpper(x.Name)]```.
Method Calls are not chainable and can only be invoked on special variables beginning with $. So this is good: ```$usr:ToString()``` while this is not: ```x:GetType().Assembly:CreateInstance("dangerous-type")```.

##API Reference
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