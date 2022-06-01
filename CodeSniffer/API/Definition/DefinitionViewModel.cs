using System.ComponentModel.DataAnnotations;

namespace CodeSniffer.API.Definition
{
    public class DefinitionViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        public DefinitionSourceViewModel[]? Sources { get; set; }
        public DefinitionCheckViewModel[]? Checks { get; set; }
    }


    public class DefinitionSourceViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? PluginName { get; set; }
        
        public string? Configuration { get; set; }
    }


    public class DefinitionCheckViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? PluginName { get; set; }
        
        public string? Configuration { get; set; }
    }
}
