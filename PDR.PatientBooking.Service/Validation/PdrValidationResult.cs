using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.Validation
{
    public class PdrValidationResult
    {
        public bool PassedValidation => !Errors.Any();
        public List<string> Errors { get; set; } 

        public PdrValidationResult(bool passedValidation)
        {
            Errors = new List<string>();
        }

        public PdrValidationResult(bool passedValidation, string error)
        {
            Errors = new List<string> { error };
        }

        public PdrValidationResult(bool passedValidation, List<string> errors)
        {
            Errors = errors;
        }
    }
}
