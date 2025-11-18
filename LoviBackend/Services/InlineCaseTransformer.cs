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
            return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1_$2").ToLowerInvariant();
        }
    }

    // 🥙 kebab-case
    class InlineKebabCaseTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }

}
