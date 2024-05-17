namespace ClientMessages;

public record Prospect(string Name, string Surname)
{
    public override string ToString() => $"{Name} {Surname}";
}