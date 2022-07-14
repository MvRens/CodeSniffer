using System;
using System.ComponentModel.DataAnnotations;

namespace CodeSniffer.API.Definition
{
    public class ListDefinitionViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public ListDefinitionViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }


    public class DefinitionViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? SourceGroupId { get; set; }

        public DefinitionCheckViewModel[]? Checks { get; set; }
    }



    public class DefinitionCheckViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required]
        public Guid? PluginId { get; set; }
        
        public string? Configuration { get; set; }
    }
}
