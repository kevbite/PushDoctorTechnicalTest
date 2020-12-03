using System.Collections.Generic;
using System.Linq;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service
{
    public class EmailValidator
    {
        public bool ValidEmailAddress(string email, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            var emailUsernameDomainSplit = email.Split("@");

            if (emailUsernameDomainSplit.Length != 2 || emailUsernameDomainSplit.Any(x => x.Length == 0))
            {
                errors.Add("Email must be a valid email address");
            }
            
            if (errors.Any())
            {
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }
    }
}