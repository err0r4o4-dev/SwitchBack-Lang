using SwitchBack.SystemServices;

namespace SwitchBack.Tests;

public sealed class KeyboardInputServiceTests
{
    [Fact]
    public void NativeInputStructureMatchesWindowsAbi()
    {
        var expectedSize = Environment.Is64BitProcess ? 40 : 28;

        Assert.Equal(expectedSize, KeyboardInputService.NativeInputSize);

        if (Environment.GetEnvironmentVariable("SWITCHBACK_EXPECT_X86") == "1")
        {
            Assert.False(Environment.Is64BitProcess);
            Assert.Equal(28, KeyboardInputService.NativeInputSize);
        }
    }
}
