InternalPatternBind.cs contains a parser generator that allows you to convert Scheme data into any set of classes. It can be compared to XML Serialization in the .NET framework, but it uses Scheme data instead of XML. Also InternalPatternBind is one-way (you cannot convert the classes back to Scheme data).

This parser is used by Pascalesque.

It works by means of attributes. For example:

{{
    [Pattern("(byte $b)")](Pattern(_(byte-$b)_))
    public class LiteralByteSyntax : ExprSyntax
    {
        [Bind("$b")](Bind(_$b_))
        public byte b;

        public override IExpression GetExpr()
        {
 	    return new LiteralExpr(b);
        }
    }
}}

This creates a syntax rule that will match a Scheme datum of the form {{ (byte $b) }} and, on success, create an instance of the {{ LiteralByteSyntax }} class.

You can also create an abstract class and then mark it with the {{ [DescendantsWithPatterns](DescendantsWithPatterns) }} attribute. The parser generator will search the entire assembly for all the descendants of the abstract class which have a {{ [Pattern](Pattern) }} attribute. This also works with interfaces; it will find all the implementing classes in the assembly.

You can only bind to public data members. The types of the bound members are used for further (possibly recursive) parsing. You can have a data member whose type is an abstract class with the {{ [DescendantsWithPatterns](DescendantsWithPatterns) }} attribute, so that one parser fits within another.