using SwitchBack.SystemServices;

namespace SwitchBack.Tests;

public sealed class KeyboardInputServiceTests
{
    [Fact]
    public void NativeInputStructureMatchesWindowsAbi()
    {
        var expectedSize = Environment.Is64BitProcess ? 40 : 28;

        Assert.Equal(expectedSize, KeyboardInputService.NativeInputSize);
    }
}
