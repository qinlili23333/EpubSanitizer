using css2xpath.Selectors;
using System.Text.RegularExpressions;

// From https://github.com/qinlili23333/css2xpath-reloaded-core
// --- Start of file: Converter.cs ---

namespace css2xpath
{
    public static partial class Converter
    {
        private static readonly Regex ElementRegex = ElementNameRegex();
        private static readonly Regex IdRegex = ElementIdRegex();
        private static readonly Regex ClassRegex = ElementClassRegex();

        public static string CSSToXPath(string expr, string prefix = "descendant-or-self::")
        {
            var elementMatch = ElementRegex.Match(expr);
            if (elementMatch.Success)
            {
                return string.Format("{0}{1}", prefix, elementMatch.Value.Trim());
            }

            var idMatch = IdRegex.Match(expr);
            if (idMatch.Success)
            {
                return string.Format("{0}{1}[@id = '{2}']", prefix, string.IsNullOrEmpty(idMatch.Groups[1].Value) ? "*" : idMatch.Groups[1].Value, idMatch.Groups[2].Value);
            }

            var classMatch = ClassRegex.Match(expr);
            if (classMatch.Success)
            {
                return string.Format("{0}{1}[contains(concat(' ', normalize-space(@class), ' '), ' {2} ')]", prefix, string.IsNullOrEmpty(classMatch.Groups[1].Value) ? "*" : classMatch.Groups[1].Value, classMatch.Groups[2].Value);
            }

            var selector = Parser.Parse(expr);
            var xpath = selector.GetXPath();
            if (!string.IsNullOrEmpty(prefix))
            {
                xpath.AddPrefix(prefix);
            }

            return xpath.ToString();
        }

        [GeneratedRegex(@"^\w+\s*$")]
        private static partial Regex ElementNameRegex();
        [GeneratedRegex(@"^(\w*)#(\w+)\s*$")]
        private static partial Regex ElementIdRegex();
        [GeneratedRegex(@"^(\w*)\.(\w+)\s*$")]
        private static partial Regex ElementClassRegex();
    }
}

// --- Start of file: Exceptions.cs ---

namespace css2xpath
{
    public class SelectorSyntaxException : Exception
    {
        public SelectorSyntaxException() { }
        public SelectorSyntaxException(string msg) : base(msg) { }
    }

    public class ExpressionException : Exception
    {
        public ExpressionException() { }
        public ExpressionException(string msg) : base(msg) { }
    }
}
// --- Start of file: Extensions.cs ---

namespace css2xpath
{
    public static class Extensions
    {
        public static int End(this Match match)
        {
            return match.Index + match.Length;
        }

        public static bool In<T>(this T source, IEnumerable<T> items)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }
            return items.Contains(source);
        }
    }
}
// --- Start of file: Parser.cs ---

namespace css2xpath
{
    public static partial class Parser
    {
        public static ISelector Parse(string input)
        {
            var stream = new TokenStream([.. Tokenizer.Tokenize(input)]);
            return ParseSelectorGroup(stream);
        }

        public static ISelector ParseSelectorGroup(TokenStream stream)
        {
            var result = new List<ISelector>();
            while (true)
            {
                result.Add(ParseSelector(stream));
                if (stream.Peek().Contents == ",")
                {
                    stream.Next();
                }
                else
                {
                    break;
                }
            }

            return result.Count == 1 ? result.Single() : new Or(result);
        }

        public static ISelector ParseSelector(TokenStream stream)
        {
            var result = ParseSimpleSelector(stream);

            while (true)
            {
                var peek = stream.Peek();

                if (peek.Contents == "" || peek.Contents == ",")
                {
                    return result;
                }

                var combinator = peek.Contents.In(["+", ">", "~"]) ? stream.Next().Contents[0] : ' ';

                var nextSelector = ParseSimpleSelector(stream);
                result = new CombinedSelector(result, combinator, nextSelector);
            }
        }

