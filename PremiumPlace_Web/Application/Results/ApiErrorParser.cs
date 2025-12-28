using System.Text.Json;

namespace PremiumPlace_Web.Application.Results
{
    public static class ApiErrorParser
    {
        public static async Task<(string? message, IReadOnlyDictionary<string, string[]>? errors)> ParseAsync(
            HttpResponseMessage resp, CancellationToken ct)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return (resp.ReasonPhrase, null);

            // Plain text
            if (!LooksLikeJson(body))
                return (body.Trim(), null);

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // message
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("message", out var msg) &&
                    msg.ValueKind == JsonValueKind.String)
                {
                    return (msg.GetString(), TryReadErrors(root));
                }

                // errors (ModelState / ValidationProblemDetails)
                var errors = TryReadErrors(root);
                if (errors is not null && errors.Count > 0)
                    return ("Validation failed.", errors);

                // root string JSON: "..."
                if (root.ValueKind == JsonValueKind.String)
                    return (root.GetString(), null);

                return (resp.ReasonPhrase, null);
            }
            catch
            {
                return (body.Trim(), null);
            }
        }

        private static bool LooksLikeJson(string s)
        {
            s = s.Trim();
            return (s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")) || (s.StartsWith("\"") && s.EndsWith("\""));
        }

        private static IReadOnlyDictionary<string, string[]>? TryReadErrors(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object) return null;

            if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Object)
            {
                var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

                foreach (var prop in errorsEl.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var arr = prop.Value.EnumerateArray()
                            .Where(x => x.ValueKind == JsonValueKind.String)
                            .Select(x => x.GetString()!)
                            .ToArray();

                        dict[prop.Name] = arr;
                    }
                }

                return dict;
            }

            return null;
        }
    }
}
