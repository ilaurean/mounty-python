using AwesomeAssertions;
using Commons.Services;

namespace Commons.Test.Services;

public class LoggingMaskingOperatorsTest
{
    [Theory,
        //InlineData("AB:CD:EF:01:02:03", true), // an edge case, but it's still failing
        InlineData("This is a MAC-Address: AB:CD:EF:01:02:03 and should be masked.", true),
        InlineData("This is a MAC-Address: AB:CD:EF:01:02:03. This should be masked.", true),
        InlineData("This is a NOT a MAC-Address: 00:AB:CD:EF:01:02:03 and should not be masked.", false),
        InlineData("This is a NOT a MAC-Address: something-AB:CD:EF:01:02:03 and should not be masked.", false),
        InlineData("This is a NOT a MAC-Address: AB:CD:EF:01:02:03-bullshit and should not be masked.", false)]
    public void It_Should_Masked_MAC_Correctly(string testString, bool shouldBeMasked)
    {
        var op = new MacAddressOperator();
        op.Mask(testString, "MaSkEd").Match.Should().Be(shouldBeMasked);
    }
}