        public static ISelector ParseSimpleSelector(TokenStream stream)
        {
            var peek = stream.Peek();
            string element;
            string xNamespace;
            if (peek.Contents != "*" && peek is not Symbol)
            {
                element = "*";
                xNamespace = "*";
            }
            else
            {
                var next = stream.Next();
                if (next.Contents != "*" && next is not Symbol)
                {
                    throw new SelectorSyntaxException(string.Format("Expected symbol, got {0}", next.GetType().Name));
                }

                if (stream.Peek().Contents == "|")
                {
                    xNamespace = next.Contents;
                    stream.Next();
                    element = stream.Next().Contents;
                    if (element != "*" && next is not Symbol)
                    {
                        throw new SelectorSyntaxException(string.Format("Expected symbol, got {0}", next.GetType().Name));
                    }
                }
                else
                {
                    xNamespace = "*";
                    element = next.Contents;
                }
            }

            ISelector result = new Element(xNamespace, element);
            var hasHash = false;

            while (true)
            {
                peek = stream.Peek();

                if (peek.Contents == "#")
                {
                    if (hasHash)
                    {
                        break;
                    }

                    stream.Next();
                    result = new Hash(result, stream.Next().Contents);
                    hasHash = true;
                }
                else if (peek.Contents == ".")
                {
                    stream.Next();
                    result = new Class(result, stream.Next().Contents);
                }
                else if (peek.Contents == "[")
                {
                    stream.Next();
                    result = ParseAttribute(result, stream);
                    var next = stream.Next();
                    if (next.Contents != "]")
                    {
                        throw new SelectorSyntaxException(string.Format("] expected, got {0}", next.Contents));
                    }
                }
                else if (peek.Contents == ":" || peek.Contents == "::")
                {
                    stream.Next();
                    var function = stream.Next();
                    if (function is not Symbol)
                    {
                        throw new SelectorSyntaxException(string.Format("Expected symbol, got {0}", function));
                    }

                    if (stream.Peek().Contents == "(")
                    {
                        stream.Next();
                        peek = stream.Peek();

                        Object selector;
                        if (peek is Str)
                        {
                            selector = stream.Next().Contents;
                        }
                        else if (peek is Symbol && int.TryParse(peek.Contents, out int asInt))
                        {
                            selector = asInt;
                            stream.Next();
                        }
                        else
                        {
                            selector = ParseSimpleSelector(stream);
                        }

                        var next = stream.Next();
                        if (next.Contents != ")")
                        {
                            throw new SelectorSyntaxException(string.Format("Expected ), got {0} and {1}", next.Contents, selector));
                        }

                        result = new Function(result, function.Contents, selector);
                    }
                    else
                    {
                        result = new Pseudo(result, function.Contents);
                    }
                }
                else
                {
                    if (peek.Contents == " ")
                    {
                        stream.Next();
                    }

                    break;
                }
            }

            return result;
        }

        public static Attrib ParseAttribute(ISelector selector, TokenStream stream)
        {
            var attrib = stream.Next();
            string xNamespace;
            if (stream.Peek().Contents == "|")
            {
                xNamespace = attrib.Contents;
                stream.Next();
                attrib = stream.Next();
            }
            else
            {
                xNamespace = "*";
            }

            if (stream.Peek().Contents == "]")
            {
                return new Attrib(selector, xNamespace, attrib.Contents, "exists", null);
            }

            var oper = stream.Next().Contents;
            if (!oper.In(["^=", "$=", "*=", "=", "~=", "|=", "!="]))
            {
                throw new SelectorSyntaxException(string.Format("Operator expected, got {0}", oper));
            }

            var value = stream.Next();
            if (value is Symbol || value is Str)
            {
                return new Attrib(selector, xNamespace, attrib.Contents, oper, value.Contents);
            }

            throw new SelectorSyntaxException(string.Format("Expected string or symbol, got {0}", value));
        }

        public static Tuple<int, int> ParseSeries(object obj)
        {
            if (obj is int v)
            {
                return new Tuple<int, int>(0, v);
            }

            var text = obj.ToString().Replace(" ", "");
            if (text == "*")
            {
                return new Tuple<int, int>(0, 0);
            }

            if (text == "odd")
            {
                return new Tuple<int, int>(2, 1);
            }

            if (text == "even")
            {
                return new Tuple<int, int>(2, 0);
            }

            if (text == "n")
            {
                return new Tuple<int, int>(1, 0);
            }

            if (!text.Contains('n'))
            {
                return new Tuple<int, int>(0, int.Parse(text));
            }

            var regex = SeriesRegex();
            var match = regex.Match(text.Trim());
            if (!match.Success)
            {
                throw new ExpressionException(string.Format("Invalid series: {0}", text));
            }

            var a = int.Parse(match.Groups[1].Value);
            var b = int.Parse(match.Groups[2].Value);
            return new Tuple<int, int>(a, b);
        }

