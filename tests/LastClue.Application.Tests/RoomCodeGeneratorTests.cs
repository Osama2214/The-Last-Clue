using LastClue.Application.Rooms;

namespace LastClue.Application.Tests;

public sealed class RoomCodeGeneratorTests
{
    [Theory]
    [InlineData(6)]
    [InlineData(12)]
    public void Generated_codes_use_only_unambiguous_characters(int length)
    {
        var generator = new RoomCodeGenerator();
        for (var sample = 0; sample < 100; sample++)
        {
            var code = generator.Generate(length);
            Assert.Equal(length, code.Length);
            Assert.All(code, character => Assert.Contains(character, RoomCodeGenerator.Alphabet));
            Assert.DoesNotContain(code, character => "0O1I".Contains(character));
        }
    }
}
