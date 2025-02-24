using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class XmlToJsonController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public XmlToJsonController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> ConvertToJson([FromBody] UrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.XmlDataUrl))
        {
            return BadRequest("A valid URL is required.");
        }

        try
        {

            string xmlContent = await _httpClient.GetStringAsync(request.XmlDataUrl);
            XDocument xmlDoc = XDocument.Parse(xmlContent);
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";


            var simpleTypes = xmlDoc.Descendants(xs + "simpleType")
                .Select(s => new
                {
                    Name = s.Attribute("name")?.Value,
                    Description = s.Element(xs + "annotation")?.Element(xs + "documentation")?.Value,
                    Enumerations = s.Descendants(xs + "enumeration")
                        .Where(e => e.Attribute("value")?.Value != null ||
                                    e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value != null)
                        .Select(e => new
                        {
                            Value = e.Attribute("value")?.Value,
                            Description = e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value
                        })
                        .Where(e => !string.IsNullOrWhiteSpace(e.Description))
                        .ToList()
                })
                .Where(s => !string.IsNullOrWhiteSpace(s.Name) && !string.IsNullOrWhiteSpace(s.Description) || s.Enumerations.Any())
                .ToList();


            var complexTypes = xmlDoc.Descendants(xs + "complexType")
                .Select(c => new
                {
                    Name = c.Attribute("name")?.Value,
                    Description = c.Element(xs + "annotation")?.Element(xs + "documentation")?.Value,
                    Attributes = c.Descendants(xs + "attribute")
                        .Where(a => !string.IsNullOrWhiteSpace(a.Attribute("name")?.Value) &&
                                    !string.IsNullOrWhiteSpace(a.Element(xs + "annotation")?.Element(xs + "documentation")?.Value))
                        .Select(a => new
                        {
                            Name = a.Attribute("name")?.Value,
                            Description = a.Element(xs + "annotation")?.Element(xs + "documentation")?.Value
                        })
                        .Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Description))
                        .ToList(),
                    NestedElements = c.Descendants(xs + "element")
                        .Where(e => !string.IsNullOrWhiteSpace(e.Attribute("name")?.Value) &&
                                    !string.IsNullOrWhiteSpace(e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value))
                        .Select(e => new
                        {
                            Name = e.Attribute("name")?.Value,
                            Description = e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value
                        })
                        .Where(e => !string.IsNullOrWhiteSpace(e.Name) && !string.IsNullOrWhiteSpace(e.Description))
                        .ToList()
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.Name) &&
                            (!string.IsNullOrWhiteSpace(c.Description) || c.Attributes.Any() || c.NestedElements.Any()))
                .ToList();


            var result = new
            {
                SimpleTypes = simpleTypes,
                ComplexTypes = complexTypes
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