        [GeneratedRegex(@"^([+\-]?\d+)?n([+\-]\d+)$")]
        private static partial Regex SeriesRegex();
    }
}
// --- Start of file: Tokenizer.cs ---

namespace css2xpath
{
    public partial class Tokenizer
    {
        private static readonly Regex CommentsRegex = CommentBlockRegex();
        private static readonly Regex WhitespaceRegex = WhitespaceBlockRegex();
        private static readonly Regex CountRegex = NthExpressionRegex();
        private static readonly Regex IllegalSymbolRegex = IllegalSymbolBlockRegex();

        public static IEnumerable<Token> Tokenize(string input)
        {
            var pos = 0;

            // Strip comments
            input = CommentsRegex.Replace(input, "");

            while (true)
            {
                var whitespaceMatch = WhitespaceRegex.Match(input, pos);
                int precedingWhitespacePos;
                if (whitespaceMatch.Success)
                {
                    precedingWhitespacePos = pos;
                    pos = whitespaceMatch.End();
                }
                else
                {
                    precedingWhitespacePos = 0;
                }

                if (pos >= input.Length)
                {
                    yield break;
                }

                var countMatch = CountRegex.Match(input, pos);
                if (countMatch.Success && countMatch.Value != "n")
                {
                    yield return new Symbol(input[pos..countMatch.End()]);
                    pos = countMatch.End();
                    continue;
                }

                var c = input[pos];
                var c2 = (pos + 2 > input.Length) ? "" : input.Substring(pos, 2);

                if (c2.In(["~=", "|=", "^=", "$=", "*=", "::", "!="]))
                {
                    yield return new Token(c2);
                    pos += 2;
                    continue;
                }

                if (">+~,.*=[]()|:#".Contains(c))
                {
                    if (".#".Contains(c) && precedingWhitespacePos > 0)
                    {
                        yield return new Token(" ");
                    }

                    yield return new Token(c.ToString());
                    pos += 1;
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    yield return TokenizeEscapedString(input, ref pos);
                    continue;
                }

                yield return TokenizeSymbol(input, ref pos);
            }
        }

        public static Str TokenizeEscapedString(string input, ref int pos)
        {
            var quote = input[pos];
            var start = pos + 1;
            var next = input.IndexOf(quote, start);
            if (next == -1)
            {
                throw new SelectorSyntaxException(string.Format("Expected closing {0} for string in: {1}", quote, input[start..]));
            }

            // TODO: Still need to actually perform the escaping of backslashes?

            pos = next + 1;
            return new Str(input[start..next]);
        }

        public static Symbol TokenizeSymbol(string input, ref int pos)
        {
            var start = pos;
            var match = IllegalSymbolRegex.Match(input, pos);
            if (!match.Success)
            {
                pos = input.Length;
                return new Symbol(input[start..]);
            }

            if (match.Index == pos)
            {
                throw new SelectorSyntaxException(string.Format("Unexpected symbol: {0} at {1}", input[pos], pos));
            }

            string result;

            if (!match.Success)
            {
                result = input[start..];
                pos = input.Length;
            }
            else
            {
                result = input[start..match.Index];
                pos = match.Index;
            }

            // TODO: More backslash escaping stuff?

            return new Symbol(result);
        }

        [GeneratedRegex(@"/\*.*?\*/")]
        private static partial Regex CommentBlockRegex();
        [GeneratedRegex(@"\G\s+")]
        private static partial Regex WhitespaceBlockRegex();
        [GeneratedRegex(@"\G[+-]?\d*n(?:[+-]\d+)?")]
        private static partial Regex NthExpressionRegex();
        [GeneratedRegex(@"[^\w\\-]")]
        private static partial Regex IllegalSymbolBlockRegex();
    }
}
// --- Start of file: Tokens.cs ---
namespace css2xpath
{
    public class Token(string contents)
    {
        private readonly string _contents = contents;

        public string Contents
        {
            get { return this._contents; }
        }
    }

    public class Symbol(string contents) : Token(contents)
    {
    }

    public class Str(string contents) : Token(contents)
    {
    }
}
// --- Start of file: TokenStream.cs ---

