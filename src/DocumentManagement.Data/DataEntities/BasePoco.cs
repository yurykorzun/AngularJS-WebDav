using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace DocumentManagement.Data.DataEntities
{
    public abstract class BasePoco
    {
        protected BasePoco()
        {
            Errors = new List<DbEntityValidationResult>();
        }

        public IEnumerable<DbEntityValidationResult> Errors { get; set; }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }
    }
}