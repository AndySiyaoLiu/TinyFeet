namespace TinyFeet.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using TinyFeet.Interfaces.Command;

    public class CommandResult : ICommandResult
    {
        #region Fields

        private bool success;
        private List<string> errors;

        #endregion Fields

        #region Constructor

        public CommandResult(bool success = true, string recordId = "", string[] errors = null)
        {
            this.errors = errors?.ToList();
            this.success = success;
            this.RecordId = recordId;
        }

        #endregion Constructor

        #region Properties

        public bool Success => this.success;
        public string[] Errors => this.errors?.ToArray();
        public string RecordId { get; private set; }

        #endregion Properties

        #region Methods

        public void Fail(Exception ex)
        {
            this.Fail(ex.GetInnerMostMessage());
        }

        public void Fail(string errorMessage)
        {
            this.success = false;

            if (this.errors == null)
            {
                this.errors = new List<string>() { errorMessage };
            }
            else
            {
                if (!this.errors.Exists(x => x == errorMessage))
                {
                    this.errors.Add(errorMessage);
                }
            }
        }

        public void Fail(IEnumerable<Exception> validationResults)
        {
            foreach (var validationResult in validationResults)
            {
                this.Fail(validationResult.Message);
            }
        }

        public void SetRecord(string recordId)
        {
            this.RecordId = recordId;
        }

        public void SetRecord(Guid recordId)
        {
            this.RecordId = recordId.ToString();
        }

        public void SetRecord(int recordId)
        {
            this.RecordId = recordId.ToString();
        }

        #endregion Methods
    }
}
