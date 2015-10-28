using ImageViewer.Infrastructures;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Serialization;

namespace ImageViewer.Models
{
    public class PixivAccount
    {
        [XmlElement(IsNullable = true)]
        public string Id { get; set; }

        [XmlIgnore]
        public SecureString Password { get; set; }

        [XmlIgnore]
        public unsafe string RawPassword
        {
            get
            {
                var strPtr = IntPtr.Zero;
                try {
                    strPtr = Marshal.SecureStringToGlobalAllocUnicode(Password);
                    return Marshal.PtrToStringUni(strPtr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(strPtr);
                }
            }
            set
            {
                fixed(char* strPtr = value)
                {
                    Password = new SecureString(strPtr, value.Length);
                }
            }
        }

        [XmlElement(IsNullable = true)]
        public string EncryptedPassword
        {
            get
            {
                return Encrypt.EncryptString(RawPassword);
            }
            set
            {
                RawPassword = Encrypt.DecryptString(value);
            }
        }
    }
}
