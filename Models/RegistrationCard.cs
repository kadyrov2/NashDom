using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashDom.Models
{
    public class RegistrationCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;
        public DateTimeOffset? BirthDate { get; set; }
        public string BirthPlaceRegion { get; set; } = string.Empty;
        public string BirthPlaceDistrict { get; set; } = string.Empty;
        public string BirthPlaceCity { get; set; } = string.Empty;
        public string BirthPlaceVillage { get; set; } = string.Empty;
        public string ArrivedFromRegion { get; set; } = string.Empty;
        public string ArrivedFromDistrict { get; set; } = string.Empty;
        public string ArrivedFromCity { get; set; } = string.Empty;
        public string ArrivedFromStreet { get; set; } = string.Empty;
        public string ArrivedHouse { get; set; } = string.Empty;
        public string ArrivedBuilding { get; set; } = string.Empty;
        public string ArrivedApartment { get; set; } = string.Empty;
        public DateTimeOffset? ArrivedDate { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentSeries { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentIssuedBy { get; set; } = string.Empty;
        public DateTimeOffset? DocumentDate { get; set; }
        public string ResidenceLocality { get; set; } = string.Empty;
        public string ResidenceStreet { get; set; } = string.Empty;
        public string ResidenceHouse { get; set; } = string.Empty;
        public string ResidenceBuilding { get; set; } = string.Empty;
        public string ResidenceApartment { get; set; } = string.Empty;
        public string MilitaryNotes { get; set; } = string.Empty;
        public string RegistrationNote { get; set; } = string.Empty;
        public string DeregistrationNote { get; set; } = string.Empty;

        public int ApartmentId { get; set; }
        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }
    }
}