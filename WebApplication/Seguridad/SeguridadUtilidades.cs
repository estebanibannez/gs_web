using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutenticacionPersonalizada.Utilidades
{
    public class SeguridadUtilidades
    {
        public static String GetSha1(String texto)
        {
            var sha = SHA1.Create();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] datos;
            StringBuilder builder = new StringBuilder();
            datos = sha.ComputeHash(encoding.GetBytes(texto));
            for (int i = 0; i < datos.Length; i++)
            {
                builder.AppendFormat("{0:x2}", datos[i]);
            }

            return builder.ToString();
        }

        public static byte[] GetKey(String txt)
        {
            return new PasswordDeriveBytes(txt, null).GetBytes(32);
        }
        public static string Cifrar(String contenido, String clave)
        {
            var encoding = new UTF8Encoding();
            var cripto = new RijndaelManaged();

            byte[] cifrado;
            byte[] retorno;
            byte[] key = GetKey(clave);

            cripto.Key = key;
            cripto.GenerateIV();
            byte[] aEncriptar = encoding.GetBytes(contenido);

            cifrado = cripto.CreateEncryptor().
                 TransformFinalBlock(aEncriptar, 0, aEncriptar.Length);

            retorno = new byte[cripto.IV.Length + cifrado.Length];

            cripto.IV.CopyTo(retorno, 0);
            cifrado.CopyTo(retorno, cripto.IV.Length);

            return Convert.ToBase64String(retorno);
        }
        public static String DesCifrar(byte[] contenido, String clave)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            var cripto = new RijndaelManaged();
            var ivTemp = new byte[cripto.IV.Length];

            var key = GetKey(clave);
            var cifrado = new byte[contenido.Length - ivTemp.Length];

            cripto.Key = key;

            Array.Copy(contenido, ivTemp, ivTemp.Length);
            Array.Copy(contenido, ivTemp.Length, cifrado, 0, cifrado.Length);

            cripto.IV = ivTemp;
            var descifrado = cripto.CreateDecryptor().
                TransformFinalBlock(cifrado, 0, cifrado.Length);
            return encoding.GetString(descifrado);
        }

        public static string Encriptar(string texto)
        {
            try
            {

                string key = "A1e2D3j4F5H6"; //llave para encriptar datos

                byte[] keyArray;

                byte[] Arreglo_a_Cifrar = UTF8Encoding.UTF8.GetBytes(texto);

                //Se utilizan las clases de encriptación MD5

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                //Algoritmo TripleDES
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();

                byte[] ArrayResultado = cTransform.TransformFinalBlock(Arreglo_a_Cifrar, 0, Arreglo_a_Cifrar.Length);

                tdes.Clear();

                //se regresa el resultado en forma de una cadena
                texto = Convert.ToBase64String(ArrayResultado, 0, ArrayResultado.Length);

            }
            catch (Exception)
            {

            }
            return texto;
        }


        public static string Desencriptar(string textoEncriptado)
        {
            try
            {
                string key = "A1e2D3j4F5H6";
                byte[] keyArray;
                byte[] Array_a_Descifrar = Convert.FromBase64String(textoEncriptado);

                //algoritmo MD5
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();

                byte[] resultArray = cTransform.TransformFinalBlock(Array_a_Descifrar, 0, Array_a_Descifrar.Length);

                tdes.Clear();
                textoEncriptado = UTF8Encoding.UTF8.GetString(resultArray);

            }
            catch (Exception e)
            {

            }
            return textoEncriptado;
        }


    }
}
