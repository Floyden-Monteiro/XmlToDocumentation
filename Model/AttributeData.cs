public class AttributeData
{
    public string Attribute { get; set; }
    public string Description { get; set; }
    public List<NestedElement> NestedElements { get; set; } = new List<NestedElement>();
}