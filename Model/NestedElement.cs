public class NestedElement
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Enumeration> Enumerations { get; set; } = new List<Enumeration>();
    public List<NestedElement> NestedElements { get; set; } = new List<NestedElement>();
}