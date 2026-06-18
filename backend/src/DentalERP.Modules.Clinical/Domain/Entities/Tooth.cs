namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class Tooth
{
    public short Id { get; private set; }
    public short FdiNumber { get; private set; }
    public short? UniversalNumber { get; private set; }
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string Jaw { get; private set; } = string.Empty;      // Upper | Lower
    public string Side { get; private set; } = string.Empty;     // Right | Left
    public string ToothType { get; private set; } = string.Empty; // Incisor|Canine|Premolar|Molar
    public bool IsPrimary { get; private set; }
    public short Position { get; private set; }
}
