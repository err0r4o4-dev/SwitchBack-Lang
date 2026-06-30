using System.Text;

namespace SwitchBack.Core;

public sealed class MixedLayoutTextConverter
{
    public LayoutConversionResult Convert(
        string input,
        ICharacterLayoutMapper sourceToTarget,
        ICharacterLayoutMapper targetToSource,
        LayoutConversionBehavior behavior)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(sourceToTarget);
        ArgumentNullException.ThrowIfNull(targetToSource);

        var output = new StringBuilder(input.Length);
        var convertedCharacterCount = 0;

        foreach (var character in input)
        {
            if (char.IsWhiteSpace(character) || char.IsControl(character))
            {
                output.Append(character);
                continue;
            }

            if (TryAppendMapped(character, sourceToTarget, output))
            {
                convertedCharacterCount++;
                continue;
            }

            if (behavior == LayoutConversionBehavior.SwapBothLayouts &&
                TryAppendMapped(character, targetToSource, output))
            {
                convertedCharacterCount++;
                continue;
            }

            output.Append(character);
        }

        return new LayoutConversionResult(
            input,
            output.ToString(),
            sourceToTarget.SourceLayoutId,
            sourceToTarget.TargetLayoutId,
            convertedCharacterCount);
    }

    private static bool TryAppendMapped(
        char input,
        ICharacterLayoutMapper mapper,
        StringBuilder output)
    {
        if (!mapper.TryMap(input, out var mapped) || string.IsNullOrEmpty(mapped) || mapped == input.ToString())
        {
            return false;
        }

        output.Append(mapped);
        return true;
    }
}
