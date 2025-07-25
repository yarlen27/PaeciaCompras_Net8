﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCore.Identity.MongoDB.Models
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local", Justification = "MongoDB serialization needs private setters")]
    public abstract class MongoUserContactRecord : IEquatable<MongoUserEmail>
    {
        protected MongoUserContactRecord(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            Value = value;
        }

        public string Value { get; private set; }
        public ConfirmationOccurrence ConfirmationRecord { get; private set; }

        public bool IsConfirmed()
        {
            return ConfirmationRecord != null;
        }

        public void SetConfirmed()
        {
            SetConfirmed(new ConfirmationOccurrence());
        }

        public void SetConfirmed(ConfirmationOccurrence confirmationRecord)
        {
            if (ConfirmationRecord == null)
            {
                ConfirmationRecord = confirmationRecord;
            }
        }

        public void SetUnconfirmed()
        {
            ConfirmationRecord = null;
        }

        public bool Equals(MongoUserEmail other)
        {
            return other.Value.Equals(Value);
        }
    }
}
