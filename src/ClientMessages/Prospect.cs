namespace ClientMessages;

public record Prospect(string Name, string Surname, string SSN)
{
    public override string ToString() => $"{Name} {Surname}";
}