namespace css2xpath
{
    public class TokenStream(IEnumerable<Token> items) : Queue<Token>(items)
    {
        public Token Next()
        {
            Token next;
            try
            {
                next = this.Dequeue();
            }
            catch (InvalidOperationException)
            {
                return new Token("");
            }

            return next;
        }

        public new Token Peek()
        {
            Token peek;
            try
            {
                peek = base.Peek();
            }
            catch (InvalidOperationException)
            {
                return new Token("");
            }

            return peek;
        }
    }
}
// --- Start of file: XPathExpr.cs ---
namespace css2xpath
{
    public class XPathExpr(string prefix = null, string path = null, string element = "*", string condition = null, bool starPrefix = false)
    {
        private string _condition = condition;
        private string _element = element;
        private string _path = path;
        private string _prefix = prefix;
        private bool _starPrefix = starPrefix;

        public override string ToString()
        {
            string path = "";
            path += this._prefix ?? "";
            path += this._path ?? "";
            path += this._element;

            if (this._condition != null)
            {
                path += string.Format("[{0}]", this._condition);
            }

            return path;
        }

        public void AddPrefix(string prefix)
        {
            if (this._prefix != null)
            {
                this._prefix = prefix + this._prefix;
            }
            else
            {
                this._prefix = prefix;
            }
        }

        public void AddCondition(string condition)
        {
            if (this._condition != null)
            {
                this._condition = string.Format("{0} and ({1})", this._condition, condition);
            }
            else
            {
                this._condition = condition;
            }
        }

        public void AddPath(string part)
        {
            if (this._path == null)
            {
                this._path = this._element;
            }
            else
            {
                this._path += this._element;
            }

            this._element = part;
        }

        public void AddNameTest()
        {
            if (this._element == "*")
            {
                return;
            }

            this.AddCondition(string.Format("name() = '{0}'", this._element));
            this._element = "*";
        }

        public void AddStarPrefix()
        {
            if (this._path != null)
            {
                this._path += "*/";
            }
            else
            {
                this._path = "*/";
            }

            this._starPrefix = true;
        }

        public void Join(string combiner, XPathExpr other)
        {
            string prefix = this + combiner;
            string path = (other._prefix ?? "") + (other._path ?? "");

            if (other._starPrefix && path == "*/")
            {
                path = "";
            }

            this._prefix = prefix;
            this._path = path;
            this._element = other._element;
            this._condition = other._condition;
        }

        public string GetElement()
        {
            return this._element;
        }

        public string GetCondition()
        {
            return this._condition;
        }
    }
}
// --- Start of file: XPathExprOr.cs ---

namespace css2xpath
{
    public class XPathExprOr(IEnumerable<XPathExpr> items, string prefix = null) : XPathExpr
    {
        private readonly IEnumerable<XPathExpr> _items = items;
        private readonly string _prefix = prefix;

        public override string ToString()
        {
            string prefix = this._prefix ?? "";
            return string.Join(" | ", this._items.Select(item => prefix + item.ToString()));
        }
    }
}
// --- Start of file: Selectors\Attrib.cs ---

namespace css2xpath.Selectors
{
    public class Attrib(ISelector selector, string xNamespace, string attrib, string oper, string value) : ISelector
    {
        private readonly string _attrib = attrib;
        private readonly string _namespace = xNamespace;
        private readonly string _operator = oper;
        private readonly ISelector _selector = selector;
        private readonly string _value = value;

