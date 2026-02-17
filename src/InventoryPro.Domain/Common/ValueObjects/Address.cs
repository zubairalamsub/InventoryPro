namespace InventoryPro.Domain.Common.ValueObjects;

public record Address
{
    public string? Street { get; init; }
    public string? Street2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }

    private Address() { }

    public Address(
        string? street,
        string? street2,
        string? city,
        string? state,
        string? postalCode,
        string? country)
    {
        Street = street;
        Street2 = street2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Empty => new();

    public string GetFullAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street))
            parts.Add(Street);

        if (!string.IsNullOrWhiteSpace(Street2))
            parts.Add(Street2);

        if (!string.IsNullOrWhiteSpace(City))
            parts.Add(City);

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);

        if (!string.IsNullOrWhiteSpace(Country))
            parts.Add(Country);

        return string.Join(", ", parts);
    }

    public override string ToString() => GetFullAddress();
}
