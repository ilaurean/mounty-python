using AwesomeAssertions;
using Commons.Domain;

namespace Commons.Test.Domain;

public class GenerateUniqueIDTests
{
    [Fact]
    public void It_Should_Generate_Unique_Id_Using_Guid_And_DateTime()
    {
        var guid = Guid.NewGuid();
        var ticks1 = DateTime.UtcNow.Ticks;
        var ticks2 = DateTime.UtcNow.Ticks;

        var id1 = guid.GetHashCode().ToString("x") + ticks1.ToString("x");
        var id2 = guid.GetHashCode().ToString("x") + ticks2.ToString("x");
        Console.WriteLine($"ID1: {id1}, ID2: {id2}");
        id1.Should().NotBeNull();
        id2.Should().NotBeNull();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void It_Should_Generate_Unique_Id_Using_RNGCrypto()
    {
        var id1 = AuditableEntity.GenerateUniqueID();
        var id2 = AuditableEntity.GenerateUniqueID();
        Console.WriteLine($"ID1: {id1}, ID2: {id2}");
        id1.Should().NotBeNull();
        id1.Should().HaveLength(AuditableEntity.idLength);
        id1.Should().Be(id1.ToUpperInvariant());
        id2.Should().NotBeNull();
        id2.Should().HaveLength(AuditableEntity.idLength);
        id2.Should().Be(id2.ToUpperInvariant());
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void It_Should_Check_Unique_ID_Format()
    {
        // TODO: check with regular expressions
        var id = AuditableEntity.GenerateUniqueID();
        var format = AuditableEntity.idFormat;
        id.Should().NotBeNull();
        id.Should().HaveLength(AuditableEntity.idLength);
        id = id.Replace("-", "");
        id.Should().HaveLength(AuditableEntity.idLength - 1);

        for (int i = 0; i < AuditableEntity.idLength - 1; i++)
        {
            AuditableEntity.chars.Any(x => x == id[i]).Should().BeTrue();
        }
    }
}
