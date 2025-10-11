using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace NutriAI.Models
{
    public class UserInfo
    {
        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(10, 120, ErrorMessage = "La edad debe estar entre 10 y 120 años.")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg.")]
        public float Peso { get; set; }

        [Required(ErrorMessage = "La altura es obligatoria.")]
        [Range(100, 250, ErrorMessage = "La altura debe estar entre 100 y 250 cm.")]
        public float Altura { get; set; }

        [StringLength(100, ErrorMessage = "La preferencia no puede superar los 100 caracteres.")]
        public string PreferenciaAlimenticia { get; set; }
    }
}
