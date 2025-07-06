using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniEcommerce.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } = string.Empty;


        public ICollection<Product>? Products { get; set; }
    }
}