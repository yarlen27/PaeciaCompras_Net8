using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AspNetCore.Identity.MongoDB.Models;
using System.Security.Claims;
using MongoDB.Bson;

namespace AspNetCore.Identity.MongoDB
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local", Justification = "MongoDB serialization needs private setters")]
    public class MongoIdentityUser
    {
        private List<MongoUserClaim> _claims;
        private List<MongoUserLogin> _logins;

       
        public MongoIdentityUser(string userName, string email) : this(userName)
        {
            if (email != null)
            {
                Email = new MongoUserEmail(email);
            }
        }

        public MongoIdentityUser(string userName, string email, string nombre, string apellido, string identificacion, List<ClientRol> client) : this(userName)
        {
            if (email != null)
            {
                Email = new MongoUserEmail(email);
            }
            Nombre = nombre;
            Apellido = apellido;
            Identificacion= identificacion;

            Client = client;
        }

        public MongoIdentityUser(string userName, MongoUserEmail email) : this(userName)
        {
            if (email != null)
            {
                Email = email;
            }
        }

        public MongoIdentityUser(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Id = ObjectId.GenerateNewId().ToString();
            UserName = userName;
            CreatedOn = new Occurrence();

            EnsureClaimsIsSet();
            EnsureLoginsIsSet();
        }

        public string Id { get; private set; }
        public string UserName { get; private set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Identificacion { get; set; }
        public List<ClientRol> Client { get; set; }

        public Guid imageId { get; set; }
        public bool Super { get; set; }
        public string NormalizedUserName { get; private set; }
        public MongoUserEmail Email { get; private set; }

        public MongoUserPhoneNumber PhoneNumber { get; private set; }
        public string PasswordHash { get; private set; }
        public string SecurityStamp { get; private set; }
        public bool IsTwoFactorEnabled { get; private set; }

        public IEnumerable<MongoUserClaim> Claims
        {
            get
            {
                EnsureClaimsIsSet();
                return _claims;
            }

            // ReSharper disable once UnusedMember.Local, MongoDB serialization needs private setters
            private set
            {
                EnsureClaimsIsSet();
                if (value != null)
                {
                    _claims.AddRange(value);
                }
            }
        }

        public IEnumerable<MongoUserLogin> Logins
        {
            get
            {
                EnsureLoginsIsSet();
                return _logins;
            }

            // ReSharper disable once UnusedMember.Local, MongoDB serialization needs private setters
            private set
            {
                EnsureLoginsIsSet();
                if (value != null)
                {
                    _logins.AddRange(value);
                }
            }
        }

        public int AccessFailedCount { get; private set; }
        public bool IsLockoutEnabled { get; private set; }
        public FutureOccurrence LockoutEndDate { get; private set; }

        public Occurrence CreatedOn { get; private set; }
        public Occurrence DeletedOn { get; private set; }

        public virtual void EnableTwoFactorAuthentication()
        {
            IsTwoFactorEnabled = true;
        }

        public virtual void DisableTwoFactorAuthentication()
        {
            IsTwoFactorEnabled = false;
        }

        public virtual void EnableLockout()
        {
            IsLockoutEnabled = true;
        }

        public virtual void DisableLockout()
        {
            IsLockoutEnabled = false;
        }

        public virtual void SetEmail(string email)
        {
            var mongoUserEmail = new MongoUserEmail(email);
            SetEmail(mongoUserEmail);
        }

        public virtual void SetEmail(MongoUserEmail mongoUserEmail)
        {
            Email = mongoUserEmail;
        }

        public virtual void SetNormalizedUserName(string normalizedUserName)
        {
            if (normalizedUserName == null)
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            NormalizedUserName = normalizedUserName;
        }

        public virtual void SetPhoneNumber(string phoneNumber)
        {
            var mongoUserPhoneNumber = new MongoUserPhoneNumber(phoneNumber);
            SetPhoneNumber(mongoUserPhoneNumber);
        }

        public virtual void SetPhoneNumber(MongoUserPhoneNumber mongoUserPhoneNumber)
        {
            PhoneNumber = mongoUserPhoneNumber;
        }

        public virtual void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public virtual void SetSecurityStamp(string securityStamp)
        {
            SecurityStamp = securityStamp;
        }

        public virtual void SetAccessFailedCount(int accessFailedCount)
        {
            AccessFailedCount = accessFailedCount;
        }

        public virtual void ResetAccessFailedCount()
        {
            AccessFailedCount = 0;
        }

        public virtual void LockUntil(DateTime lockoutEndDate)
        {
            LockoutEndDate = new FutureOccurrence(lockoutEndDate);
        }

        public virtual void AddClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            AddClaim(new MongoUserClaim(claim));
        }

        public virtual void AddClaim(MongoUserClaim mongoUserClaim)
        {
            if (mongoUserClaim == null)
            {
                throw new ArgumentNullException(nameof(mongoUserClaim));
            }

            _claims.Add(mongoUserClaim);
        }

        public virtual void RemoveClaim(MongoUserClaim mongoUserClaim)
        {
            if (mongoUserClaim == null)
            {
                throw new ArgumentNullException(nameof(mongoUserClaim));
            }

            _claims.Remove(mongoUserClaim);
        }

        public virtual void AddLogin(MongoUserLogin mongoUserLogin)
        {
            if (mongoUserLogin == null)
            {
                throw new ArgumentNullException(nameof(mongoUserLogin));
            }

            _logins.Add(mongoUserLogin);
        }

        public virtual void RemoveLogin(MongoUserLogin mongoUserLogin)
        {
            if (mongoUserLogin == null)
            {
                throw new ArgumentNullException(nameof(mongoUserLogin));
            }

            _logins.Remove(mongoUserLogin);
        }

        public void Delete()
        {
            if (DeletedOn != null)
            {
                throw new InvalidOperationException($"User '{Id}' has already been deleted.");
            }

            DeletedOn = new Occurrence();
        }

        private void EnsureClaimsIsSet()
        {
            if (_claims == null)
            {
                _claims = new List<MongoUserClaim>();
            }
        }

        private void EnsureLoginsIsSet()
        {
            if (_logins == null)
            {
                _logins = new List<MongoUserLogin>();
            }
        }
    }   
}