        public XPathExpr GetXPath()
        {
            var result = this._selector.GetXPath();
            var attribute = this._namespace == "*" ? ("@" + this._attrib) : string.Format("@{0}:{1}", this._namespace, this._attrib);

            switch (this._operator)
            {
                case "exists":
                    if (!string.IsNullOrEmpty(this._value))
                    {
                        throw new ExpressionException("Value should be empty");
                    }
                    result.AddCondition(attribute);
                    break;
                case "=":
                    result.AddCondition(string.Format("{0} = '{1}'", attribute, this._value));
                    break;
                case "!=":
                    if (string.IsNullOrEmpty(this._value))
                    {
                        result.AddCondition(string.Format("{0} != '{1}'", attribute, this._value));
                    }
                    else
                    {
                        result.AddCondition(string.Format("not({0}) or {1} != '{2}'", attribute, attribute, this._value));
                    }
                    break;
                case "~=":
                    result.AddCondition(string.Format("contains(concat(' ', normalize-space({0}), ' '), ' {1} ')", attribute, this._value));
                    break;
                case "|=":
                    result.AddCondition(string.Format("{0} = '{1}' or starts-with({2}, '{3}-')", attribute, this._value, attribute, this._value));
                    break;
                case "^=":
                    result.AddCondition(string.Format("starts-with({0}, '{1}')", attribute, this._value));
                    break;
                case "$=":
                    result.AddCondition(string.Format("substring({0}, string-length({1})-{2}) = '{3}'", attribute, attribute, this._value.Length - 1, this._value));
                    break;
                case "*=":
                    result.AddCondition(string.Format("contains({0}, '{1}')", attribute, this._value));
                    break;
                default:
                    throw new NotImplementedException(string.Format("Operator {0} is not supported", this._operator));
            }

            return result;
        }
    }
}
// --- Start of file: Selectors\Class.cs ---
namespace css2xpath.Selectors
{
    public class Class(ISelector selector, string className) : ISelector
    {
        private readonly string _className = className;
        private readonly ISelector _selector = selector;

        public XPathExpr GetXPath()
        {
            var result = this._selector.GetXPath();
            result.AddCondition(string.Format("contains(concat(' ', normalize-space(@class), ' '), ' {0} ')", this._className));
            return result;
        }
    }
}
// --- Start of file: Selectors\CombinedSelector.cs ---
namespace css2xpath.Selectors
{
    public class CombinedSelector(ISelector selector, char combinator, ISelector subselector) : ISelector
    {
        private readonly char _combinator = combinator;
        private readonly ISelector _selector = selector;
        private readonly ISelector _subselector = subselector;

        public XPathExpr GetXPath()
        {
            return this._combinator switch
            {
                ' ' => this.MakeSimpleJoined("/descendant::"),
                '>' => this.MakeSimpleJoined("/"),
                '+' => this.MakeDirectAdjacent(),
                '~' => this.MakeSimpleJoined("/following-sibling::"),
                _ => throw new ExpressionException(string.Format("Unknown combinator: {0}", this._combinator)),
            };
        }

        private XPathExpr MakeSimpleJoined(string combiner)
        {
            var result = this._selector.GetXPath();
            result.Join(combiner, this._subselector.GetXPath());
            return result;
        }

        private XPathExpr MakeDirectAdjacent()
        {
            var result = this.MakeSimpleJoined("/following-sibling::");
            result.AddNameTest();
            result.AddCondition("position() = 1");
            return result;
        }
    }
}
// --- Start of file: Selectors\Element.cs ---
namespace css2xpath.Selectors
{
    public class Element(string xNamespace, string element) : ISelector
    {
        private readonly string _element = element;
        private readonly string _namespace = xNamespace;

        public XPathExpr GetXPath()
        {
            var element = this._element.ToLower();
            if (this._namespace != "*")
            {
                element = string.Format("{0}:{1}", this._namespace, this._element);
            }

            return new XPathExpr(element: element);
        }

        public override string ToString()
        {
            if (this._namespace == "*")
            {
                return this._element;
            }

            return string.Format("{0}|{1}", this._namespace, this._element);
        }
    }
}
// --- Start of file: Selectors\Function.cs ---

namespace css2xpath.Selectors
{
    public class Function(ISelector selector, string name, object param) : ISelector
    {
        private readonly string _name = name;
        private readonly object _param = param;
        private readonly ISelector _selector = selector;

        public XPathExpr GetXPath()
        {
            XPathExpr result = null;

            switch (this._name)
            {
                case "nth-child":
                    return this.MakeNthChild();
                case "nth-last-child":
                    return this.MakeNthChild(last: true);
                case "nth-of-type":
                    if (this._selector.GetXPath().GetElement() == "*")
                    {
                        throw new NotImplementedException("*:nth-of-type() is not implemented");
                    }

                    return this.MakeNthChild(addNameTest: false);
                case "nth-last-of-type":
                    return this.MakeNthChild(last: true, addNameTest: false);
                case "contains":
                    result = this._selector.GetXPath();
                    result.AddCondition(string.Format("contains(css:lower-case(string(.)), '{0}')", this._param.ToString().ToLower()));
                    return result;
                case "not":
                    var condition = ((ISelector)this._param).GetXPath().GetCondition();
                    result = this._selector.GetXPath();
                    result.AddCondition(string.Format("not({0})", condition));
                    return result;
                default:
                    throw new ExpressionException(string.Format("Unsupported function: {0}", this._name));
            }
        }

