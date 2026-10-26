using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commons.Domain;

/// <summary>
/// This is a pseudo value object implementation, compatible with EFCore before adding support for ComplexType. 
/// Contains a propertyChange, inspired by WPF, to allow for change tracking and dirty checking
/// => UpdateDate is part of business, exported as DTO, and not part of DB update tracked entities
/// </summary>
public abstract class ValueObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected ValueObject() { }

    [Key()]
    public Guid PK { get; protected set; } = Guid.Empty;

    [Timestamp]
    public virtual byte[]? Timestamp { get; private set; }

    [NotMapped]
    public bool IsDirty { get; private set; }

    protected virtual void ResetDirty()
    {
        IsDirty = false;
    }

    protected virtual void SetDirty()
    {
        IsDirty = true;
    }

    public virtual void OnPropertyChanged(string propertyName, object before, object after)
    {
        if (propertyName != nameof(IsDirty))
        {
            IsDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public override string ToString()
    {
        return $"{GetType().Name}, {PK}";
    }

    public static bool operator ==(ValueObject e1, ValueObject e2)
    {
        if (ReferenceEquals(e1, e2))
        {
            return true;
        }

        if (e1 is null || e2 is null)
        {
            return false;
        }

        return e1.PK == e2.PK;
    }

    public static bool operator !=(ValueObject e1, ValueObject e2)
    {
        return !(e1 == e2);
    }

    public virtual bool Equals(ValueObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        return PK == obj.PK;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not ValueObject cat)
        {
            return false;
        }

        return PK == cat.PK;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return PK.GetHashCode();
        }
    }
}
