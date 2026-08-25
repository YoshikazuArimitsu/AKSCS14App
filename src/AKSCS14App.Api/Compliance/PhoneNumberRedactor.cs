using Microsoft.Extensions.Compliance.Redaction;

namespace CS14App.Api.Compliance;

public sealed class PhoneNumberRedactor : Redactor
{
    private const char MaskChar = '*';
    private const int VisibleSuffixLength = 4;

    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (source.Length <= VisibleSuffixLength)
        {
            destination[..source.Length].Fill(MaskChar);
            return source.Length;
        }

        var maskedLength = source.Length - VisibleSuffixLength;
        destination[..maskedLength].Fill(MaskChar);
        source[maskedLength..].CopyTo(destination[maskedLength..source.Length]);
        return source.Length;
    }
}