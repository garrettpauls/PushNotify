using System;
using Template10.Interfaces.Validation;
using Template10.Validation;

namespace PushNotify.Models
{
    public abstract class ValidatableModel<TSelf> : ValidatableModelBase
        where TSelf : ValidatableModel<TSelf>, IValidatableModel
    {
        private Action<TSelf> mValidator;

        public new Action<TSelf> Validator
        {
            get => mValidator;
            set
            {
                mValidator = value;
                base.Validator = (model) =>
                {
                    if (model is TSelf self)
                    {
                        value(self);
                    }
                };
            }
        }
    }
}
