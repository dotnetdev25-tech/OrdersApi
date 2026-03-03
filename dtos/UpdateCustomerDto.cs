public record UpdateCustomerDto
{
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;

    public UpdateCustomerDto(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public void Deconstruct(out string name, out string email) => (name, email) = (Name, Email);
}
