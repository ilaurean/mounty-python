using Commons.Contracts.Events;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace Commons.Domain;

/// <summary>
/// This an Entity with minimal auditable info: when created, updated, (soft)-deleted, independently of business requirements.
/// </summary>
public abstract class AuditableEntity : ValueObject
{
    public static readonly string idFormat = "XXX-XXX";
    public static readonly int idLength = idFormat.Length;

    protected AuditableEntity() { }

    [Required]
    [StringLength(10)]
    public string ID { get; protected set; } = GenerateUniqueID();

    [Required]
    public DateTime CreateDate { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdateDate { get; protected set; }
    public DateTime? DeleteDate { get; protected set; }

    public virtual void Delete(DateTime? datetime = null)
    {
        DeleteDate = datetime ?? DateTime.UtcNow;
    }

    [NotMapped]
    public bool IsDisabled => DeleteDate.HasValue;

    [NotMapped]
    public IList<DomainEvent> Events { get; private set; } = [];

    protected override void ResetDirty()
    {
        base.ResetDirty();
        // TODO: should be set to the previous value, not to null
        UpdateDate = null;
    }

    protected override void SetDirty()
    {
        base.SetDirty();
        UpdateDate = DateTime.UtcNow;
    }

    public override void OnPropertyChanged(string propertyName, object before, object after)
    {
        if (propertyName != nameof(IsDirty))
        {
            UpdateDate = DateTime.UtcNow;
        }
        base.OnPropertyChanged(propertyName, before, after);
    }

    public override string ToString()
    {
        return $"{base.ToString()}, ID: {ID}, Registration Date: {CreateDate:u}, Latest Update Date: {UpdateDate?.ToString("u")}, Disable Date: {DeleteDate?.ToString("u")}";
    }

    #region ID generator
    private static readonly RandomNumberGenerator random = RandomNumberGenerator.Create();
    public static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GenerateUniqueID()
    {
        var buffer = new byte[idLength];
        random.GetNonZeroBytes(buffer);

        var builder = new StringBuilder(idLength);
        for (int i = 0; i < idLength; i++)
        {
            if (idFormat[i] == 'X')
            {
                builder.Append(chars[buffer[i] % chars.Length]);
            }
            else
            {
                builder.Append('-');
            }
        }
        return builder.ToString();
    }
    #endregion
}
