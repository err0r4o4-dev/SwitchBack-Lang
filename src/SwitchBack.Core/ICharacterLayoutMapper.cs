namespace SwitchBack.Core;

public interface ICharacterLayoutMapper
{
    string SourceLayoutId { get; }

    string TargetLayoutId { get; }

    bool TryMap(char input, out string output);
}