        private XPathExpr MakeNthChild(bool last = false, bool addNameTest = true)
        {
            var result = this._selector.GetXPath();
            var series = Parser.ParseSeries(this._param);

            if (!last && series.Item1 == 0 && series.Item2 == 0)
            {
                result.AddCondition("false() and position() = 0");
                return result;
            }

            if (addNameTest)
            {
                result.AddNameTest();
            }

            result.AddStarPrefix();

            if (series.Item1 == 0)
            {
                var bText = series.Item2.ToString();
                if (last)
                {
                    bText = string.Format("last() - {0}", bText);
                }
                result.AddCondition(string.Format("position() = {0}", bText));
                return result;
            }

            var a = series.Item1;
            var b = series.Item2;
            if (last)
            {
                a = -a;
                b = -b;
            }

            var bNeg = (-b).ToString();
            if (b <= 0)
            {
                bNeg = string.Format("+{0}", bNeg);
            }

            var expr = new List<string>();
            if (a != 1)
            {
                expr.Add(string.Format("(position() {0}) mod {1} = 0", bNeg, a));
            }

            if (b >= 0)
            {
                expr.Add(string.Format("position() >= {0}", b));
            }
            else if (b < 0 && last)
            {
                expr.Add(string.Format("position() < (last() {0})", b));
            }

            var exprString = string.Join(" and ", expr);
            if (expr.Count > 0)
            {
                result.AddCondition(exprString);
            }

            return result;
        }
    }
}
// --- Start of file: Selectors\Hash.cs ---
namespace css2xpath.Selectors
{
    public class Hash(ISelector selector, string id) : ISelector
    {
        private readonly string _id = id;
        private readonly ISelector _selector = selector;

        public XPathExpr GetXPath()
        {
            var path = this._selector.GetXPath();
            path.AddCondition(string.Format("@id = '{0}'", this._id));
            return path;
        }
    }
}
// --- Start of file: Selectors\ISelector.cs ---
namespace css2xpath.Selectors
{
    public interface ISelector
    {
        XPathExpr GetXPath();
    }
}
// --- Start of file: Selectors\Or.cs ---

namespace css2xpath.Selectors
{
    public class Or(IEnumerable<ISelector> items) : ISelector
    {
        private readonly IEnumerable<ISelector> _items = items;

        public XPathExpr GetXPath()
        {
            var paths = this._items.Select(item => item.GetXPath());
            return new XPathExprOr(paths);
        }
    }
}
// --- Start of file: Selectors\Pseudo.cs ---

namespace css2xpath.Selectors
{
    public class Pseudo(ISelector element, string name) : ISelector
    {
        public XPathExpr GetXPath()
        {
            var result = element.GetXPath();

            switch (name)
            {
                case "checked":
                    result.AddCondition("(@selected or @checked) and (name(.) = 'input' or name(.) = 'option')");
                    break;
                case "first-child":
                    result.AddStarPrefix();
                    result.AddNameTest();
                    result.AddCondition("position() = 1");
                    break;
                case "last-child":
                    result.AddStarPrefix();
                    result.AddNameTest();
                    result.AddCondition("position() = last()");
                    break;
                case "first-of-type":
                    if (result.GetElement() == "*")
                    {
                        throw new NotImplementedException("*:first-of-type is not implemented");
                    }
                    result.AddStarPrefix();
                    result.AddCondition("position() = 1");
                    break;
                case "last-of-type":
                    if (result.GetElement() == "*")
                    {
                        throw new NotImplementedException("*:last-of-type is not implemented");
                    }
                    result.AddStarPrefix();
                    result.AddCondition("position() = last()");
                    break;
                case "only-child":
                    result.AddNameTest();
                    result.AddStarPrefix();
                    result.AddCondition("last() = 1");
                    break;
                case "only-of-type":
                    if (result.GetElement() == "*")
                    {
                        throw new NotImplementedException("*:only-of-type is not implemented");
                    }
                    result.AddCondition("last() = 1");
                    break;
                case "empty":
                    result.AddCondition("not(*) and not(normalize-space())");
                    break;
                default:
                    throw new NotImplementedException(string.Format("The pseudo-selector, {0}, is not implemented", name));
            }

            return result;
        }
    }
}