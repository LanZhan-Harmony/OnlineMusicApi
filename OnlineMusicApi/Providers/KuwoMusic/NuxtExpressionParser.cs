using System.Globalization;
using System.Text;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic;

internal static class NuxtExpressionParser
{
    public static IReadOnlyDictionary<string, object?> Parse(string html)
    {
        const string marker = "window.__NUXT__=";
        int start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return new Dictionary<string, object?>();
        }

        int expressionStart = start + marker.Length;
        int end = html.IndexOf("</script>", expressionStart, StringComparison.Ordinal);
        if (end < 0)
        {
            end = html.Length;
        }

        string expression = html[expressionStart..end].Trim().TrimEnd(';');
        int functionIndex = expression.IndexOf("function(", StringComparison.Ordinal);
        if (functionIndex < 0)
        {
            return new Dictionary<string, object?>();
        }

        int paramsStart = functionIndex + "function(".Length;
        int paramsEnd = expression.IndexOf(')', paramsStart);
        if (paramsEnd < 0)
        {
            return new Dictionary<string, object?>();
        }

        string[] names = expression[paramsStart..paramsEnd]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        int returnIndex = expression.IndexOf("return", paramsEnd, StringComparison.Ordinal);
        int objectStart = returnIndex < 0 ? -1 : expression.IndexOf('{', returnIndex);
        int objectEnd = objectStart < 0 ? -1 : FindMatching(expression, objectStart, '{', '}');
        int argsStart = objectEnd < 0 ? -1 : expression.IndexOf('(', objectEnd);
        int argsEnd = argsStart < 0 ? -1 : expression.LastIndexOf(')');
        if (argsStart < 0 || argsEnd <= argsStart)
        {
            return new Dictionary<string, object?>();
        }

        Parser parser = new(expression);
        List<object?> args = parser.ParseValues(argsStart + 1, argsEnd);
        Dictionary<string, object?> variables = [];
        for (int index = 0; index < names.Length && index < args.Count; index++)
        {
            variables[names[index]] = args[index];
        }

        parser.SetVariables(variables);
        parser.SetPosition(objectStart);
        return parser.ParseValue() as IReadOnlyDictionary<string, object?>
            ?? new Dictionary<string, object?>();
    }

    private static int FindMatching(string text, int openIndex, char open, char close)
    {
        int depth = 0;
        bool inString = false;
        char quote = '\0';

        for (int index = openIndex; index < text.Length; index++)
        {
            char current = text[index];
            if (inString)
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == quote)
                {
                    inString = false;
                }

                continue;
            }

            if (current is '"' or '\'')
            {
                inString = true;
                quote = current;
                continue;
            }

            if (current == open)
            {
                depth++;
            }
            else if (current == close)
            {
                depth--;
                if (depth == 0)
                {
                    return index;
                }
            }
        }

        return -1;
    }

    private sealed class Parser(string text)
    {
        private readonly string _text = text;
        private int _position;
        private Dictionary<string, object?> _variables = [];

        public void SetVariables(Dictionary<string, object?> variables)
        {
            _variables = variables;
        }

        public void SetPosition(int position)
        {
            _position = position;
        }

        public object? ParseValue()
        {
            SkipWhitespace();
            if (_position >= _text.Length)
            {
                return null;
            }

            char current = _text[_position];
            if (current == '{')
            {
                return ParseObject();
            }

            if (current == '[')
            {
                return ParseArray();
            }

            if (current is '"' or '\'')
            {
                return ParseString();
            }

            if (current == '!' && _position + 1 < _text.Length)
            {
                _position++;
                return _text[_position++] == '0';
            }

            if (current == '-' || char.IsDigit(current))
            {
                return ParseNumber();
            }

            if (StartsWith("void"))
            {
                _position += 4;
                SkipWhitespace();
                if (_position < _text.Length && _text[_position] == '0')
                {
                    _position++;
                }

                return null;
            }

            if (StartsWith("null"))
            {
                _position += 4;
                return null;
            }

            if (StartsWith("true"))
            {
                _position += 4;
                return true;
            }

            if (StartsWith("false"))
            {
                _position += 5;
                return false;
            }

            string name = ParseIdentifierName();
            return _variables.GetValueOrDefault(name);
        }

        public List<object?> ParseValues(int start, int end)
        {
            _position = start;
            List<object?> values = [];

            while (_position < end)
            {
                SkipWhitespace();
                if (_position >= end)
                {
                    break;
                }

                values.Add(ParseValue());
                SkipWhitespace();
                if (_position < end && _text[_position] == ',')
                {
                    _position++;
                    continue;
                }

                break;
            }

            return values;
        }

        private Dictionary<string, object?> ParseObject()
        {
            Dictionary<string, object?> result = [];
            _position++;

            while (_position < _text.Length)
            {
                SkipWhitespace();
                if (_text[_position] == '}')
                {
                    _position++;
                    break;
                }

                string key = _text[_position] is '"' or '\''
                    ? ParseString() ?? string.Empty
                    : ParseIdentifierName();

                SkipWhitespace();
                if (_position < _text.Length && _text[_position] == ':')
                {
                    _position++;
                }

                result[key] = ParseValue();
                SkipWhitespace();

                if (_position < _text.Length && _text[_position] == ',')
                {
                    _position++;
                    continue;
                }

                if (_position < _text.Length && _text[_position] == '}')
                {
                    _position++;
                    break;
                }

                break;
            }

            return result;
        }

        private List<object?> ParseArray()
        {
            List<object?> result = [];
            _position++;

            while (_position < _text.Length)
            {
                SkipWhitespace();
                if (_text[_position] == ']')
                {
                    _position++;
                    break;
                }

                result.Add(ParseValue());
                SkipWhitespace();

                if (_position < _text.Length && _text[_position] == ',')
                {
                    _position++;
                    continue;
                }

                if (_position < _text.Length && _text[_position] == ']')
                {
                    _position++;
                    break;
                }

                break;
            }

            return result;
        }

        private string? ParseString()
        {
            char quote = _text[_position++];
            StringBuilder builder = new();

            while (_position < _text.Length)
            {
                char current = _text[_position++];
                if (current == quote)
                {
                    return builder.ToString();
                }

                if (current != '\\' || _position >= _text.Length)
                {
                    builder.Append(current);
                    continue;
                }

                char escaped = _text[_position++];
                switch (escaped)
                {
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'u':
                        if (_position + 4 <= _text.Length
                            && int.TryParse(
                                _text.AsSpan(_position, 4),
                                NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture,
                                out int code))
                        {
                            builder.Append((char)code);
                            _position += 4;
                        }

                        break;
                    default:
                        builder.Append(escaped);
                        break;
                }
            }

            return builder.ToString();
        }

        private object? ParseNumber()
        {
            int start = _position;
            if (_text[_position] == '-')
            {
                _position++;
            }

            while (_position < _text.Length
                && (char.IsDigit(_text[_position])
                    || _text[_position] is '.' or 'e' or 'E' or '+' or '-'))
            {
                _position++;
            }

            string raw = _text[start.._position];
            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
            {
                return integer;
            }

            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                ? number
                : null;
        }

        private string ParseIdentifierName()
        {
            int start = _position;
            while (_position < _text.Length
                && (char.IsLetterOrDigit(_text[_position])
                    || _text[_position] is '_' or '$'))
            {
                _position++;
            }

            return _text[start.._position];
        }

        private bool StartsWith(string value)
        {
            return _text.AsSpan(_position).StartsWith(value, StringComparison.Ordinal);
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }
    }
}