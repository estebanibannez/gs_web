using Circon.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebApplication.Controllers;
using WebApplicationMod;

namespace WebApplication.Seguridad
{
    public class ProveedorAutenticacion : MembershipProvider
    {
        private static List<SessionUser> USERS = new List<SessionUser>();
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
    bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
            string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            try
            {                
                using (var db = new GSPSOEntities())
                {
                    var usu = db.Usu.Where(o => o.Sigla.Trim() == username.Trim() && o.pass.Trim() == password && o.uactivo=="s");
                    if (usu.Any())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex) {
                return false;
            }
            
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            USERS.RemoveAll(p => p.SessionExpires < DateTime.Now);
            try
            {
                var fn = USERS.FirstOrDefault(p => p.user.Sigla == username);
                if (fn != null)
                {
                    return fn.user == null ? null : new UsuarioMembership(fn.user, fn.roles, fn.clientUser);
                }
                else {
                    using (var db = new GSPSOEntities())
                    {
                        Usu user = db.Usu.FirstOrDefault(o => o.Sigla == username && o.uactivo=="s");
                        //var permisos = (from frm in db.GS_FRMS
                        //                join acc in db.GS_AccFrm on frm.ID equals acc.Frm
                        //                where acc.Usu == user.ID
                        //                select frm.WebGS.Trim()).ToList();

                        var permisos = (from per in db.Permisos_GS_WEB
                                        join para in db.Parametros_GS_WEB on per.ID_Parametro equals para.ID_Parametro
                                        where per.ID_Usu == user.ID
                                        select para.Controller).ToList();

                        var Clientes = (from clie in db.Clientes
                                    join per in db.Permisos on clie.ID equals per.Clie
                                    where clie.Activo == "S"
                                     && per.Usu == user.ID
                                        select clie ).ToList();

                        USERS.Add(new SessionUser() { user = user, roles = permisos, SessionExpires = DateTime.Now.AddMinutes(5), clientUser= Clientes });
                        return user == null ? null : new UsuarioMembership(user, permisos, Clientes);
                    }
                }
            }
            catch (Exception ex) {
                return null;
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordRetrieval { get; }
        public override bool EnablePasswordReset { get; }
        public override bool RequiresQuestionAndAnswer { get; }
        public override string ApplicationName { get; set; }
        public override int MaxInvalidPasswordAttempts { get; }
        public override int PasswordAttemptWindow { get; }
        public override bool RequiresUniqueEmail { get; }
        public override MembershipPasswordFormat PasswordFormat { get; }
        public override int MinRequiredPasswordLength { get; }
        public override int MinRequiredNonAlphanumericCharacters { get; }
        public override string PasswordStrengthRegularExpression { get; }
    }
}