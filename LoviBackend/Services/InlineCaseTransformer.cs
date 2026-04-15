using System.Text.RegularExpressions;

namespace LoviBackend.Services
{
    // 🐫 camelCase
    class InlineCamelCaseTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            var text = value.ToString()!;
            return string.IsNullOrEmpty(text) ? text : char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
    }

    // 🧱 PascalCase (essentially no transformation)
    class InlinePascalCaseTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            var text = value.ToString()!;
            return string.IsNullOrEmpty(text) ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
    }

    // 🐍 snake_case
    class InlineSnakeCaseTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            var text = value.ToString()!;
            if (string.IsNullOrEmpty(text)) return text;

            // Split lower-to-upper (e.g. "myValue" -> "my_Value")
            var result = Regex.Replace(text, "(?<=[a-z0-9])([A-Z])", "_$1");
            // Split upper-acronym followed by Pascal word (e.g. "EBooks" -> "E_Books")
            result = Regex.Replace(result, "(?<=[A-Z])([A-Z][a-z])", "_$1");

            return result.ToLowerInvariant();
        }
    }

    // 🥙 kebab-case
    class InlineKebabCaseTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            var text = value.ToString()!;
            if (string.IsNullOrEmpty(text)) return text;

            // Split lower-to-upper (e.g. "myValue" -> "my-Value")
            var result = Regex.Replace(text, "(?<=[a-z0-9])([A-Z])", "-$1");
            // Split upper-acronym followed by Pascal word (e.g. "EBooks" -> "E-Books")
            result = Regex.Replace(result, "(?<=[A-Z])([A-Z][a-z])", "-$1");

            return result.ToLowerInvariant();
        }
    }

}
