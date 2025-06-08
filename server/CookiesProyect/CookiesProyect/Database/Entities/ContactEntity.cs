using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CookiesProyect.Database.Entities
{
    [Table("contacts", Schema = "dbo")]
    public class ContactEntity : AuditEntity
    {
        [Key]
        [Column("id")]
        [Display(Name = "Identificador")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Column("first_name")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Column("last_name")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [Column("email")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Column("telephone")]
        [Display(Name = "Teléfono")]
        public string Telephone { get; set; }

        [Column("city")]
        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Column("country")]
        [Display(Name = "País")]
        public string Country { get; set; }

        [Column("latitude")]
        [Display(Name = "Latitud")]
        public double Latitude { get; set; }

        [Column("longitude")]
        [Display(Name = "Longitud")]
        public double Longitude { get; set; }

        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}
