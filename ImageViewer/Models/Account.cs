using ImageViewer.Infrastructures;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Serialization;

namespace ImageViewer.Models
{
    public class Account
    {
        public string Id { get; set; }

        [XmlIgnore]
        private SecureString Password { get; set; }

        [XmlIgnore]
        public unsafe string RawPassword
        {
            get
            {
                var strPtr = IntPtr.Zero;
                try
                {
                    strPtr = Marshal.SecureStringToGlobalAllocUnicode(Password);
                    return Marshal.PtrToStringUni(strPtr);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(strPtr);
                }
            }
            set
            {
                fixed (char* strPtr = value)
                {
                    Password = new SecureString(strPtr, value.Length);
                }
            }
        }

        public string EncryptedPassword
        {
            get { return RawPassword != null ? Encrypt.EncryptString(RawPassword) : null; }
            set { RawPassword = Encrypt.DecryptString(value); }
        }
    }

    public class PixivAccount : Account
    {
    }

    public class NijieAccount : Account
    {
    }

    public class NicovideoAccount : Account
    {
    }
}