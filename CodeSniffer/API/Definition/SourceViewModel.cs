using System;
using System.ComponentModel.DataAnnotations;

namespace CodeSniffer.API.Definition
{
    public class ListSourceViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public ListSourceViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }


    public class ListSourceGroupViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public ListSourceGroupViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /*
    public class SourceViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? SourceGroupId { get; set; }

        public SourceCheckViewModel[]? Checks { get; set; }
    }


    public class SourceSourceViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required]
        public Guid? PluginId { get; set; }
        
        public string? Configuration { get; set; }
    }


    public class SourceCheckViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string? Name { get; set; }

        [Required]
        public Guid? PluginId { get; set; }
        
        public string? Configuration { get; set; }
    }
*/
}
