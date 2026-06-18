using System.Globalization;

namespace PoeltlSolver.Models;

public sealed record JerseyNumber(string Text, int NumericValue)
{
    public static JerseyNumber? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        if (text.All(char.IsDigit))
        {
            return new JerseyNumber(text, int.Parse(text, CultureInfo.InvariantCulture));
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var legacyNumber)
            && legacyNumber == decimal.Truncate(legacyNumber))
        {
            var numericValue = (int)legacyNumber;
            return new JerseyNumber(numericValue.ToString(CultureInfo.InvariantCulture), numericValue);
        }

        throw new FormatException($"Invalid jersey number: '{value}'.");
    }

    public override string ToString()
    {
        return Text;
    }
}
